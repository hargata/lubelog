using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using CarCareTracker.Logic;
using CarCareTracker.Middleware;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;

namespace CarCareTracker.Tests
{
    /// <summary>A fake identity provider: signs tokens and serves its JWKS over a fake HttpClient.</summary>
    internal sealed class FakeIdp : HttpMessageHandler, IHttpClientFactory
    {
        public const string Issuer = "https://id.example.com/application/o/api-vehicles/";
        public const string Audience = "api-vehicles";
        public RsaSecurityKey Key { get; private set; } = NewKey("k1");
        public int JwksFetches { get; private set; }

        private static RsaSecurityKey NewKey(string kid) => new RsaSecurityKey(RSA.Create(2048)) { KeyId = kid };
        public void Rotate(string kid) => Key = NewKey(kid);

        public string Token(Action<Dictionary<string, object>>? tweak = null, SecurityKey? signWith = null)
        {
            var claims = new Dictionary<string, object>
            {
                ["iss"] = Issuer,
                ["aud"] = Audience,
                ["sub"] = "abc",
                ["email"] = "member@example.com",
                ["email_verified"] = true,
            };
            tweak?.Invoke(claims);
            var descriptor = new SecurityTokenDescriptor
            {
                Claims = claims,
                Expires = claims.TryGetValue("exp", out var exp) ? DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(exp)).UtcDateTime : DateTime.UtcNow.AddMinutes(10),
                NotBefore = claims.ContainsKey("exp") ? DateTime.UtcNow.AddHours(-2) : null,
                IssuedAt = claims.ContainsKey("exp") ? DateTime.UtcNow.AddHours(-2) : null,
                SigningCredentials = new SigningCredentials(signWith ?? Key, SecurityAlgorithms.RsaSha256),
            };
            return new JsonWebTokenHandler().CreateToken(descriptor);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            JwksFetches++;
            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(new RsaSecurityKey(Key.Rsa.ExportParameters(false)) { KeyId = Key.KeyId });
            jwk.Use = "sig";
            jwk.Alg = "RS256";
            var json = System.Text.Json.JsonSerializer.Serialize(new { keys = new[] { new { kty = jwk.Kty, kid = jwk.Kid, use = jwk.Use, alg = jwk.Alg, n = jwk.N, e = jwk.E } } });
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
        }
        public HttpClient CreateClient(string name) => new HttpClient(this, disposeHandler: false);
    }

    public class ApiJwtValidatorTests
    {
        private readonly FakeIdp _idp = new FakeIdp();

        private ApiJwtValidator Validator(Dictionary<string, string?>? extra = null)
        {
            var settings = new Dictionary<string, string?>
            {
                ["APIAuthJWT:Issuer"] = FakeIdp.Issuer,
                ["APIAuthJWT:Audience"] = FakeIdp.Audience,
            };
            foreach (var kv in extra ?? new()) settings[kv.Key] = kv.Value;
            var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            return new ApiJwtValidator(config, _idp, new MemoryCache(new MemoryCacheOptions()), NullLogger<ApiJwtValidator>.Instance);
        }

        [Fact]
        public async Task ValidTokenReturnsEmail() =>
            Assert.Equal("member@example.com", await Validator().ValidateAsync(_idp.Token()));

        [Fact]
        public async Task DisabledUnlessIssuerAndAudienceSet()
        {
            var v = Validator(new() { ["APIAuthJWT:Audience"] = "" });
            Assert.False(v.Enabled);
            Assert.Null(await v.ValidateAsync(_idp.Token()));
        }

        [Fact]
        public async Task WrongAudienceRejected() =>
            Assert.Null(await Validator().ValidateAsync(_idp.Token(c => c["aud"] = "api-things")));

        [Fact]
        public async Task WrongIssuerRejected() =>
            Assert.Null(await Validator().ValidateAsync(_idp.Token(c => c["iss"] = "https://elsewhere.example/")));

        [Fact]
        public async Task ExpiredRejected() =>
            Assert.Null(await Validator().ValidateAsync(_idp.Token(c => c["exp"] = DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeSeconds())));

        [Fact]
        public async Task ForeignSignatureRejected() =>
            Assert.Null(await Validator().ValidateAsync(_idp.Token(signWith: new RsaSecurityKey(RSA.Create(2048)) { KeyId = "k1" })));

        [Fact]
        public async Task UnverifiedOrMissingEmailRejected()
        {
            var v = Validator();
            Assert.Null(await v.ValidateAsync(_idp.Token(c => c["email_verified"] = false)));
            Assert.Null(await v.ValidateAsync(_idp.Token(c => c.Remove("email_verified"))));
            Assert.Null(await v.ValidateAsync(_idp.Token(c => c.Remove("email"))));
        }

        [Fact]
        public async Task UnverifiedEmailAllowedWhenConfigured() =>
            Assert.NotNull(await Validator(new() { ["APIAuthJWT:RequireVerifiedEmail"] = "false" }).ValidateAsync(_idp.Token(c => c.Remove("email_verified"))));

        [Fact]
        public async Task NotAJwtRejected() =>
            Assert.Null(await Validator().ValidateAsync("not-a-jwt"));

        [Fact]
        public async Task KeysAreCachedAndRefreshedOnRotation()
        {
            var v = Validator(new() { ["APIAuthJWT:JwksMinRefreshSeconds"] = "0" });
            Assert.NotNull(await v.ValidateAsync(_idp.Token()));
            Assert.NotNull(await v.ValidateAsync(_idp.Token()));
            Assert.Equal(1, _idp.JwksFetches);
            _idp.Rotate("k2");
            Assert.NotNull(await v.ValidateAsync(_idp.Token()));
            Assert.Equal(2, _idp.JwksFetches);
        }

        [Fact]
        public async Task UnknownKeyIdRefetchIsRateLimited()
        {
            var v = Validator();
            Assert.NotNull(await v.ValidateAsync(_idp.Token()));
            var stranger = new RsaSecurityKey(RSA.Create(2048)) { KeyId = "nope" };
            Assert.Null(await v.ValidateAsync(_idp.Token(signWith: stranger)));
            Assert.Null(await v.ValidateAsync(_idp.Token(signWith: stranger)));
            Assert.Equal(1, _idp.JwksFetches);
        }
    }

    public class AuthenBearerTests
    {
        private readonly FakeIdp _idp = new FakeIdp();
        private readonly ILoginLogic _login = Substitute.For<ILoginLogic>();

        public AuthenBearerTests()
        {
            _login.ValidateOpenIDUser(Arg.Any<LoginModel>()).Returns(new UserData());
            _login.ValidateOpenIDUser(Arg.Is<LoginModel>(m => m.EmailAddress == "member@example.com"))
                .Returns(new UserData { Id = 7, UserName = "member", EmailAddress = "member@example.com" });
        }

        private async Task<AuthenticateResult> Authenticate(string path, string authorization, bool enabled = true)
        {
            var settings = new Dictionary<string, string?> { ["EnableAuth"] = "true" };
            if (enabled)
            {
                settings["APIAuthJWT:Issuer"] = FakeIdp.Issuer;
                settings["APIAuthJWT:Audience"] = FakeIdp.Audience;
            }
            var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
            var validator = new ApiJwtValidator(config, _idp, new MemoryCache(new MemoryCacheOptions()), NullLogger<ApiJwtValidator>.Instance);
            var context = new DefaultHttpContext();
            context.Request.Path = path;
            context.Request.Headers.Authorization = authorization;
            var accessor = new HttpContextAccessor { HttpContext = context };
            var options = Substitute.For<IOptionsMonitor<AuthenticationSchemeOptions>>();
            options.Get(Arg.Any<string>()).Returns(new AuthenticationSchemeOptions());
            var handler = new Authen(options, UrlEncoder.Default, NullLoggerFactory.Instance, config, _login,
                new EphemeralDataProtectionProvider(), accessor, validator);
            await handler.InitializeAsync(new AuthenticationScheme("AuthN", null, typeof(Authen)), context);
            return await handler.AuthenticateAsync();
        }

        [Fact]
        public async Task BearerOnApiMapsToExistingUser()
        {
            var result = await Authenticate("/api/vehicles", "Bearer " + _idp.Token());
            Assert.True(result.Succeeded);
            var user = result.Principal!;
            Assert.Equal("member", user.Identity!.Name);
            Assert.Equal("7", user.FindFirstValue(ClaimTypes.NameIdentifier));
            Assert.True(user.IsInRole("APIAuth"));
            Assert.True(user.IsInRole("JWTAuth"));
            Assert.False(user.IsInRole("APIKeyAuth"));
            Assert.False(user.IsInRole(nameof(UserData.IsRootUser)));
        }

        [Fact]
        public async Task UnknownPersonIsNotRegistered()
        {
            var result = await Authenticate("/api/vehicles", "Bearer " + _idp.Token(c => c["email"] = "stranger@example.com"));
            Assert.False(result.Succeeded);
            _login.DidNotReceiveWithAnyArgs().RegisterOpenIdUser(default!);
        }

        [Fact]
        public async Task BearerOutsideApiRejected() =>
            Assert.False((await Authenticate("/Vehicle/Index", "Bearer " + _idp.Token())).Succeeded);

        [Fact]
        public async Task BearerRejectedWhenNotConfigured() =>
            Assert.False((await Authenticate("/api/vehicles", "Bearer " + _idp.Token(), enabled: false)).Succeeded);

        [Fact]
        public async Task BadBearerRejected() =>
            Assert.False((await Authenticate("/api/vehicles", "Bearer " + _idp.Token(c => c["aud"] = "other"))).Succeeded);
    }
}

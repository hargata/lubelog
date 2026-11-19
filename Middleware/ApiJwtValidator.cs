using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CarCareTracker.Middleware
{
    /// <summary>
    /// Validates bearer JWTs on the API that were issued by an external identity provider
    /// (for example a token-exchange flow in Authentik or Keycloak). Off unless APIAuthJWT:Issuer
    /// and APIAuthJWT:Audience are configured. Signature (JWKS), issuer, audience and lifetime are
    /// all checked; the caller maps the returned email to an EXISTING user (users are never created).
    /// </summary>
    public interface IApiJwtValidator
    {
        bool Enabled { get; }
        /// <summary>Returns the token's email address, or null if the token is not acceptable.</summary>
        Task<string?> ValidateAsync(string token);
    }
    public class ApiJwtValidator : IApiJwtValidator
    {
        private const string JwksCacheKey = "APIAuthJWT_JWKS";
        private static readonly TimeSpan JwksLifetime = TimeSpan.FromMinutes(10);
        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _jwksUrl;
        private readonly string _emailClaim;
        private readonly bool _requireVerifiedEmail;
        private readonly TimeSpan _jwksMinRefresh;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ApiJwtValidator> _logger;
        private readonly JsonWebTokenHandler _tokenHandler = new JsonWebTokenHandler();
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);
        private DateTime _lastRefresh = DateTime.MinValue;
        public ApiJwtValidator(IConfiguration configuration, IHttpClientFactory httpClientFactory, IMemoryCache cache, ILogger<ApiJwtValidator> logger)
        {
            _issuer = configuration["APIAuthJWT:Issuer"] ?? string.Empty;
            _audience = configuration["APIAuthJWT:Audience"] ?? string.Empty;
            _jwksUrl = configuration["APIAuthJWT:JwksURL"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(_jwksUrl) && !string.IsNullOrWhiteSpace(_issuer))
            {
                //Authentik and most providers publish keys at <issuer>/jwks/ ; set APIAuthJWT:JwksURL otherwise.
                _jwksUrl = $"{_issuer.TrimEnd('/')}/jwks/";
            }
            _emailClaim = configuration["APIAuthJWT:EmailClaim"] ?? "email";
            _requireVerifiedEmail = bool.Parse(configuration["APIAuthJWT:RequireVerifiedEmail"] ?? "true");
            //an unknown key id triggers a JWKS refetch at most this often.
            _jwksMinRefresh = TimeSpan.FromSeconds(int.Parse(configuration["APIAuthJWT:JwksMinRefreshSeconds"] ?? "30"));
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _logger = logger;
        }
        public bool Enabled => !string.IsNullOrWhiteSpace(_issuer) && !string.IsNullOrWhiteSpace(_audience);
        public static bool LooksLikeJwt(string token) => token.StartsWith("ey") && token.Count(x => x == '.') == 2;
        public async Task<string?> ValidateAsync(string token)
        {
            if (!Enabled || !LooksLikeJwt(token))
            {
                return null;
            }
            try
            {
                var keys = await GetSigningKeys(false);
                var kid = _tokenHandler.ReadJsonWebToken(token).Kid;
                if (!string.IsNullOrWhiteSpace(kid) && !keys.Any(x => x.KeyId == kid))
                {
                    //keys may have rotated, fetch them again (rate limited).
                    keys = await GetSigningKeys(true);
                }
                var result = await ValidateWithKeys(token, keys);
                if (!result.IsValid)
                {
                    _logger.LogInformation($"API bearer token rejected: {result.Exception?.GetType().Name}");
                    return null;
                }
                var claims = result.Claims;
                if (_requireVerifiedEmail && !(claims.TryGetValue("email_verified", out var verified) && verified is bool isVerified && isVerified))
                {
                    _logger.LogInformation("API bearer token rejected: email not verified");
                    return null;
                }
                if (claims.TryGetValue(_emailClaim, out var email) && email is string emailAddress && !string.IsNullOrWhiteSpace(emailAddress))
                {
                    return emailAddress;
                }
                _logger.LogInformation("API bearer token rejected: no email claim");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"API bearer token could not be validated: {ex.Message}");
                return null;
            }
        }
        private Task<TokenValidationResult> ValidateWithKeys(string token, IList<SecurityKey> keys)
        {
            var tokenValidationParams = new TokenValidationParameters
            {
                ValidIssuer = _issuer,
                ValidateIssuer = true,
                ValidAudience = _audience,
                ValidateAudience = true,
                RequireAudience = true,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                IssuerSigningKeys = keys,
                ValidateIssuerSigningKey = true
            };
            return _tokenHandler.ValidateTokenAsync(token, tokenValidationParams);
        }
        private async Task<IList<SecurityKey>> GetSigningKeys(bool forceRefresh)
        {
            if (!forceRefresh && _cache.TryGetValue(JwksCacheKey, out IList<SecurityKey>? cached) && cached != null)
            {
                return cached;
            }
            await _refreshLock.WaitAsync();
            try
            {
                if (_cache.TryGetValue(JwksCacheKey, out cached) && cached != null && (!forceRefresh || DateTime.UtcNow - _lastRefresh < _jwksMinRefresh))
                {
                    return cached;
                }
                var httpClient = _httpClientFactory.CreateClient();
                var jwksData = await httpClient.GetStringAsync(_jwksUrl);
                var signingKeys = new JsonWebKeySet(jwksData).GetSigningKeys();
                _cache.Set(JwksCacheKey, signingKeys, JwksLifetime);
                _lastRefresh = DateTime.UtcNow;
                return signingKeys;
            }
            finally
            {
                _refreshLock.Release();
            }
        }
    }
}

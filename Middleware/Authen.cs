using CarCareTracker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CarCareTracker.Middleware
{
    public class Authen : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private IHttpContextAccessor _httpContext;
        private IDataProtector _dataProtector;
        private bool enableAuth;
        public Authen(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            UrlEncoder encoder,
            ILoggerFactory logger,
            IConfiguration configuration,
            IDataProtectionProvider securityProvider,
            IHttpContextAccessor httpContext) : base(options, logger, encoder)
        {
            _httpContext = httpContext;
            _dataProtector = securityProvider.CreateProtector("login");
            enableAuth = bool.Parse(configuration["EnableAuth"]);
        }
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!enableAuth)
            {
                //generate a fake user ticket to go with it lol.
                var appIdentity = new ClaimsIdentity("Custom");
                var userIdentity = new List<Claim>
                {
                    new(ClaimTypes.Name, "admin")
                };
                appIdentity.AddClaims(userIdentity);
                AuthenticationTicket ticket = new AuthenticationTicket(new ClaimsPrincipal(appIdentity), this.Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            else
            {
                //auth is enabled by user, we will have to authenticate the user via a ticket retrieved from the auth cookie.
                var access_token = _httpContext.HttpContext.Request.Cookies["ACCESS_TOKEN"];
                if (string.IsNullOrWhiteSpace(access_token))
                {
                    return AuthenticateResult.Fail("Cookie is invalid or does not exist.");
                }
                else
                {
                    //decrypt the access token.
                    var decryptedCookie = _dataProtector.Unprotect(access_token);
                    AuthCookie authCookie = JsonSerializer.Deserialize<AuthCookie>(decryptedCookie);
                    if (authCookie != null)
                    {
                        //validate auth cookie
                        if (authCookie.ExpiresOn < DateTime.Now)
                        {
                            //if cookie is expired
                            return AuthenticateResult.Fail("Expired credentials");
                        }
                        else if (authCookie.Id == default || string.IsNullOrWhiteSpace(authCookie.UserName))
                        {
                            return AuthenticateResult.Fail("Corrupted credentials");
                        }
                        else
                        {
                            var appIdentity = new ClaimsIdentity("Custom");
                            var userIdentity = new List<Claim>
                            {
                                new(ClaimTypes.Name, authCookie.UserName)
                            };
                            appIdentity.AddClaims(userIdentity);
                            AuthenticationTicket ticket = new AuthenticationTicket(new ClaimsPrincipal(appIdentity), this.Scheme.Name);
                            return AuthenticateResult.Success(ticket);
                        }
                    }
                }
                return AuthenticateResult.Fail("Invalid credentials");
            }
        }
        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.Redirect("/Login/Index");
            return Task.CompletedTask;
        }
    }
}

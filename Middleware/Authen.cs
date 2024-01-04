using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace CarCareTracker.Middleware
{
    public class Authen : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private IHttpContextAccessor _httpContext;
        private bool enableAuth;
        public Authen(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            UrlEncoder encoder,
            ILoggerFactory logger,
            IConfiguration configuration, 
            IHttpContextAccessor httpContext): base(options, logger, encoder)
        {
            _httpContext = httpContext;
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
            } else
            {
                //auth is enabled by user, we will have to authenticate the user via a ticket.
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

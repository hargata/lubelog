using CarCareTracker.Logic;
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
        private ILoginLogic _loginLogic;
        private bool enableAuth;
        public Authen(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            UrlEncoder encoder,
            ILoggerFactory logger,
            IConfiguration configuration,
            ILoginLogic loginLogic,
            IDataProtectionProvider securityProvider,
            IHttpContextAccessor httpContext) : base(options, logger, encoder)
        {
            _httpContext = httpContext;
            _dataProtector = securityProvider.CreateProtector("login");
            _loginLogic = loginLogic;
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
                    new(ClaimTypes.Name, "admin"),
                    new(ClaimTypes.NameIdentifier, "-1"),
                    new(ClaimTypes.Role, nameof(UserData.IsRootUser))
                };
                appIdentity.AddClaims(userIdentity);
                AuthenticationTicket ticket = new AuthenticationTicket(new ClaimsPrincipal(appIdentity), this.Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            else
            {
                //auth is enabled by user, we will have to authenticate the user via a ticket retrieved from the auth cookie.
                var access_token = _httpContext.HttpContext.Request.Cookies["ACCESS_TOKEN"];
                //auth using Basic Auth for API.
                var request_header = _httpContext.HttpContext.Request.Headers["Authorization"];
                if (string.IsNullOrWhiteSpace(access_token) && string.IsNullOrWhiteSpace(request_header))
                {
                    return AuthenticateResult.Fail("Cookie is invalid or does not exist.");
                }
                else if (!string.IsNullOrWhiteSpace(request_header))
                {
                    var cleanedHeader = request_header.ToString().Replace("Basic ", "").Trim();
                    byte[] data = Convert.FromBase64String(cleanedHeader);
                    string decodedString = System.Text.Encoding.UTF8.GetString(data);
                    var splitString = decodedString.Split(":");
                    if (splitString.Count() != 2)
                    {
                        return AuthenticateResult.Fail("Invalid credentials");
                    }
                    else
                    {
                        var userData = _loginLogic.ValidateUserCredentials(new LoginModel { UserName = splitString[0], Password = splitString[1] });
                        if (userData.Id != default)
                        {
                            var appIdentity = new ClaimsIdentity("Custom");
                            var userIdentity = new List<Claim>
                            {
                                new(ClaimTypes.Name, splitString[0]),
                                new(ClaimTypes.NameIdentifier, userData.Id.ToString())
                            };
                            if (userData.IsAdmin)
                            {
                                userIdentity.Add(new(ClaimTypes.Role, nameof(UserData.IsAdmin)));
                            }
                            if (userData.IsRootUser)
                            {
                                userIdentity.Add(new(ClaimTypes.Role, nameof(UserData.IsRootUser)));
                            }
                            appIdentity.AddClaims(userIdentity);
                            AuthenticationTicket ticket = new AuthenticationTicket(new ClaimsPrincipal(appIdentity), this.Scheme.Name);
                            return AuthenticateResult.Success(ticket);
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(access_token))
                {
                    try
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
                            else if (authCookie.UserData is null || authCookie.UserData.Id == default || string.IsNullOrWhiteSpace(authCookie.UserData.UserName))
                            {
                                return AuthenticateResult.Fail("Corrupted credentials");
                            }
                            else
                            {
                                var appIdentity = new ClaimsIdentity("Custom");
                                var userIdentity = new List<Claim>
                                {
                                    new(ClaimTypes.Name, authCookie.UserData.UserName),
                                    new(ClaimTypes.NameIdentifier, authCookie.UserData.Id.ToString())
                                };
                                if (authCookie.UserData.IsAdmin)
                                {
                                    userIdentity.Add(new(ClaimTypes.Role, nameof(UserData.IsAdmin)));
                                }
                                if (authCookie.UserData.IsRootUser)
                                {
                                    userIdentity.Add(new(ClaimTypes.Role, nameof(UserData.IsRootUser)));
                                }
                                appIdentity.AddClaims(userIdentity);
                                AuthenticationTicket ticket = new AuthenticationTicket(new ClaimsPrincipal(appIdentity), this.Scheme.Name);
                                return AuthenticateResult.Success(ticket);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return AuthenticateResult.Fail("Corrupted credentials");
                    }
                }
                return AuthenticateResult.Fail("Invalid credentials");
            }
        }
        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            if (Request.RouteValues.TryGetValue("controller", out object value))
            {
                if (value.ToString().ToLower() == "api")
                {
                    Response.StatusCode = 401;
                    return Task.CompletedTask;
                }
            }
            Response.Redirect("/Login/Index");
            return Task.CompletedTask;
        }
    }
}

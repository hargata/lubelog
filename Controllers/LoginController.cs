using CarCareTracker.Helper;
using CarCareTracker.Logic;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace CarCareTracker.Controllers
{
    public class LoginController : Controller
    {
        private IDataProtector _dataProtector;
        private ILoginLogic _loginLogic;
        private IConfigHelper _config;
        private readonly ILogger<LoginController> _logger;
        public LoginController(
            ILogger<LoginController> logger,
            IDataProtectionProvider securityProvider,
            ILoginLogic loginLogic,
            IConfigHelper config
            )
        {
            _dataProtector = securityProvider.CreateProtector("login");
            _logger = logger;
            _loginLogic = loginLogic;
            _config = config;
        }
        public IActionResult Index(string redirectURL = "")
        {
            return View(model: redirectURL);
        }
        public IActionResult Registration()
        {
            return View();
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        public IActionResult ResetPassword()
        {
            return View();
        }
        public IActionResult GetRemoteLoginLink()
        {
            var remoteAuthURL = _config.GetOpenIDConfig().RemoteAuthURL;
            return Json(remoteAuthURL);
        }
        public async Task<IActionResult> RemoteAuth(string code)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(code))
                {
                    //received code from OIDC provider
                    //create http client to retrieve user token from OIDC
                    var httpClient = new HttpClient();
                    var openIdConfig = _config.GetOpenIDConfig();
                    var httpParams = new List<KeyValuePair<string, string>>
                {
                     new KeyValuePair<string, string>("code", code),
                     new KeyValuePair<string, string>("grant_type", "authorization_code"),
                     new KeyValuePair<string, string>("client_id", openIdConfig.ClientId),
                     new KeyValuePair<string, string>("client_secret", openIdConfig.ClientSecret),
                     new KeyValuePair<string, string>("redirect_uri", openIdConfig.RedirectURL)
                };
                    var httpRequest = new HttpRequestMessage(HttpMethod.Post, openIdConfig.TokenURL)
                    {
                        Content = new FormUrlEncodedContent(httpParams)
                    };
                    var tokenResult = await httpClient.SendAsync(httpRequest).Result.Content.ReadAsStringAsync();
                    var userJwt = JsonSerializer.Deserialize<OpenIDResult>(tokenResult)?.id_token ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(userJwt))
                    {
                        //validate JWT token
                        var tokenParser = new JwtSecurityTokenHandler();
                        var parsedToken = tokenParser.ReadJwtToken(userJwt);
                        var userEmailAddress = parsedToken.Claims.First(x => x.Type == "email").Value;
                        if (!string.IsNullOrWhiteSpace(userEmailAddress))
                        {
                            var userData = _loginLogic.ValidateOpenIDUser(new LoginModel() { EmailAddress = userEmailAddress });
                            if (userData.Id != default)
                            {
                                AuthCookie authCookie = new AuthCookie
                                {
                                    UserData = userData,
                                    ExpiresOn = DateTime.Now.AddDays(1)
                                };
                                var serializedCookie = JsonSerializer.Serialize(authCookie);
                                var encryptedCookie = _dataProtector.Protect(serializedCookie);
                                Response.Cookies.Append("ACCESS_TOKEN", encryptedCookie, new CookieOptions { Expires = new DateTimeOffset(authCookie.ExpiresOn) });
                                return new RedirectResult("/Home");
                            } else
                            {
                                _logger.LogInformation($"User {userEmailAddress} tried to login via OpenID but is not a registered user in LubeLogger.");
                                return View("OpenIDRegistration", model: userEmailAddress);
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new RedirectResult("/Login");
            }
            return new RedirectResult("/Login");
        }
        [HttpPost]
        public IActionResult Login(LoginModel credentials)
        {
            if (string.IsNullOrWhiteSpace(credentials.UserName) ||
                string.IsNullOrWhiteSpace(credentials.Password))
            {
                return Json(false);
            }
            //compare it against hashed credentials
            try
            {
                var userData = _loginLogic.ValidateUserCredentials(credentials);
                if (userData.Id != default)
                {
                    AuthCookie authCookie = new AuthCookie
                    {
                        UserData = userData,
                        ExpiresOn = DateTime.Now.AddDays(credentials.IsPersistent ? 30 : 1)
                    };
                    var serializedCookie = JsonSerializer.Serialize(authCookie);
                    var encryptedCookie = _dataProtector.Protect(serializedCookie);
                    Response.Cookies.Append("ACCESS_TOKEN", encryptedCookie, new CookieOptions { Expires = new DateTimeOffset(authCookie.ExpiresOn) });
                    return Json(true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on saving config file.");
            }
            return Json(false);
        }

        [HttpPost]
        public IActionResult Register(LoginModel credentials)
        {
            var result = _loginLogic.RegisterNewUser(credentials);
            return Json(result);
        }
        [HttpPost]
        public IActionResult RegisterOpenIdUser(LoginModel credentials)
        {
            var result = _loginLogic.RegisterOpenIdUser(credentials);
            if (result.Success)
            {
                var userData = _loginLogic.ValidateOpenIDUser(new LoginModel() { EmailAddress = credentials.EmailAddress });
                if (userData.Id != default)
                {
                    AuthCookie authCookie = new AuthCookie
                    {
                        UserData = userData,
                        ExpiresOn = DateTime.Now.AddDays(1)
                    };
                    var serializedCookie = JsonSerializer.Serialize(authCookie);
                    var encryptedCookie = _dataProtector.Protect(serializedCookie);
                    Response.Cookies.Append("ACCESS_TOKEN", encryptedCookie, new CookieOptions { Expires = new DateTimeOffset(authCookie.ExpiresOn) });
                }
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult RequestResetPassword(LoginModel credentials)
        {
            var result = _loginLogic.RequestResetPassword(credentials);
            return Json(result);
        }
        [HttpPost]
        public IActionResult PerformPasswordReset(LoginModel credentials)
        {
            var result = _loginLogic.ResetPasswordByUser(credentials);
            return Json(result);
        }
        [Authorize] //User must already be logged in to do this.
        [HttpPost]
        public IActionResult CreateLoginCreds(LoginModel credentials)
        {
            try
            {
                var result = _loginLogic.CreateRootUserCredentials(credentials);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on saving config file.");
            }
            return Json(false);
        }
        [Authorize]
        [HttpPost]
        public IActionResult DestroyLoginCreds()
        {
            try
            {
                var result = _loginLogic.DeleteRootUserCredentials();
                //destroy any login cookies.
                if (result)
                {
                    Response.Cookies.Delete("ACCESS_TOKEN");
                }
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on saving config file.");
            }
            return Json(false);
        }
        [Authorize]
        [HttpPost]
        public IActionResult LogOut()
        {
            Response.Cookies.Delete("ACCESS_TOKEN");
            return Json(true);
        }
    }
}

using CarCareTracker.Helper;
using CarCareTracker.Logic;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
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
            var remoteAuthConfig = _config.GetOpenIDConfig();
            if (remoteAuthConfig.DisableRegularLogin && !string.IsNullOrWhiteSpace(remoteAuthConfig.LogOutURL))
            {
                var generatedState = Guid.NewGuid().ToString().Substring(0, 8);
                remoteAuthConfig.State = generatedState;
                var pkceKeyPair = _loginLogic.GetPKCEChallengeCode();
                remoteAuthConfig.CodeChallenge = pkceKeyPair.Value;
                if (remoteAuthConfig.ValidateState)
                {
                    Response.Cookies.Append("OIDC_STATE", remoteAuthConfig.State, new CookieOptions { Expires = new DateTimeOffset(DateTime.Now.AddMinutes(5)) });
                }
                if (remoteAuthConfig.UsePKCE)
                {
                    Response.Cookies.Append("OIDC_VERIFIER", pkceKeyPair.Key, new CookieOptions { Expires = new DateTimeOffset(DateTime.Now.AddMinutes(5)) });
                }
                var remoteAuthURL = remoteAuthConfig.RemoteAuthURL;
                return Redirect(remoteAuthURL);
            }
            return View(model: redirectURL);
        }
        public IActionResult Registration(string token = "", string email = "")
        {
            if (_config.GetServerDisabledRegistration())
            {
                return RedirectToAction("Index");
            }
            var viewModel = new LoginModel
            {
                EmailAddress = string.IsNullOrWhiteSpace(email) ? string.Empty : email,
                Token = string.IsNullOrWhiteSpace(token) ? string.Empty : token
            };
            return View(viewModel);
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        public IActionResult ResetPassword(string token = "", string email = "")
        {
            var viewModel = new LoginModel
            {
                EmailAddress = string.IsNullOrWhiteSpace(email) ? string.Empty : email,
                Token = string.IsNullOrWhiteSpace(token) ? string.Empty : token
            };
            return View(viewModel);
        }
        public IActionResult GetRemoteLoginLink()
        {
            var remoteAuthConfig = _config.GetOpenIDConfig();
            var generatedState = Guid.NewGuid().ToString().Substring(0, 8);
            remoteAuthConfig.State = generatedState;
            var pkceKeyPair = _loginLogic.GetPKCEChallengeCode();
            remoteAuthConfig.CodeChallenge = pkceKeyPair.Value;
            if (remoteAuthConfig.ValidateState)
            {
                Response.Cookies.Append("OIDC_STATE", remoteAuthConfig.State, new CookieOptions { Expires = new DateTimeOffset(DateTime.Now.AddMinutes(5)) });
            }
            if (remoteAuthConfig.UsePKCE)
            {
                Response.Cookies.Append("OIDC_VERIFIER", pkceKeyPair.Key, new CookieOptions { Expires = new DateTimeOffset(DateTime.Now.AddMinutes(5)) });
            }
            var remoteAuthURL = remoteAuthConfig.RemoteAuthURL;
            return Json(remoteAuthURL);
        }
        public async Task<IActionResult> RemoteAuth(string code, string state = "")
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(code))
                {
                    //received code from OIDC provider
                    //create http client to retrieve user token from OIDC
                    var httpClient = new HttpClient();
                    var openIdConfig = _config.GetOpenIDConfig();
                    //check if validate state is enabled.
                    if (openIdConfig.ValidateState)
                    {
                        var storedStateValue = Request.Cookies["OIDC_STATE"];
                        if (!string.IsNullOrWhiteSpace(storedStateValue))
                        {
                            Response.Cookies.Delete("OIDC_STATE");
                        }
                        if (string.IsNullOrWhiteSpace(storedStateValue) || string.IsNullOrWhiteSpace(state) || storedStateValue != state)
                        {
                            _logger.LogInformation("Failed OIDC State Validation - Try disabling state validation if you are confident this is not a malicious attempt.");
                            return new RedirectResult("/Login");
                        }
                    }
                    var httpParams = new List<KeyValuePair<string, string>>
                {
                     new KeyValuePair<string, string>("code", code),
                     new KeyValuePair<string, string>("grant_type", "authorization_code"),
                     new KeyValuePair<string, string>("client_id", openIdConfig.ClientId),
                     new KeyValuePair<string, string>("client_secret", openIdConfig.ClientSecret),
                     new KeyValuePair<string, string>("redirect_uri", openIdConfig.RedirectURL)
                };
                    if (openIdConfig.UsePKCE)
                    {
                        //retrieve stored challenge verifier
                        var storedVerifier = Request.Cookies["OIDC_VERIFIER"];
                        if (!string.IsNullOrWhiteSpace(storedVerifier))
                        {
                            httpParams.Add(new KeyValuePair<string, string>("code_verifier", storedVerifier));
                            Response.Cookies.Delete("OIDC_VERIFIER");
                        }
                    }
                    var httpRequest = new HttpRequestMessage(HttpMethod.Post, openIdConfig.TokenURL)
                    {
                        Content = new FormUrlEncodedContent(httpParams)
                    };
                    var tokenResult = await httpClient.SendAsync(httpRequest).Result.Content.ReadAsStringAsync();
                    var decodedToken = JsonSerializer.Deserialize<OpenIDResult>(tokenResult);
                    var userJwt = decodedToken?.id_token ?? string.Empty;
                    var userAccessToken = decodedToken?.access_token ?? string.Empty;
                    var tokenParser = new JsonWebTokenHandler();
                    bool passedSignatureCheck = true;
                    if (!string.IsNullOrWhiteSpace(openIdConfig.JwksURL))
                    {
                        //validate token signature if jwks endpoint is provided
                        var jwksData = await httpClient.GetStringAsync(openIdConfig.JwksURL);
                        if (!string.IsNullOrWhiteSpace(jwksData))
                        {
                            var signingKeys = new JsonWebKeySet(jwksData).GetSigningKeys();
                            var tokenValidationParams = new TokenValidationParameters
                            {
                                ValidateAudience = false,
                                ValidateIssuer = false,
                                RequireAudience = false,
                                IssuerSigningKeys = signingKeys,
                                ValidateIssuerSigningKey = true
                            };
                            var validatedIdToken = await tokenParser.ValidateTokenAsync(userJwt, tokenValidationParams);
                            if (!validatedIdToken.IsValid)
                            {
                                passedSignatureCheck = false;
                            }
                        }
                    }
                    if (passedSignatureCheck)
                    {
                        if (!string.IsNullOrWhiteSpace(userJwt))
                        {
                            //validate JWT token
                            var parsedToken = tokenParser.ReadJsonWebToken(userJwt);
                            var userEmailAddress = string.Empty;
                            if (parsedToken.Claims.Any(x => x.Type == "email"))
                            {
                                userEmailAddress = parsedToken.Claims.First(x => x.Type == "email").Value;
                            }
                            else if (!string.IsNullOrWhiteSpace(openIdConfig.UserInfoURL) && !string.IsNullOrWhiteSpace(userAccessToken))
                            {
                                //retrieve claims from userinfo endpoint if no email claims are returned within id_token
                                var userInfoHttpRequest = new HttpRequestMessage(HttpMethod.Get, openIdConfig.UserInfoURL);
                                userInfoHttpRequest.Headers.Add("Authorization", $"Bearer {userAccessToken}");
                                var userInfoResult = await httpClient.SendAsync(userInfoHttpRequest).Result.Content.ReadAsStringAsync();
                                var userInfo = JsonSerializer.Deserialize<OpenIDUserInfo>(userInfoResult);
                                if (!string.IsNullOrWhiteSpace(userInfo?.email ?? string.Empty))
                                {
                                    userEmailAddress = userInfo?.email ?? string.Empty;
                                }
                                else
                                {
                                    _logger.LogError($"OpenID Provider did not provide an email claim via UserInfo endpoint");
                                }
                            }
                            else
                            {
                                var returnedClaims = parsedToken.Claims.Select(x => x.Type);
                                _logger.LogError($"OpenID Provider did not provide an email claim, claims returned: {string.Join(",", returnedClaims)}");
                            }
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
                                    Response.Cookies.Append(StaticHelper.LoginCookieName, encryptedCookie, new CookieOptions { Expires = new DateTimeOffset(authCookie.ExpiresOn) });
                                    return new RedirectResult("/Home");
                                }
                                else
                                {
                                    _logger.LogInformation($"User {userEmailAddress} tried to login via OpenID but is not a registered user in LubeLogger.");
                                    return View("OpenIDRegistration", model: userEmailAddress);
                                }
                            }
                            else
                            {
                                _logger.LogInformation("OpenID Provider did not provide a valid email address for the user");
                            }
                        }
                        else
                        {
                            _logger.LogInformation("OpenID Provider did not provide a valid id_token");
                            if (!string.IsNullOrWhiteSpace(tokenResult))
                            {
                                //if something was returned from the IdP but it's invalid, we want to log it as an error.
                                _logger.LogError($"Expected id_token, received {tokenResult}");
                            }
                        }
                    } 
                    else
                    {
                        _logger.LogError($"OpenID Provider did not provide a valid id_token: check jwks endpoint");
                    }
                }
                else
                {
                    _logger.LogInformation("OpenID Provider did not provide a code.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new RedirectResult("/Login");
            }
            return new RedirectResult("/Login");
        }
        public async Task<IActionResult> RemoteAuthDebug(string code, string state = "")
        {
            List<OperationResponse> results = new List<OperationResponse>();
            try
            {
                if (!string.IsNullOrWhiteSpace(code))
                {
                    results.Add(OperationResponse.Succeed($"Received code from OpenID Provider: {code}"));
                    //received code from OIDC provider
                    //create http client to retrieve user token from OIDC
                    var httpClient = new HttpClient();
                    var openIdConfig = _config.GetOpenIDConfig();
                    //check if validate state is enabled.
                    if (openIdConfig.ValidateState)
                    {
                        var storedStateValue = Request.Cookies["OIDC_STATE"];
                        if (!string.IsNullOrWhiteSpace(storedStateValue))
                        {
                            Response.Cookies.Delete("OIDC_STATE");
                        }
                        if (string.IsNullOrWhiteSpace(storedStateValue) || string.IsNullOrWhiteSpace(state) || storedStateValue != state)
                        {
                            results.Add(OperationResponse.Failed($"Failed State Validation - Expected: {storedStateValue} Received: {state}"));
                        }
                        else
                        {
                            results.Add(OperationResponse.Succeed($"Passed State Validation - Expected: {storedStateValue} Received: {state}"));
                        }
                    }
                    var httpParams = new List<KeyValuePair<string, string>>
                {
                     new KeyValuePair<string, string>("code", code),
                     new KeyValuePair<string, string>("grant_type", "authorization_code"),
                     new KeyValuePair<string, string>("client_id", openIdConfig.ClientId),
                     new KeyValuePair<string, string>("client_secret", openIdConfig.ClientSecret),
                     new KeyValuePair<string, string>("redirect_uri", openIdConfig.RedirectURL)
                };
                    if (openIdConfig.UsePKCE)
                    {
                        //retrieve stored challenge verifier
                        var storedVerifier = Request.Cookies["OIDC_VERIFIER"];
                        if (!string.IsNullOrWhiteSpace(storedVerifier))
                        {
                            httpParams.Add(new KeyValuePair<string, string>("code_verifier", storedVerifier));
                            Response.Cookies.Delete("OIDC_VERIFIER");
                        }
                    }
                    var httpRequest = new HttpRequestMessage(HttpMethod.Post, openIdConfig.TokenURL)
                    {
                        Content = new FormUrlEncodedContent(httpParams)
                    };
                    var tokenResult = await httpClient.SendAsync(httpRequest).Result.Content.ReadAsStringAsync();
                    var decodedToken = JsonSerializer.Deserialize<OpenIDResult>(tokenResult);
                    var userJwt = decodedToken?.id_token ?? string.Empty;
                    var userAccessToken = decodedToken?.access_token ?? string.Empty;
                    var tokenParser = new JsonWebTokenHandler();
                    bool passedSignatureCheck = true;
                    if (!string.IsNullOrWhiteSpace(openIdConfig.JwksURL))
                    {
                        //validate token signature if jwks endpoint is provided
                        var jwksData = await httpClient.GetStringAsync(openIdConfig.JwksURL);
                        if (!string.IsNullOrWhiteSpace(jwksData))
                        {
                            var signingKeys = new JsonWebKeySet(jwksData).GetSigningKeys();
                            var tokenValidationParams = new TokenValidationParameters
                            {
                                ValidateAudience = false,
                                ValidateIssuer = false,
                                RequireAudience = false,
                                IssuerSigningKeys = signingKeys,
                                ValidateIssuerSigningKey = true
                            };
                            var validatedIdToken = await tokenParser.ValidateTokenAsync(userJwt, tokenValidationParams);
                            if (!validatedIdToken.IsValid)
                            {
                                passedSignatureCheck = false;
                            } else
                            {
                                results.Add(OperationResponse.Succeed($"Passed JWT Validation - Valid To: {validatedIdToken.SecurityToken.ValidTo}"));
                            }
                        }
                    }
                    if (passedSignatureCheck)
                    {
                        if (!string.IsNullOrWhiteSpace(userJwt))
                        {
                            results.Add(OperationResponse.Succeed($"Passed JWT Parsing - id_token: {userJwt}"));
                            //validate JWT token
                            var parsedToken = tokenParser.ReadJsonWebToken(userJwt);
                            var userEmailAddress = string.Empty;
                            if (parsedToken.Claims.Any(x => x.Type == "email"))
                            {
                                userEmailAddress = parsedToken.Claims.First(x => x.Type == "email").Value;
                                results.Add(OperationResponse.Succeed($"Passed Claim Validation - email"));
                            }
                            else if (!string.IsNullOrWhiteSpace(openIdConfig.UserInfoURL) && !string.IsNullOrWhiteSpace(userAccessToken))
                            {
                                //retrieve claims from userinfo endpoint if no email claims are returned within id_token
                                var userInfoHttpRequest = new HttpRequestMessage(HttpMethod.Get, openIdConfig.UserInfoURL);
                                userInfoHttpRequest.Headers.Add("Authorization", $"Bearer {userAccessToken}");
                                var userInfoResult = await httpClient.SendAsync(userInfoHttpRequest).Result.Content.ReadAsStringAsync();
                                var userInfo = JsonSerializer.Deserialize<OpenIDUserInfo>(userInfoResult);
                                if (!string.IsNullOrWhiteSpace(userInfo?.email ?? string.Empty))
                                {
                                    userEmailAddress = userInfo?.email ?? string.Empty;
                                    results.Add(OperationResponse.Succeed($"Passed Claim Validation - Retrieved email via UserInfo endpoint"));
                                }
                                else
                                {
                                    results.Add(OperationResponse.Failed($"Failed Claim Validation - Unable to retrieve email via UserInfo endpoint: {openIdConfig.UserInfoURL} using access_token: {userAccessToken} - Received {userInfoResult}"));
                                }
                            }
                            else
                            {
                                var returnedClaims = parsedToken.Claims.Select(x => x.Type);
                                results.Add(OperationResponse.Failed($"Failed Claim Validation - Expected: email Received: {string.Join(",", returnedClaims)}"));
                            }
                            if (!string.IsNullOrWhiteSpace(userEmailAddress))
                            {
                                var userData = _loginLogic.ValidateOpenIDUser(new LoginModel() { EmailAddress = userEmailAddress });
                                if (userData.Id != default)
                                {
                                    results.Add(OperationResponse.Succeed($"Passed User Validation - Email: {userEmailAddress} Username: {userData.UserName}"));
                                }
                                else
                                {
                                    results.Add(OperationResponse.Succeed($"Passed Email Validation - Email: {userEmailAddress} User not registered"));
                                }
                            }
                            else
                            {
                                results.Add(OperationResponse.Failed($"Failed Email Validation - No email received from OpenID Provider"));
                            }
                        }
                        else
                        {
                            results.Add(OperationResponse.Failed($"Failed to parse JWT - Expected: id_token Received: {tokenResult}"));
                        }
                    } 
                    else
                    {
                        results.Add(OperationResponse.Failed("Failed JWT Validation: Check Signing Keys"));
                    }
                }
                else
                {
                    results.Add(OperationResponse.Failed("No code received from OpenID Provider"));
                }
            }
            catch (Exception ex)
            {
                results.Add(OperationResponse.Failed($"Exception: {ex.Message}"));
            }
            return View(results);
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
                        ExpiresOn = DateTime.Now.AddDays(credentials.IsPersistent ? _config.GetAuthCookieLifeSpan() : 1)
                    };
                    var serializedCookie = JsonSerializer.Serialize(authCookie);
                    var encryptedCookie = _dataProtector.Protect(serializedCookie);
                    Response.Cookies.Append(StaticHelper.LoginCookieName, encryptedCookie, new CookieOptions { Expires = new DateTimeOffset(authCookie.ExpiresOn) });
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
                    Response.Cookies.Append(StaticHelper.LoginCookieName, encryptedCookie, new CookieOptions { Expires = new DateTimeOffset(authCookie.ExpiresOn) });
                }
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult SendRegistrationToken(LoginModel credentials)
        {
            var result = _loginLogic.SendRegistrationToken(credentials);
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
        [Authorize(Roles = nameof(UserData.IsRootUser))] //User must already be logged in as root user to do this.
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
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpPost]
        public IActionResult DestroyLoginCreds()
        {
            try
            {
                var result = _loginLogic.DeleteRootUserCredentials();
                //destroy any login cookies.
                if (result)
                {
                    Response.Cookies.Delete(StaticHelper.LoginCookieName);
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
            Response.Cookies.Delete(StaticHelper.LoginCookieName);
            var remoteAuthConfig = _config.GetOpenIDConfig();
            if (remoteAuthConfig.DisableRegularLogin && !string.IsNullOrWhiteSpace(remoteAuthConfig.LogOutURL))
            {
                return Json(remoteAuthConfig.LogOutURL);
            }
            return Json("/Login");
        }
    }
}

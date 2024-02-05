using CarCareTracker.Helper;
using CarCareTracker.Logic;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CarCareTracker.Controllers
{
    public class LoginController : Controller
    {
        private IDataProtector _dataProtector;
        private ILoginLogic _loginLogic;
        private readonly ILogger<LoginController> _logger;
        public LoginController(
            ILogger<LoginController> logger,
            IDataProtectionProvider securityProvider,
            ILoginLogic loginLogic
            ) 
        {
            _dataProtector = securityProvider.CreateProtector("login");
            _logger = logger;
            _loginLogic = loginLogic;
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

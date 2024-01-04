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
        private readonly ILogger<LoginController> _logger;
        public LoginController(
            ILogger<LoginController> logger,
            IDataProtectionProvider securityProvider
            ) 
        {
            _dataProtector = securityProvider.CreateProtector("login");
            _logger = logger;
        }
        public IActionResult Index()
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
                var configFileContents = System.IO.File.ReadAllText("userConfig.json");
                var existingUserConfig = System.Text.Json.JsonSerializer.Deserialize<UserConfig>(configFileContents);
                if (existingUserConfig is not null)
                {
                    //create hashes of the login credentials.
                    var hashedUserName = Sha256_hash(credentials.UserName);
                    var hashedPassword = Sha256_hash(credentials.Password);
                    //compare against stored hash.
                    if (hashedUserName == existingUserConfig.UserNameHash &&
                        hashedPassword == existingUserConfig.UserPasswordHash)
                    {
                        //auth success, create auth cookie
                        //encrypt stuff.
                        AuthCookie authCookie = new AuthCookie
                        {
                            Id = 1, //this is hardcoded for now
                            UserName = credentials.UserName,
                            ExpiresOn = DateTime.Now.AddDays(credentials.IsPersistent ? 30 : 1)
                        };
                        var serializedCookie = JsonSerializer.Serialize(authCookie);
                        var encryptedCookie = _dataProtector.Protect(serializedCookie);
                        Response.Cookies.Append("ACCESS_TOKEN", encryptedCookie, new CookieOptions { Expires = new DateTimeOffset(authCookie.ExpiresOn) });
                        return Json(true);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on saving config file.");
            }
            return Json(false);
        }
        [Authorize] //User must already be logged in to do this.
        [HttpPost]
        public IActionResult CreateLoginCreds(LoginModel credentials)
        {
            try
            {
                var configFileContents = System.IO.File.ReadAllText("userConfig.json");
                var existingUserConfig = JsonSerializer.Deserialize<UserConfig>(configFileContents);
                if (existingUserConfig is not null)
                {
                    //create hashes of the login credentials.
                    var hashedUserName = Sha256_hash(credentials.UserName);
                    var hashedPassword = Sha256_hash(credentials.Password);
                    //copy over settings that are off limits on the settings page.
                    existingUserConfig.EnableAuth = true;
                    existingUserConfig.UserNameHash = hashedUserName;
                    existingUserConfig.UserPasswordHash = hashedPassword;
                }
                System.IO.File.WriteAllText("userConfig.json", JsonSerializer.Serialize(existingUserConfig));
                return Json(true);
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
                var configFileContents = System.IO.File.ReadAllText("userConfig.json");
                var existingUserConfig = JsonSerializer.Deserialize<UserConfig>(configFileContents);
                if (existingUserConfig is not null)
                {
                    //copy over settings that are off limits on the settings page.
                    existingUserConfig.EnableAuth = false;
                    existingUserConfig.UserNameHash = string.Empty;
                    existingUserConfig.UserPasswordHash = string.Empty;
                }
                System.IO.File.WriteAllText("userConfig.json", JsonSerializer.Serialize(existingUserConfig));
                //destroy any login cookies.
                Response.Cookies.Delete("ACCESS_TOKEN");
                return Json(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on saving config file.");
            }
            return Json(false);
        }
        private static string Sha256_hash(string value)
        {
            StringBuilder Sb = new StringBuilder();

            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
    }
}

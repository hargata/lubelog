using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CarCareTracker.Logic
{
    public interface ILoginLogic
    {
        bool MakeUserAdmin(int userId, bool isAdmin);
        OperationResponse GenerateUserToken(string emailAddress, bool autoNotify);
        bool DeleteUserToken(int tokenId);
        bool DeleteUser(int userId);
        OperationResponse RegisterNewUser(LoginModel credentials);
        OperationResponse RequestResetPassword(LoginModel credentials);
        OperationResponse ResetPasswordByUser(LoginModel credentials);
        OperationResponse ResetUserPassword(LoginModel credentials);
        UserData ValidateUserCredentials(LoginModel credentials);
        UserData ValidateOpenIDUser(LoginModel credentials);
        bool CheckIfUserIsValid(int userId);
        bool CreateRootUserCredentials(LoginModel credentials);
        bool DeleteRootUserCredentials();
        List<UserData> GetAllUsers();
        List<Token> GetAllTokens();

    }
    public class LoginLogic : ILoginLogic
    {
        private readonly IUserRecordDataAccess _userData;
        private readonly ITokenRecordDataAccess _tokenData;
        private readonly IMailHelper _mailHelper;
        private IMemoryCache _cache;
        public LoginLogic(IUserRecordDataAccess userData, 
            ITokenRecordDataAccess tokenData, 
            IMailHelper mailHelper,
            IMemoryCache memoryCache)
        {
            _userData = userData;
            _tokenData = tokenData;
            _mailHelper = mailHelper;
            _cache = memoryCache;
        }
        public bool CheckIfUserIsValid(int userId)
        {
            if (userId == -1)
            {
                return true;
            }
            var result = _userData.GetUserRecordById(userId);
            if (result == null)
            {
                return false;
            } else
            {
                return result.Id != 0;
            }
        }
        //handles user registration
        public OperationResponse RegisterNewUser(LoginModel credentials)
        {
            //validate their token.
            var existingToken = _tokenData.GetTokenRecordByBody(credentials.Token);
            if (existingToken.Id == default || existingToken.EmailAddress != credentials.EmailAddress)
            {
                return new OperationResponse { Success = false, Message = "Invalid Token" };
            }
            //token is valid, check if username and password is acceptable and that username is unique.
            if (string.IsNullOrWhiteSpace(credentials.EmailAddress) || string.IsNullOrWhiteSpace(credentials.UserName) || string.IsNullOrWhiteSpace(credentials.Password))
            {
                return new OperationResponse { Success = false, Message = "Neither username nor password can be blank" };
            }
            var existingUser = _userData.GetUserRecordByUserName(credentials.UserName);
            if (existingUser.Id != default)
            {
                return new OperationResponse { Success = false, Message = "Username already taken" };
            }
            var existingUserWithEmail = _userData.GetUserRecordByEmailAddress(credentials.EmailAddress);
            if (existingUserWithEmail.Id != default)
            {
                return new OperationResponse { Success = false, Message = "A user with that email already exists" };
            }
            //username is unique then we delete the token and create the user.
            _tokenData.DeleteToken(existingToken.Id);
            var newUser = new UserData()
            {
                UserName = credentials.UserName,
                Password = GetHash(credentials.Password),
                EmailAddress = credentials.EmailAddress
            };
            var result = _userData.SaveUserRecord(newUser);
            if (result)
            {
                return new OperationResponse { Success = true, Message = "You will be redirected to the login page briefly." };
            }
            else
            {
                return new OperationResponse { Success = false, Message = "Something went wrong, please try again later." };
            }
        }
        /// <summary>
        /// Generates a token and notifies user via email so they can reset their password.
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public OperationResponse RequestResetPassword(LoginModel credentials)
        {
            var existingUser = _userData.GetUserRecordByUserName(credentials.UserName);
            if (existingUser.Id != default)
            {
                //user exists, generate a token and send email.
                //check to see if there is an existing token sent to the user.
                var existingToken = _tokenData.GetTokenRecordByEmailAddress(existingUser.EmailAddress);
                if (existingToken.Id == default)
                {
                    var token = new Token()
                    {
                        Body = NewToken(),
                        EmailAddress = existingUser.EmailAddress
                    };
                    var result = _tokenData.CreateNewToken(token);
                    if (result)
                    {
                        result = _mailHelper.NotifyUserForPasswordReset(existingUser.EmailAddress, token.Body).Success;
                    }
                }
            }
            //for security purposes we want to always return true for this method.
            //otherwise someone can spam the reset password method to sniff out users.
            return new OperationResponse { Success = true, Message = "If your user exists in the system you should receive an email shortly with instructions on how to proceed." };
        }
        public OperationResponse ResetPasswordByUser(LoginModel credentials)
        {
            var existingToken = _tokenData.GetTokenRecordByBody(credentials.Token);
            if (existingToken.Id == default || existingToken.EmailAddress != credentials.EmailAddress)
            {
                return new OperationResponse { Success = false, Message = "Invalid Token" };
            }
            if (string.IsNullOrWhiteSpace(credentials.Password))
            {
                return new OperationResponse { Success = false, Message = "New Password cannot be blank" };
            }
            //if token is valid.
            var existingUser = _userData.GetUserRecordByEmailAddress(credentials.EmailAddress);
            if (existingUser.Id == default)
            {
                return new OperationResponse { Success = false, Message = "Unable to locate user" };
            }
            existingUser.Password = GetHash(credentials.Password);
            var result = _userData.SaveUserRecord(existingUser);
            //delete token
            _tokenData.DeleteToken(existingToken.Id);
            if (result)
            {
                return new OperationResponse { Success = true, Message = "Password resetted, you will be redirected to login page shortly." };
            } else
            {
                return new OperationResponse { Success = false, Message = StaticHelper.GenericErrorMessage };
            }
        }
        /// <summary>
        /// Returns an empty user if can't auth against neither root nor db user.
        /// </summary>
        /// <param name="credentials">credentials from login page</param>
        /// <returns></returns>
        public UserData ValidateUserCredentials(LoginModel credentials)
        {
            if (UserIsRoot(credentials))
            {
                return new UserData()
                {
                    Id = -1,
                    UserName = credentials.UserName,
                    IsAdmin = true,
                    IsRootUser = true
                };
            }
            else
            {
                //authenticate via DB.
                var result = _userData.GetUserRecordByUserName(credentials.UserName);
                if (GetHash(credentials.Password) == result.Password)
                {
                    result.Password = string.Empty;
                    return result;
                }
                else
                {
                    return new UserData();
                }
            }
        }
        public UserData ValidateOpenIDUser(LoginModel credentials)
        {
            var result = _userData.GetUserRecordByEmailAddress(credentials.EmailAddress);
            if (result.Id != default)
            {
                result.Password = string.Empty;
                return result;
            }
            else
            {
                return new UserData();
            }
        }
        #region "Admin Functions"
        public bool MakeUserAdmin(int userId, bool isAdmin)
        {
            var user = _userData.GetUserRecordById(userId);
            if (user == default)
            {
                return false;
            }
            user.IsAdmin = isAdmin;
            var result = _userData.SaveUserRecord(user);
            return result;
        }
        public List<UserData> GetAllUsers()
        {
            var result = _userData.GetUsers();
            return result;
        }
        public List<Token> GetAllTokens()
        {
            var result = _tokenData.GetTokens();
            return result;
        }
        public OperationResponse GenerateUserToken(string emailAddress, bool autoNotify)
        {
            //check if email address already has a token attached to it.
            var existingToken = _tokenData.GetTokenRecordByEmailAddress(emailAddress);
            if (existingToken.Id != default)
            {
                return new OperationResponse { Success = false, Message = "There is an existing token tied to this email address" };
            }
            var token = new Token()
            {
                Body = NewToken(),
                EmailAddress = emailAddress
            };
            var result = _tokenData.CreateNewToken(token);
            if (result && autoNotify)
            {
                result = _mailHelper.NotifyUserForRegistration(emailAddress, token.Body).Success;
                if (!result)
                {
                    return new OperationResponse { Success = false, Message = "Token Generated, but Email failed to send, please check your SMTP settings." };
                }
            }
            if (result)
            {
                return new OperationResponse { Success = true, Message = "Token Generated!" };
            }
            else
            {
                return new OperationResponse { Success = false, Message = StaticHelper.GenericErrorMessage };
            }
        }
        public bool DeleteUserToken(int tokenId)
        {
            var result = _tokenData.DeleteToken(tokenId);
            return result;
        }
        public bool DeleteUser(int userId)
        {
            var result = _userData.DeleteUserRecord(userId);
            return result;
        }
        public OperationResponse ResetUserPassword(LoginModel credentials)
        {
            //user might have forgotten their password.
            var existingUser = _userData.GetUserRecordByUserName(credentials.UserName);
            if (existingUser.Id == default)
            {
                return new OperationResponse { Success = false, Message = "Unable to find user" };
            }
            var newPassword = Guid.NewGuid().ToString().Substring(0, 8);
            existingUser.Password = GetHash(newPassword);
            var result = _userData.SaveUserRecord(existingUser);
            if (result)
            {
                return new OperationResponse { Success = true, Message = newPassword };
            }
            else
            {
                return new OperationResponse { Success = false, Message = "Something went wrong, please try again later." };
            }
        }
        #endregion
        #region "Root User"
        public bool CreateRootUserCredentials(LoginModel credentials)
        {
            //check if file exists
            if (File.Exists(StaticHelper.UserConfigPath))
            {
                var configFileContents = File.ReadAllText(StaticHelper.UserConfigPath);
                var existingUserConfig = JsonSerializer.Deserialize<UserConfig>(configFileContents);
                if (existingUserConfig is not null)
                {
                    //create hashes of the login credentials.
                    var hashedUserName = GetHash(credentials.UserName);
                    var hashedPassword = GetHash(credentials.Password);
                    //copy over settings that are off limits on the settings page.
                    existingUserConfig.EnableAuth = true;
                    existingUserConfig.UserNameHash = hashedUserName;
                    existingUserConfig.UserPasswordHash = hashedPassword;
                }
                File.WriteAllText(StaticHelper.UserConfigPath, JsonSerializer.Serialize(existingUserConfig));
            } else
            {
                var newUserConfig = new UserConfig()
                {
                    EnableAuth = true,
                    UserNameHash = GetHash(credentials.UserName),
                    UserPasswordHash = GetHash(credentials.Password)
                };
                File.WriteAllText(StaticHelper.UserConfigPath, JsonSerializer.Serialize(newUserConfig));
            }
            _cache.Remove("userConfig_-1");
            return true;
        }
        public bool DeleteRootUserCredentials()
        {
            var configFileContents = File.ReadAllText(StaticHelper.UserConfigPath);
            var existingUserConfig = JsonSerializer.Deserialize<UserConfig>(configFileContents);
            if (existingUserConfig is not null)
            {
                //copy over settings that are off limits on the settings page.
                existingUserConfig.EnableAuth = false;
                existingUserConfig.UserNameHash = string.Empty;
                existingUserConfig.UserPasswordHash = string.Empty;
            }
            //clear out the cached config for the root user.
            _cache.Remove("userConfig_-1");
            File.WriteAllText(StaticHelper.UserConfigPath, JsonSerializer.Serialize(existingUserConfig));
            return true;
        }
        private bool UserIsRoot(LoginModel credentials)
        {
            var configFileContents = File.ReadAllText(StaticHelper.UserConfigPath);
            var existingUserConfig = JsonSerializer.Deserialize<UserConfig>(configFileContents);
            if (existingUserConfig is not null)
            {
                //create hashes of the login credentials.
                var hashedUserName = GetHash(credentials.UserName);
                var hashedPassword = GetHash(credentials.Password);
                //compare against stored hash.
                if (hashedUserName == existingUserConfig.UserNameHash &&
                    hashedPassword == existingUserConfig.UserPasswordHash)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
        private static string GetHash(string value)
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
        private string NewToken()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }
    }
}

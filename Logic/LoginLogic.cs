using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
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
        OperationResponse RegisterOpenIdUser(LoginModel credentials);
        OperationResponse UpdateUserDetails(int userId, LoginModel credentials);
        OperationResponse RegisterNewUser(LoginModel credentials);
        OperationResponse RequestResetPassword(LoginModel credentials);
        OperationResponse ResetPasswordByUser(LoginModel credentials);
        OperationResponse ResetUserPassword(LoginModel credentials);
        UserData ValidateUserCredentials(LoginModel credentials);
        UserData ValidateOpenIDUser(LoginModel credentials);
        bool CheckIfUserIsValid(int userId);
        bool CreateRootUserCredentials(LoginModel credentials);
        bool DeleteRootUserCredentials();
        bool GenerateTokenForEmailAddress(string emailAddress, bool isPasswordReset);
        List<UserData> GetAllUsers();
        List<Token> GetAllTokens();
        KeyValuePair<string, string> GetPKCEChallengeCode();
    }
    public class LoginLogic : ILoginLogic
    {
        private readonly IUserRecordDataAccess _userData;
        private readonly ITokenRecordDataAccess _tokenData;
        private readonly IMailHelper _mailHelper;
        private readonly IConfigHelper _configHelper;
        private readonly IMemoryCache _cache;

        private const string InvalidToken = "Invalid Token";
        private const string UsernameAlreadyTaken = "Username already taken";
        private const string UserWithEmailExists = "A user with that email already exists";
        
        public LoginLogic(IUserRecordDataAccess userData, 
            ITokenRecordDataAccess tokenData, 
            IMailHelper mailHelper,
            IConfigHelper configHelper,
            IMemoryCache memoryCache)
        {
            _userData = userData;
            _tokenData = tokenData;
            _mailHelper = mailHelper;
            _configHelper = configHelper;
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
        public OperationResponse UpdateUserDetails(int userId, LoginModel credentials)
        {
            //get current user details
            var existingUser = _userData.GetUserRecordById(userId);
            if (existingUser.Id == default)
            {
                return StaticHelper.GetOperationResponse(false, "Invalid user");
            }
            //validate user token
            var existingToken = _tokenData.GetTokenRecordByBody(credentials.Token);
            if (existingToken.Id == default || existingToken.EmailAddress != existingUser.EmailAddress)
            {
                return StaticHelper.GetOperationResponse(false, InvalidToken);
            }
            if (!string.IsNullOrWhiteSpace(credentials.UserName) && existingUser.UserName != credentials.UserName)
            {
                //check if new username is already taken.
                var existingUserWithUserName = _userData.GetUserRecordByUserName(credentials.UserName);
                if (existingUserWithUserName.Id != default)
                {
                    return StaticHelper.GetOperationResponse(false, UsernameAlreadyTaken);
                }
                existingUser.UserName = credentials.UserName;
            }
            if (!string.IsNullOrWhiteSpace(credentials.EmailAddress) && existingUser.EmailAddress != credentials.EmailAddress)
            {
                //check if email address already exists
                var existingUserWithEmailAddress = _userData.GetUserRecordByEmailAddress(credentials.EmailAddress);
                if (existingUserWithEmailAddress.Id != default)
                {
                    return StaticHelper.GetOperationResponse(false, UserWithEmailExists);
                }
                existingUser.EmailAddress = credentials.EmailAddress;
            }
            if (!string.IsNullOrWhiteSpace(credentials.Password))
            {
                //update password
                existingUser.Password = GetHash(credentials.Password);
            }
            //delete token
            _tokenData.DeleteToken(existingToken.Id);
            var result = _userData.SaveUserRecord(existingUser);
            return StaticHelper.GetOperationResponse(result, result ? "User Updated" : StaticHelper.GenericErrorMessage);
        }
        public OperationResponse RegisterOpenIdUser(LoginModel credentials)
        {
            //validate their token.
            var existingToken = _tokenData.GetTokenRecordByBody(credentials.Token);
            if (existingToken.Id == default || existingToken.EmailAddress != credentials.EmailAddress)
            {
                return StaticHelper.GetOperationResponse(false, InvalidToken);
            }
            if (string.IsNullOrWhiteSpace(credentials.EmailAddress) || string.IsNullOrWhiteSpace(credentials.UserName))
            {
                return StaticHelper.GetOperationResponse(false, "Username cannot be blank");
            }
            var existingUser = _userData.GetUserRecordByUserName(credentials.UserName);
            if (existingUser.Id != default)
            {
                return StaticHelper.GetOperationResponse(false, UsernameAlreadyTaken);
            }
            var existingUserWithEmail = _userData.GetUserRecordByEmailAddress(credentials.EmailAddress);
            if (existingUserWithEmail.Id != default)
            {
                return StaticHelper.GetOperationResponse(false, UserWithEmailExists);
            }
            _tokenData.DeleteToken(existingToken.Id);
            var newUser = new UserData()
            {
                UserName = credentials.UserName,
                Password = GetHash(NewToken()), //generate a password for OpenID User
                EmailAddress = credentials.EmailAddress
            };
            var result = _userData.SaveUserRecord(newUser);
            if (result)
            {
                return StaticHelper.GetOperationResponse(true, "You will be logged in briefly.");
            }
            else
            {
                return StaticHelper.GetOperationResponse(false, StaticHelper.GenericErrorMessage);
            }
        }
        //handles user registration
        public OperationResponse RegisterNewUser(LoginModel credentials)
        {
            //validate their token.
            var existingToken = _tokenData.GetTokenRecordByBody(credentials.Token);
            if (existingToken.Id == default || existingToken.EmailAddress != credentials.EmailAddress)
            {
                return StaticHelper.GetOperationResponse(false, InvalidToken);
            }
            //token is valid, check if username and password is acceptable and that username is unique.
            if (string.IsNullOrWhiteSpace(credentials.EmailAddress) || string.IsNullOrWhiteSpace(credentials.UserName) || string.IsNullOrWhiteSpace(credentials.Password))
            {
                return StaticHelper.GetOperationResponse(false, "Neither username nor password can be blank");
            }
            var existingUser = _userData.GetUserRecordByUserName(credentials.UserName);
            if (existingUser.Id != default)
            {
                return StaticHelper.GetOperationResponse(false, UsernameAlreadyTaken);
            }
            var existingUserWithEmail = _userData.GetUserRecordByEmailAddress(credentials.EmailAddress);
            if (existingUserWithEmail.Id != default)
            {
                return StaticHelper.GetOperationResponse(false, UserWithEmailExists);
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
                return StaticHelper.GetOperationResponse(true, "You will be redirected to the login page briefly.");
            }
            else
            {
                return StaticHelper.GetOperationResponse(false, StaticHelper.GenericErrorMessage);
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
                GenerateTokenForEmailAddress(existingUser.EmailAddress, true);
            }
            //for security purposes we want to always return true for this method.
            //otherwise someone can spam the reset password method to sniff out users.
            return StaticHelper.GetOperationResponse(true, "If your user exists in the system you should receive an email shortly with instructions on how to proceed.");
        }
        public OperationResponse ResetPasswordByUser(LoginModel credentials)
        {
            var existingToken = _tokenData.GetTokenRecordByBody(credentials.Token);
            if (existingToken.Id == default || existingToken.EmailAddress != credentials.EmailAddress)
            {
                return StaticHelper.GetOperationResponse(false, InvalidToken);
            }
            if (string.IsNullOrWhiteSpace(credentials.Password))
            {
                return StaticHelper.GetOperationResponse(false, "New Password cannot be blank");
            }
            //if token is valid.
            var existingUser = _userData.GetUserRecordByEmailAddress(credentials.EmailAddress);
            if (existingUser.Id == default)
            {
                return StaticHelper.GetOperationResponse(false, "Unable to locate user");
            }
            existingUser.Password = GetHash(credentials.Password);
            var result = _userData.SaveUserRecord(existingUser);
            //delete token
            _tokenData.DeleteToken(existingToken.Id);
            if (result)
            {
                return StaticHelper.GetOperationResponse(true, "Password resetted, you will be redirected to login page shortly.");
            } else
            {
                return StaticHelper.GetOperationResponse(false, StaticHelper.GenericErrorMessage);
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
                return GetRootUserData(credentials.UserName);
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
            //validate for root user
            var isRootUser = _configHelper.AuthenticateRootUserOIDC(credentials.EmailAddress);
            if (isRootUser)
            {
                return GetRootUserData(credentials.EmailAddress);
            }

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
                return StaticHelper.GetOperationResponse(false, "There is an existing token tied to this email address");
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
                    return StaticHelper.GetOperationResponse(false, "Token Generated, but Email failed to send, please check your SMTP settings.");
                }
            }
            if (result)
            {
                return StaticHelper.GetOperationResponse(true, "Token Generated!");
            }
            else
            {
                return StaticHelper.GetOperationResponse(false, StaticHelper.GenericErrorMessage);
            }
        }
        public bool DeleteUserToken(int tokenId)
        {
            var result = _tokenData.DeleteToken(tokenId);
            return result;
        }
        public bool DeleteUser(int userId)
        {
            return _userData.DeleteUserRecord(userId);
        }
        public OperationResponse ResetUserPassword(LoginModel credentials)
        {
            //user might have forgotten their password.
            var existingUser = _userData.GetUserRecordByUserName(credentials.UserName);
            if (existingUser.Id == default)
            {
                return StaticHelper.GetOperationResponse(false, "Unable to find user");
            }
            var newPassword = Guid.NewGuid().ToString().Substring(0, 8);
            existingUser.Password = GetHash(newPassword);
            var result = _userData.SaveUserRecord(existingUser);
            
            if (result)
            {
                return StaticHelper.GetOperationResponse(true, newPassword);
            }
            else
            {
                return StaticHelper.GetOperationResponse(false, StaticHelper.GenericErrorMessage);
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
            var hashedUserName = GetHash(credentials.UserName);
            var hashedPassword = GetHash(credentials.Password);
            return _configHelper.AuthenticateRootUser(hashedUserName, hashedPassword);
        }
        private static UserData GetRootUserData(string username)
        {
            return new UserData()
            {
                Id = -1,
                UserName = username,
                IsAdmin = true,
                IsRootUser = true,
                EmailAddress = string.Empty
            };
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
        private static string NewToken()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }
        public KeyValuePair<string, string> GetPKCEChallengeCode()
        {
            var verifierCode = Base64UrlEncoder.Encode(Guid.NewGuid().ToString().Replace("-", ""));
            var verifierBytes = Encoding.UTF8.GetBytes(verifierCode);
            var hashedCode = SHA256.HashData(verifierBytes);
            var encodedChallengeCode = Base64UrlEncoder.Encode(hashedCode);
            return new KeyValuePair<string, string>(verifierCode, encodedChallengeCode);
        }
        public bool GenerateTokenForEmailAddress(string emailAddress, bool isPasswordReset)
        {
            bool result;
            //check if there is already a token tied to this email address.
            var existingToken = _tokenData.GetTokenRecordByEmailAddress(emailAddress);
            if (existingToken.Id == default)
            {
                //no token, generate one and send.
                var token = new Token()
                {
                    Body = NewToken(),
                    EmailAddress = emailAddress
                };
                result = _tokenData.CreateNewToken(token);
                if (result)
                {
                    result = isPasswordReset ? _mailHelper.NotifyUserForPasswordReset(emailAddress, token.Body).Success : _mailHelper.NotifyUserForAccountUpdate(emailAddress, token.Body).Success;
                }
            } else
            {
                //token exists, send it again.
                result = isPasswordReset ? _mailHelper.NotifyUserForPasswordReset(emailAddress, existingToken.Body).Success : _mailHelper.NotifyUserForAccountUpdate(emailAddress, existingToken.Body).Success;
            }
            return result;
        }
    }
}

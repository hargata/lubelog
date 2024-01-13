using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CarCareTracker.Logic
{
    public interface ILoginLogic
    {
        OperationResponse GenerateUserToken(string emailAddress, bool autoNotify);
        bool DeleteUserToken(int tokenId);
        OperationResponse RegisterNewUser(LoginModel credentials);
        OperationResponse ResetUserPassword(LoginModel credentials);
        UserData ValidateUserCredentials(LoginModel credentials);
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
        public LoginLogic(IUserRecordDataAccess userData, ITokenRecordDataAccess tokenData, IMailHelper mailHelper)
        {
            _userData = userData;
            _tokenData = tokenData;
            _mailHelper = mailHelper;
        }
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
                Password = GetHash(credentials.Password)
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
        #region "Admin Functions"
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
                Body = Guid.NewGuid().ToString().Substring(0, 8),
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
            } else
            {
                return new OperationResponse { Success = false, Message = StaticHelper.GenericErrorMessage };
            }
        }
        public bool DeleteUserToken(int tokenId)
        {
            var result = _tokenData.DeleteToken(tokenId);
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
    }
}

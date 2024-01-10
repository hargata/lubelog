using CarCareTracker.Models;
using System.Security.Cryptography;
using System.Text;

namespace CarCareTracker.Helper
{
    public interface ILoginHelper
    {
        bool ValidateUserCredentials(LoginModel credentials);
    }
    public class LoginHelper: ILoginHelper
    {
        public bool ValidateUserCredentials(LoginModel credentials)
        {
            var configFileContents = System.IO.File.ReadAllText(StaticHelper.UserConfigPath);
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
                    return true;
                }
            }
            return false;
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

using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using System.Security.Claims;

namespace CarCareTracker.Helper
{
    public interface IConfigHelper
    {
        UserConfig GetUserConfig(ClaimsPrincipal user);
        bool SaveUserConfig(bool isRootUser, int userId, UserConfig configData);
        public bool DeleteUserConfig(int userId);
    }
    public class ConfigHelper : IConfigHelper
    {
        private readonly IConfiguration _config;
        private readonly IUserConfigDataAccess _userConfig;
        public ConfigHelper(IConfiguration serverConfig, IUserConfigDataAccess userConfig)
        {
            _config = serverConfig;
            _userConfig = userConfig;
        }
        public bool SaveUserConfig(bool isRootUser, int userId, UserConfig configData)
        {
            if (isRootUser)
            {
                try
                {
                    if (!File.Exists(StaticHelper.UserConfigPath))
                    {
                        //if file doesn't exist it might be because it's running on a mounted volume in docker.
                        File.WriteAllText(StaticHelper.UserConfigPath, System.Text.Json.JsonSerializer.Serialize(new UserConfig()));
                    }
                    var configFileContents = File.ReadAllText(StaticHelper.UserConfigPath);
                    var existingUserConfig = System.Text.Json.JsonSerializer.Deserialize<UserConfig>(configFileContents);
                    if (existingUserConfig is not null)
                    {
                        //copy over settings that are off limits on the settings page.
                        configData.EnableAuth = existingUserConfig.EnableAuth;
                        configData.UserNameHash = existingUserConfig.UserNameHash;
                        configData.UserPasswordHash = existingUserConfig.UserPasswordHash;
                    }
                    else
                    {
                        configData.EnableAuth = false;
                        configData.UserNameHash = string.Empty;
                        configData.UserPasswordHash = string.Empty;
                    }
                    File.WriteAllText(StaticHelper.UserConfigPath, System.Text.Json.JsonSerializer.Serialize(configData));
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            } else
            {
                var userConfig = new UserConfigData()
                {
                    Id = userId,
                    UserConfig = configData
                };
                var result = _userConfig.SaveUserConfig(userConfig);
                return result;
            }
        }
        public bool DeleteUserConfig(int userId)
        {
            var result = _userConfig.DeleteUserConfig(userId);
            return result;
        }
        public UserConfig GetUserConfig(ClaimsPrincipal user)
        {
            var serverConfig = new UserConfig
            {
                EnableCsvImports = bool.Parse(_config[nameof(UserConfig.EnableCsvImports)]),
                UseDarkMode = bool.Parse(_config[nameof(UserConfig.UseDarkMode)]),
                UseMPG = bool.Parse(_config[nameof(UserConfig.UseMPG)]),
                UseDescending = bool.Parse(_config[nameof(UserConfig.UseDescending)]),
                EnableAuth = bool.Parse(_config[nameof(UserConfig.EnableAuth)]),
                HideZero = bool.Parse(_config[nameof(UserConfig.HideZero)]),
                UseUKMPG = bool.Parse(_config[nameof(UserConfig.UseUKMPG)])
            };
            if (!user.Identity.IsAuthenticated)
            {
                return serverConfig;
            }
            bool isRootUser = user.IsInRole(nameof(UserData.IsRootUser));
            int userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
            if (isRootUser)
            {
                return serverConfig;
            } else
            {
                var result = _userConfig.GetUserConfig(userId);
                if (result == null)
                {
                    return serverConfig;
                } else
                {
                    return result.UserConfig;
                }
            }
        }
    }
}

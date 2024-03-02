using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace CarCareTracker.Helper
{
    public interface IConfigHelper
    {
        OpenIDConfig GetOpenIDConfig();
        UserConfig GetUserConfig(ClaimsPrincipal user);
        bool SaveUserConfig(ClaimsPrincipal user, UserConfig configData);
        bool AuthenticateRootUser(string username, string password);
        string GetLogoUrl();
        string GetServerLanguage();
        bool GetServerEnableShopSupplies();
        string GetServerPostgresConnection();
        public bool DeleteUserConfig(int userId);
    }
    public class ConfigHelper : IConfigHelper
    {
        private readonly IConfiguration _config;
        private readonly IUserConfigDataAccess _userConfig;
        private IMemoryCache _cache;
        public ConfigHelper(IConfiguration serverConfig, 
            IUserConfigDataAccess userConfig,
            IMemoryCache memoryCache)
        {
            _config = serverConfig;
            _userConfig = userConfig;
            _cache = memoryCache;
        }
        public OpenIDConfig GetOpenIDConfig()
        {
            OpenIDConfig openIdConfig = _config.GetSection("OpenIDConfig").Get<OpenIDConfig>() ?? new OpenIDConfig();
            return openIdConfig;
        }
        public string GetLogoUrl()
        {
            var logoUrl = _config["LUBELOGGER_LOGO_URL"];
            if (string.IsNullOrWhiteSpace(logoUrl))
            {
                logoUrl = "/defaults/lubelogger_logo.png";
            }
            return logoUrl;
        }
        public bool AuthenticateRootUser(string username, string password)
        {
            var rootUsername = _config["UserNameHash"];
            var rootPassword = _config["UserPasswordHash"];
            return username == rootUsername && password == rootPassword;
        }
        public string GetServerLanguage()
        {
            var serverLanguage = _config[nameof(UserConfig.UserLanguage)] ?? "en_US";
            return serverLanguage;
        }
        public string GetServerPostgresConnection()
        {
            if (!string.IsNullOrWhiteSpace(_config["POSTGRES_CONNECTION"]))
            {
                return _config["POSTGRES_CONNECTION"];
            } else
            {
                return string.Empty;
            }
        }
        public bool GetServerEnableShopSupplies()
        {
            return bool.Parse(_config[nameof(UserConfig.EnableShopSupplies)] ?? "false");
        }
        public bool SaveUserConfig(ClaimsPrincipal user, UserConfig configData)
        {
            var storedUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = 0;
            if (storedUserId != null)
            {
                userId = int.Parse(storedUserId);
            }
            bool isRootUser = user.IsInRole(nameof(UserData.IsRootUser)) || userId == -1;
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
                    _cache.Set<UserConfig>($"userConfig_{userId}", configData);
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
                _cache.Set<UserConfig>($"userConfig_{userId}", configData);
                return result;
            }
        }
        public bool DeleteUserConfig(int userId)
        {
            _cache.Remove($"userConfig_{userId}");
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
                UseUKMPG = bool.Parse(_config[nameof(UserConfig.UseUKMPG)]),
                UseMarkDownOnSavedNotes = bool.Parse(_config[nameof(UserConfig.UseMarkDownOnSavedNotes)]),
                UseThreeDecimalGasCost = bool.Parse(_config[nameof(UserConfig.UseThreeDecimalGasCost)]),
                EnableAutoReminderRefresh = bool.Parse(_config[nameof(UserConfig.EnableAutoReminderRefresh)]),
                EnableAutoOdometerInsert = bool.Parse(_config[nameof(UserConfig.EnableAutoOdometerInsert)]),
                PreferredGasMileageUnit = _config[nameof(UserConfig.PreferredGasMileageUnit)],
                PreferredGasUnit = _config[nameof(UserConfig.PreferredGasUnit)],
                UserLanguage = _config[nameof(UserConfig.UserLanguage)],
                EnableShopSupplies = bool.Parse(_config[nameof(UserConfig.EnableShopSupplies)]),
                EnableExtraFieldColumns = bool.Parse(_config[nameof(UserConfig.EnableExtraFieldColumns)]),
                VisibleTabs = _config.GetSection("VisibleTabs").Get<List<ImportMode>>(),
                DefaultTab = (ImportMode)int.Parse(_config[nameof(UserConfig.DefaultTab)])
            };
            int userId = 0;
            if (user != null)
            {
                var storedUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (storedUserId != null)
                {
                    userId = int.Parse(storedUserId);
                }
            } else
            {
                return serverConfig;
            }
            return _cache.GetOrCreate<UserConfig>($"userConfig_{userId}", entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(1);
                if (!user.Identity.IsAuthenticated)
                {
                    return serverConfig;
                }
                bool isRootUser = user.IsInRole(nameof(UserData.IsRootUser)) || userId == -1;
                if (isRootUser)
                {
                    return serverConfig;
                }
                else
                {
                    var result = _userConfig.GetUserConfig(userId);
                    if (result == null)
                    {
                        return serverConfig;
                    }
                    else
                    {
                        return result.UserConfig;
                    }
                }
            });
        }
    }
}

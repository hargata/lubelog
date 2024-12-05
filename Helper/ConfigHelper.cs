using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using System.Text.Json;

namespace CarCareTracker.Helper
{
    public interface IConfigHelper
    {
        OpenIDConfig GetOpenIDConfig();
        ReminderUrgencyConfig GetReminderUrgencyConfig();
        MailConfig GetMailConfig();
        UserConfig GetUserConfig(ClaimsPrincipal user);
        bool SaveUserConfig(ClaimsPrincipal user, UserConfig configData);
        bool AuthenticateRootUser(string username, string password);
        bool AuthenticateRootUserOIDC(string email);
        string GetWebHookUrl();
        bool GetCustomWidgetsEnabled();
        string GetMOTD();
        string GetLogoUrl();
        string GetServerLanguage();
        bool GetServerDisabledRegistration();
        bool GetServerEnableShopSupplies();
        string GetServerPostgresConnection();
        string GetAllowedFileUploadExtensions();
        bool DeleteUserConfig(int userId);
        bool GetInvariantApi();
    }
    public class ConfigHelper : IConfigHelper
    {
        private readonly IConfiguration _config;
        private readonly IUserConfigDataAccess _userConfig;
        private readonly ILogger<IConfigHelper> _logger;
        private IMemoryCache _cache;
        public ConfigHelper(IConfiguration serverConfig, 
            IUserConfigDataAccess userConfig,
            IMemoryCache memoryCache,
            ILogger<IConfigHelper> logger)
        {
            _config = serverConfig;
            _userConfig = userConfig;
            _cache = memoryCache;
            _logger = logger;
        }
        public string GetWebHookUrl()
        {
            var webhook = CheckString("LUBELOGGER_WEBHOOK");
            return webhook;
        }
        public bool GetCustomWidgetsEnabled()
        {
            return CheckBool(CheckString("LUBELOGGER_CUSTOM_WIDGETS"));
        }
        public bool GetInvariantApi()
        {
            return CheckBool(CheckString("LUBELOGGER_INVARIANT_API"));
        }
        public string GetMOTD()
        {
            var motd = CheckString("LUBELOGGER_MOTD");
            return motd;
        }
        public OpenIDConfig GetOpenIDConfig()
        {
            OpenIDConfig openIdConfig = _config.GetSection("OpenIDConfig").Get<OpenIDConfig>() ?? new OpenIDConfig();
            return openIdConfig;
        }
        public ReminderUrgencyConfig GetReminderUrgencyConfig()
        {
            ReminderUrgencyConfig reminderUrgencyConfig = _config.GetSection("ReminderUrgencyConfig").Get<ReminderUrgencyConfig>() ?? new ReminderUrgencyConfig();
            return reminderUrgencyConfig;
        }
        public MailConfig GetMailConfig()
        {
            MailConfig mailConfig = _config.GetSection("MailConfig").Get<MailConfig>() ?? new MailConfig();
            return mailConfig;
        }
        public string GetLogoUrl()
        {
            var logoUrl = CheckString("LUBELOGGER_LOGO_URL", "/defaults/lubelogger_logo.png");
            return logoUrl;
        }
        public string GetAllowedFileUploadExtensions()
        {
            var allowedFileExtensions = CheckString("LUBELOGGER_ALLOWED_FILE_EXTENSIONS", StaticHelper.DefaultAllowedFileExtensions);
            return allowedFileExtensions;
        }
        public bool AuthenticateRootUser(string username, string password)
        {
            var rootUsername = CheckString(nameof(UserConfig.UserNameHash));
            var rootPassword = CheckString(nameof(UserConfig.UserPasswordHash));
            if (string.IsNullOrWhiteSpace(rootUsername) || string.IsNullOrWhiteSpace(rootPassword))
            {
                return false;
            }
            return username == rootUsername && password == rootPassword;
        }
        public bool AuthenticateRootUserOIDC(string email)
        {
            var rootEmail = CheckString(nameof(UserConfig.DefaultReminderEmail));
            var rootUserOIDC = CheckBool(CheckString(nameof(UserConfig.EnableRootUserOIDC)));
            if (!rootUserOIDC || string.IsNullOrWhiteSpace(rootEmail))
            {
                return false;
            }
            return email == rootEmail;
        }
        public string GetServerLanguage()
        {
            var serverLanguage = CheckString(nameof(UserConfig.UserLanguage), "en_US");
            return serverLanguage;
        }
        public bool GetServerDisabledRegistration()
        {
            var registrationDisabled = CheckBool(CheckString(nameof(UserConfig.DisableRegistration)));
            return registrationDisabled;
        }
        public string GetServerPostgresConnection()
        {
            var postgresConnection = CheckString("POSTGRES_CONNECTION");
            return postgresConnection;
        }
        public bool GetServerEnableShopSupplies()
        {
            return CheckBool(CheckString(nameof(UserConfig.EnableShopSupplies)));
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
                        File.WriteAllText(StaticHelper.UserConfigPath, JsonSerializer.Serialize(new UserConfig()));
                    }
                    var configFileContents = File.ReadAllText(StaticHelper.UserConfigPath);
                    configData.EnableAuth = bool.Parse(_config[nameof(UserConfig.EnableAuth)] ?? "false");
                    configData.UserNameHash = _config[nameof(UserConfig.UserNameHash)] ?? string.Empty;
                    configData.UserPasswordHash = _config[nameof(UserConfig.UserPasswordHash)] ?? string.Empty;
                    File.WriteAllText(StaticHelper.UserConfigPath, JsonSerializer.Serialize(configData));
                    _cache.Set<UserConfig>($"userConfig_{userId}", configData);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex.Message);
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
        private bool CheckBool(string value, bool defaultValue = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return defaultValue;
                }
                else if (bool.TryParse(value, out bool result))
                {
                    return result;
                }
                else
                {
                    return defaultValue;
                }
            } catch (Exception ex)
            {
                _logger.LogWarning($"ConfigHelper Warning: You might be missing keys in appsettings.json, Message: ${ex.Message}");
                return defaultValue;
            }
        }
        private string CheckString(string configName, string defaultValue = "")
        {
            try
            {
                var configValue = _config[configName] ?? defaultValue;
                return configValue;
            } catch(Exception ex)
            {
                _logger.LogWarning($"ConfigHelper Warning: You might be missing keys in appsettings.json, Message: ${ex.Message}");
                return defaultValue;
            }
        }
        public UserConfig GetUserConfig(ClaimsPrincipal user)
        {
            var serverConfig = new UserConfig
            {
                EnableCsvImports = CheckBool(CheckString(nameof(UserConfig.EnableCsvImports)), true),
                UseDarkMode = CheckBool(CheckString(nameof(UserConfig.UseDarkMode))),
                UseSystemColorMode = CheckBool(CheckString(nameof(UserConfig.UseSystemColorMode))),
                UseMPG = CheckBool(CheckString(nameof(UserConfig.UseMPG)), true),
                UseDescending = CheckBool(CheckString(nameof(UserConfig.UseDescending))),
                EnableAuth = CheckBool(CheckString(nameof(UserConfig.EnableAuth))),
                EnableRootUserOIDC = CheckBool(CheckString(nameof(UserConfig.EnableRootUserOIDC))),
                HideZero = CheckBool(CheckString(nameof(UserConfig.HideZero))),
                AutomaticDecimalFormat = CheckBool(CheckString(nameof(UserConfig.AutomaticDecimalFormat))),
                UseUKMPG = CheckBool(CheckString(nameof(UserConfig.UseUKMPG))),
                UseMarkDownOnSavedNotes = CheckBool(CheckString(nameof(UserConfig.UseMarkDownOnSavedNotes))),
                UseThreeDecimalGasCost = CheckBool(CheckString(nameof(UserConfig.UseThreeDecimalGasCost)), true),
                UseThreeDecimalGasConsumption = CheckBool(CheckString(nameof(UserConfig.UseThreeDecimalGasConsumption)), true),
                EnableAutoReminderRefresh = CheckBool(CheckString(nameof(UserConfig.EnableAutoReminderRefresh))),
                EnableAutoOdometerInsert = CheckBool(CheckString(nameof(UserConfig.EnableAutoOdometerInsert))),
                PreferredGasMileageUnit = CheckString(nameof(UserConfig.PreferredGasMileageUnit)),
                PreferredGasUnit = CheckString(nameof(UserConfig.PreferredGasUnit)),
                UseUnitForFuelCost = CheckBool(CheckString(nameof(UserConfig.UseUnitForFuelCost))),
                UserLanguage = CheckString(nameof(UserConfig.UserLanguage), "en_US"),
                HideSoldVehicles = CheckBool(CheckString(nameof(UserConfig.HideSoldVehicles))),
                EnableShopSupplies = CheckBool(CheckString(nameof(UserConfig.EnableShopSupplies))),
                EnableExtraFieldColumns = CheckBool(CheckString(nameof(UserConfig.EnableExtraFieldColumns))),
                VisibleTabs = _config.GetSection(nameof(UserConfig.VisibleTabs)).Get<List<ImportMode>>() ?? new UserConfig().VisibleTabs,
                TabOrder = _config.GetSection(nameof(UserConfig.TabOrder)).Get<List<ImportMode>>() ?? new UserConfig().TabOrder,
                UserColumnPreferences = _config.GetSection(nameof(UserConfig.UserColumnPreferences)).Get<List<UserColumnPreference>>() ?? new List<UserColumnPreference>(),
                ReminderUrgencyConfig = _config.GetSection(nameof(UserConfig.ReminderUrgencyConfig)).Get<ReminderUrgencyConfig>() ?? new ReminderUrgencyConfig(),
                DefaultTab = (ImportMode)int.Parse(CheckString(nameof(UserConfig.DefaultTab), "8")),
                DefaultReminderEmail = CheckString(nameof(UserConfig.DefaultReminderEmail)),
                DisableRegistration = CheckBool(CheckString(nameof(UserConfig.DisableRegistration)))
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

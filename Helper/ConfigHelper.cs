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
        KestrelAppConfig GetKestrelAppConfig();
        bool SaveUserConfig(ClaimsPrincipal user, UserConfig configData);
        bool SaveServerConfig(ServerConfig serverConfig);
        bool AuthenticateRootUser(string username, string password);
        bool AuthenticateRootUserOIDC(string email);
        string GetWebHookUrl();
        bool GetCustomWidgetsEnabled();
        string GetLocaleOverride();
        string GetLocaleDateTimeOverride();
        string GetMOTD();
        string GetLogoUrl();
        string GetSmallLogoUrl();
        string GetServerLanguage();
        bool GetServerDisabledRegistration();
        bool GetServerEnableShopSupplies();
        bool GetServerAuthEnabled();
        bool GetEnableRootUserOIDC();
        string GetServerPostgresConnection();
        string GetAllowedFileUploadExtensions();
        string GetServerDomain();
        bool DeleteUserConfig(int userId);
        bool GetInvariantApi();
        bool GetServerOpenRegistration();
        string GetDefaultReminderEmail();
        int GetAuthCookieLifeSpan();
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

        public KestrelAppConfig GetKestrelAppConfig()
        {
            KestrelAppConfig kestrelConfig = _config.GetSection("Kestrel").Get<KestrelAppConfig>() ?? new KestrelAppConfig();
            return kestrelConfig;
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
        public string GetServerDomain()
        {
            var domain = CheckString("LUBELOGGER_DOMAIN");
            return domain;
        }
        public string GetLocaleOverride()
        {
            var locale = CheckString("LUBELOGGER_LOCALE_OVERRIDE");
            return locale;
        }
        public string GetLocaleDateTimeOverride()
        {
            var locale = CheckString("LUBELOGGER_LOCALE_DT_OVERRIDE");
            return locale;
        }
        public bool GetServerOpenRegistration()
        {
            return CheckBool(CheckString("LUBELOGGER_OPEN_REGISTRATION"));
        }
        public int GetAuthCookieLifeSpan()
        {
            var lifespan = CheckString("LUBELOGGER_COOKIE_LIFESPAN", StaticHelper.DefaultCookieLifeSpan);
            if (!string.IsNullOrWhiteSpace(lifespan) && int.TryParse(lifespan, out int lifespandays))
            {
                if (lifespandays > 90) //max 90 days because that is the max lifetime of the DPAPI keys
                {
                    lifespandays = 90;
                }
                if (lifespandays < 1) //min 1 day because cookie lifespan is incremented in days for our implementation
                {
                    lifespandays = 1;
                }
                return lifespandays;
            } 
            else
            {
                return int.Parse(StaticHelper.DefaultCookieLifeSpan); //default is 30 days for when remember me is selected.
            }
        }
        public bool GetServerAuthEnabled()
        {
            return CheckBool(CheckString(nameof(UserConfig.EnableAuth)));
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
            var logoUrl = CheckString("LUBELOGGER_LOGO_URL", StaticHelper.DefaultLogoPath);
            return logoUrl;
        }
        public string GetSmallLogoUrl()
        {
            var logoUrl = CheckString("LUBELOGGER_LOGO_SMALL_URL", StaticHelper.DefaultSmallLogoPath);
            return logoUrl;
        }
        public string GetDefaultReminderEmail()
        {
            var reminderEmail = CheckString(nameof(ServerConfig.DefaultReminderEmail));
            return reminderEmail;
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
            var rootEmail = CheckString(nameof(ServerConfig.DefaultReminderEmail));
            var rootUserOIDC = CheckBool(CheckString(nameof(ServerConfig.EnableRootUserOIDC)));
            if (!rootUserOIDC || string.IsNullOrWhiteSpace(rootEmail))
            {
                return false;
            }
            return email == rootEmail;
        }
        public bool GetEnableRootUserOIDC()
        {
            var rootUserOIDC = CheckBool(CheckString(nameof(ServerConfig.EnableRootUserOIDC)));
            return rootUserOIDC;
        }
        public string GetServerLanguage()
        {
            var serverLanguage = CheckString(nameof(UserConfig.UserLanguage), "en_US");
            return serverLanguage;
        }
        public bool GetServerDisabledRegistration()
        {
            var registrationDisabled = CheckBool(CheckString(nameof(ServerConfig.DisableRegistration)));
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
        public bool SaveServerConfig(ServerConfig serverConfig)
        {
            //nullify default values
            if (string.IsNullOrWhiteSpace(serverConfig.PostgresConnection))
            {
                serverConfig.PostgresConnection = null;
            }
            if (string.IsNullOrWhiteSpace(serverConfig.LocaleOverride))
            {
                serverConfig.LocaleOverride = null;
                serverConfig.LocaleDateTimeOverride = null;
            }
            if (string.IsNullOrWhiteSpace(serverConfig.LocaleDateTimeOverride))
            {
                serverConfig.LocaleDateTimeOverride = null;
            }
            if (serverConfig.AllowedFileExtensions == StaticHelper.DefaultAllowedFileExtensions || string.IsNullOrWhiteSpace(serverConfig.AllowedFileExtensions))
            {
                serverConfig.AllowedFileExtensions = null;
            }
            if (serverConfig.CustomLogoURL == StaticHelper.DefaultLogoPath || string.IsNullOrWhiteSpace(serverConfig.CustomLogoURL))
            {
                serverConfig.CustomLogoURL = null;
            }
            if (serverConfig.CustomSmallLogoURL == StaticHelper.DefaultSmallLogoPath || string.IsNullOrWhiteSpace(serverConfig.CustomSmallLogoURL))
            {
                serverConfig.CustomSmallLogoURL = null;
            }
            if (string.IsNullOrWhiteSpace(serverConfig.MessageOfTheDay))
            {
                serverConfig.MessageOfTheDay = null;
            }
            if (string.IsNullOrWhiteSpace(serverConfig.WebHookURL))
            {
                serverConfig.WebHookURL = null;
            }
            if (string.IsNullOrWhiteSpace(serverConfig.ServerURL))
            {
                serverConfig.ServerURL = null;
            }
            if (serverConfig.CustomWidgetsEnabled.HasValue && !serverConfig.CustomWidgetsEnabled.Value)
            {
                serverConfig.CustomWidgetsEnabled = null;
            }
            if (serverConfig.InvariantAPIEnabled.HasValue && !serverConfig.InvariantAPIEnabled.Value)
            {
                serverConfig.InvariantAPIEnabled = null;
            }
            if (string.IsNullOrWhiteSpace(serverConfig.SMTPConfig?.EmailServer ?? string.Empty))
            {
                serverConfig.SMTPConfig = null;
            }
            if (string.IsNullOrWhiteSpace(serverConfig.OIDCConfig?.Name ?? string.Empty))
            {
                serverConfig.OIDCConfig = null;
            }
            if (serverConfig.OpenRegistration.HasValue && !serverConfig.OpenRegistration.Value)
            {
                serverConfig.OpenRegistration = null;
            }
            if (serverConfig.DisableRegistration.HasValue && !serverConfig.DisableRegistration.Value)
            {
                serverConfig.DisableRegistration = null;
            }
            if (string.IsNullOrWhiteSpace(serverConfig.DefaultReminderEmail))
            {
                serverConfig.DefaultReminderEmail = null;
            }
            if (serverConfig.EnableRootUserOIDC.HasValue && !serverConfig.EnableRootUserOIDC.Value)
            {
                serverConfig.EnableRootUserOIDC = null;
            }
            if (serverConfig.CookieLifeSpan == StaticHelper.DefaultCookieLifeSpan || string.IsNullOrWhiteSpace(serverConfig.CookieLifeSpan))
            {
                serverConfig.CookieLifeSpan = null;
            }
            if (serverConfig.KestrelAppConfig != null)
            {
                if (serverConfig.KestrelAppConfig.Endpoints.Http != null)
                {
                    //validate http endpoint
                    if (string.IsNullOrWhiteSpace(serverConfig.KestrelAppConfig.Endpoints.Http.Url))
                    {
                        serverConfig.KestrelAppConfig.Endpoints.Http = null;
                    }
                }
                if (serverConfig.KestrelAppConfig.Endpoints.HttpsInlineCertFile != null)
                {
                    //https endpoint provided
                    if (string.IsNullOrWhiteSpace(serverConfig.KestrelAppConfig.Endpoints.HttpsInlineCertFile.Url))
                    {
                        serverConfig.KestrelAppConfig.Endpoints.HttpsInlineCertFile = null;
                    }
                    else if (serverConfig.KestrelAppConfig.Endpoints.HttpsInlineCertFile.Certificate != null)
                    {
                        if (string.IsNullOrWhiteSpace(serverConfig.KestrelAppConfig.Endpoints.HttpsInlineCertFile.Certificate.Password))
                        {
                            //cert not null but password is null
                            serverConfig.KestrelAppConfig.Endpoints.HttpsInlineCertFile.Certificate.Password = null;
                        }
                        if (string.IsNullOrWhiteSpace(serverConfig.KestrelAppConfig.Endpoints.HttpsInlineCertFile.Certificate.Path))
                        {
                            //cert not null but path is null
                            serverConfig.KestrelAppConfig.Endpoints.HttpsInlineCertFile.Certificate = null;
                        }
                    }
                }
                if (serverConfig.KestrelAppConfig.Endpoints.Http == null && serverConfig.KestrelAppConfig.Endpoints.HttpsInlineCertFile == null)
                {
                    //if no endpoints are provided
                    serverConfig.KestrelAppConfig = null;
                }
            }
            try
            {
                File.WriteAllText(StaticHelper.ServerConfigPath, JsonSerializer.Serialize(serverConfig));
                return true;
            } catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                return false;
            }
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
                ShowCalendar = CheckBool(CheckString(nameof(UserConfig.ShowCalendar))),
                EnableExtraFieldColumns = CheckBool(CheckString(nameof(UserConfig.EnableExtraFieldColumns))),
                VisibleTabs = _config.GetSection(nameof(UserConfig.VisibleTabs)).Get<List<ImportMode>>() ?? new UserConfig().VisibleTabs,
                TabOrder = _config.GetSection(nameof(UserConfig.TabOrder)).Get<List<ImportMode>>() ?? new UserConfig().TabOrder,
                UserColumnPreferences = _config.GetSection(nameof(UserConfig.UserColumnPreferences)).Get<List<UserColumnPreference>>() ?? new List<UserColumnPreference>(),
                DefaultTab = (ImportMode)int.Parse(CheckString(nameof(UserConfig.DefaultTab), "8")),
                ShowVehicleThumbnail = CheckBool(CheckString(nameof(UserConfig.ShowVehicleThumbnail))),
                ShowSearch = CheckBool(CheckString(nameof(UserConfig.ShowSearch)))
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

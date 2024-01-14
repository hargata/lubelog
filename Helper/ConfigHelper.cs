using CarCareTracker.Models;

namespace CarCareTracker.Helper
{
    public interface IConfigHelper
    {
        UserConfig GetUserConfig(bool userIsAdmin, int userId);
    }
    public class ConfigHelper : IConfigHelper
    {
        private readonly IConfiguration _config;
        public ConfigHelper(IConfiguration serverConfiguration)
        {
            _config = serverConfiguration;
        }
        public UserConfig GetUserConfig(bool isRootUser, int userId)
        {
            if (isRootUser)
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
                return serverConfig;
            } else
            {
                return new UserConfig();
            }
        }
    }
}

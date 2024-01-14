using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IUserConfigDataAccess
    {
        public UserConfigData GetUserConfig(int userId);
        public bool SaveUserConfig(UserConfigData userConfigData);
        public bool DeleteUserConfig(int userId);
    }
}

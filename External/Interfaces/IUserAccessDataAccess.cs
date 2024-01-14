using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IUserAccessDataAccess
    {
        List<UserAccess> GetUserAccessByUserId(int userId);
        UserAccess GetUserAccessByVehicleAndUserId(int userId, int vehicleId);
        List<UserAccess> GetUserAccessByVehicleId(int vehicleId);
        bool SaveUserAccess(UserAccess userAccess);
        bool DeleteUserAccess(int userAccessId);
        bool DeleteAllAccessRecordsByVehicleId(int vehicleId);
        bool DeleteAllAccessRecordsByUserId(int userId);
    }
}

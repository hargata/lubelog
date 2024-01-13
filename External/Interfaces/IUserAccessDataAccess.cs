using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IUserAccessDataAccess
    {
        UserAccess GetUserAccessByVehicleAndUserId(int vehicleId, int userId);
        List<UserAccess> GetUserAccessByUserId(int userId);
        List<UserAccess> GetUserAccessByVehicleId(int vehicleId);
        bool SaveUserAccess(UserAccess userAccess);
        bool DeleteUserAccess(int userAccessId);
        bool DeleteAllAccessRecordsByVehicleId(int vehicleId);
        bool DeleteAllAccessRecordsByUserId(int userId);
    }
}

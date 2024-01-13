using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace CarCareTracker.Logic
{
    public interface IUserLogic
    {
        List<Vehicle> FilterUserVehicles(List<Vehicle> results, int userId);
        bool UserCanAccessVehicle(int userId, int vehicleId);
        bool UserCanEditVehicle(int userId, int vehicleId);
    }
    public class UserLogic: IUserLogic
    {
        private readonly IUserAccessDataAccess _userAccess;
        public UserLogic(IUserAccessDataAccess userAccess) { 
            _userAccess = userAccess;
        }
        public List<Vehicle> FilterUserVehicles(List<Vehicle> results, int userId)
        {
            var accessibleVehicles = _userAccess.GetUserAccessByUserId(userId);
            if (accessibleVehicles.Any())
            {
                var vehicleIds = accessibleVehicles.Select(x => x.VehicleId);
                return results.Where(x => vehicleIds.Contains(x.Id)).ToList();
            }
            else
            {
                return new List<Vehicle>();
            }
        }
        public bool UserCanAccessVehicle(int userId, int vehicleId)
        {
            var userAccess = _userAccess.GetUserAccessByVehicleAndUserId(userId, vehicleId);
            if (userAccess != null)
            {
                return true;
            }
            return false;
        }
        public bool UserCanEditVehicle(int userId, int vehicleId)
        {
            var userAccess = _userAccess.GetUserAccessByVehicleAndUserId(userId, vehicleId);
            if (userAccess != null && userAccess.AccessType == UserAccessType.Editor)
            {
                return true;
            }
            return false;
        }
    }
}

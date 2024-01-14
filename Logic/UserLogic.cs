using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace CarCareTracker.Logic
{
    public interface IUserLogic
    {
        List<UserCollaborator> GetCollaboratorsForVehicle(int vehicleId);
        bool AddUserAccessToVehicle(int userId, int vehicleId, UserAccessType accessType);
        List<Vehicle> FilterUserVehicles(List<Vehicle> results, int userId);
        bool UserCanAccessVehicle(int userId, int vehicleId);
        bool UserCanEditVehicle(int userId, int vehicleId);
        bool DeleteAllAccessToVehicle(int vehicleId);
        bool DeleteAllAccessToUser(int userId);
    }
    public class UserLogic: IUserLogic
    {
        private readonly IUserAccessDataAccess _userAccess;
        private readonly IUserRecordDataAccess _userData;
        public UserLogic(IUserAccessDataAccess userAccess,
            IUserRecordDataAccess userData) { 
            _userAccess = userAccess;
            _userData = userData;
        }
        public List<UserCollaborator> GetCollaboratorsForVehicle(int vehicleId)
        {
            var result = _userAccess.GetUserAccessByVehicleId(vehicleId);
            var convertedResult = new List<UserCollaborator>();
            //convert useraccess to usercollaborator
            foreach(UserAccess userAccess in result)
            {
                var userCollaborator = new UserCollaborator
                {
                    UserName = _userData.GetUserRecordById(userAccess.Id.UserId).UserName,
                    AccessType = userAccess.AccessType,
                    UserVehicle = userAccess.Id
                };
                convertedResult.Add(userCollaborator);
            }
            return convertedResult;
        }
        public bool AddUserAccessToVehicle(int userId, int vehicleId, UserAccessType accessType)
        {
            if (userId == -1)
            {
                return true;
            }
            var userVehicle = new UserVehicle { UserId = userId, VehicleId = vehicleId };
            var userAccess = new UserAccess { Id = userVehicle, AccessType = accessType };
            var result = _userAccess.SaveUserAccess(userAccess);
            return result;
        }
        public List<Vehicle> FilterUserVehicles(List<Vehicle> results, int userId)
        {
            //user is root user.
            if (userId == -1)
            {
                return results;
            }
            var accessibleVehicles = _userAccess.GetUserAccessByUserId(userId);
            if (accessibleVehicles.Any())
            {
                var vehicleIds = accessibleVehicles.Select(x => x.Id.VehicleId);
                return results.Where(x => vehicleIds.Contains(x.Id)).ToList();
            }
            else
            {
                return new List<Vehicle>();
            }
        }
        public bool UserCanAccessVehicle(int userId, int vehicleId)
        {
            if (userId == -1)
            {
                return true;
            }
            var userAccess = _userAccess.GetUserAccessByVehicleAndUserId(userId, vehicleId);
            if (userAccess != null)
            {
                return true;
            }
            return false;
        }
        public bool UserCanEditVehicle(int userId, int vehicleId)
        {
            if (userId == -1)
            {
                return true;
            }
            var userAccess = _userAccess.GetUserAccessByVehicleAndUserId(userId, vehicleId);
            if (userAccess != null && userAccess.AccessType == UserAccessType.Editor)
            {
                return true;
            }
            return false;
        }
        public bool DeleteAllAccessToVehicle(int vehicleId)
        {
            var result = _userAccess.DeleteAllAccessRecordsByVehicleId(vehicleId);
            return result;
        }
        public bool DeleteAllAccessToUser(int userId)
        {
            var result = _userAccess.DeleteAllAccessRecordsByUserId(userId);
            return result;
        }
    }
}

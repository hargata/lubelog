using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace CarCareTracker.Logic
{
    public interface IUserLogic
    {
        List<UserCollaborator> GetCollaboratorsForVehicle(int vehicleId);
        bool AddUserAccessToVehicle(int userId, int vehicleId);
        bool DeleteCollaboratorFromVehicle(int userId, int vehicleId);
        OperationResponse AddCollaboratorToVehicle(int vehicleId, string username);
        List<Vehicle> FilterUserVehicles(List<Vehicle> results, int userId);
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
                    UserVehicle = userAccess.Id
                };
                convertedResult.Add(userCollaborator);
            }
            return convertedResult;
        }
        public OperationResponse AddCollaboratorToVehicle(int vehicleId, string username)
        {
            //try to find existing user.
            var existingUser = _userData.GetUserRecordByUserName(username);
            if (existingUser.Id != default)
            {
                //user exists.
                //check if user is already a collaborator
                var userAccess = _userAccess.GetUserAccessByVehicleAndUserId(existingUser.Id, vehicleId);
                if (userAccess != null)
                {
                    return new OperationResponse { Success = false, Message = "User is already a collaborator" };
                }
                var result = AddUserAccessToVehicle(existingUser.Id, vehicleId);
                if (result)
                {
                    return new OperationResponse { Success = true, Message = "Collaborator Added" };
                }
                return new OperationResponse { Success = false, Message = StaticHelper.GenericErrorMessage };
            }
            return new OperationResponse { Success = false, Message = $"Unable to find user {username} in the system" };
        }
        public bool DeleteCollaboratorFromVehicle(int userId, int vehicleId)
        {
            var result = _userAccess.DeleteUserAccess(userId, vehicleId);
            return result;
        }
        public bool AddUserAccessToVehicle(int userId, int vehicleId)
        {
            if (userId == -1)
            {
                return true;
            }
            var userVehicle = new UserVehicle { UserId = userId, VehicleId = vehicleId };
            var userAccess = new UserAccess { Id = userVehicle };
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
        public bool UserCanEditVehicle(int userId, int vehicleId)
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

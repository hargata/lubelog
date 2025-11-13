using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;

namespace CarCareTracker.Logic
{
    public interface IUserLogic
    {
        List<UserCollaborator> GetCollaboratorsForVehicle(int vehicleId);
        bool AddUserAccessToVehicle(int userId, int vehicleId);
        bool DeleteCollaboratorFromVehicle(int userId, int vehicleId);
        OperationResponse DeleteCollaboratorFromVehicle(int vehicleId, string username);
        OperationResponse AddCollaboratorToVehicle(int vehicleId, string username);
        List<Vehicle> FilterUserVehicles(List<Vehicle> results, int userId);
        bool UserCanEditVehicle(int userId, int vehicleId, HouseholdPermission permission);
        bool UserCanDirectlyEditVehicle(int userId, int vehicleId);
        bool DeleteAllAccessToVehicle(int vehicleId);
        bool DeleteAllAccessToUser(int userId);
        List<UserHouseholdViewModel> GetHouseholdForParentUserId(int parentUserId);
        OperationResponse AddUserToHousehold(int parentUserId, string childUsername);
        bool UpdateUserHousehold(int parentUserId, int childUserId, List<HouseholdPermission> permissions);
        bool DeleteUserFromHousehold(int parentUserId, int childUserId);
        bool DeleteAllHouseholdByParentUserId(int parentUserId);
        bool DeleteAllHouseholdByChildUserId(int childUserId);
    }
    public class UserLogic: IUserLogic
    {
        private readonly IUserAccessDataAccess _userAccess;
        private readonly IUserRecordDataAccess _userData;
        private readonly IUserHouseholdDataAccess _userHouseholdData;
        public UserLogic(IUserAccessDataAccess userAccess,
            IUserRecordDataAccess userData,
            IUserHouseholdDataAccess userHouseholdData) { 
            _userAccess = userAccess;
            _userData = userData;
            _userHouseholdData = userHouseholdData;
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
                    return OperationResponse.Failed("User is already a collaborator");
                }
                var result = AddUserAccessToVehicle(existingUser.Id, vehicleId);
                if (result)
                {
                    return OperationResponse.Succeed("Collaborator Added");
                }
                return OperationResponse.Failed();
            }
            return OperationResponse.Failed($"Unable to find user {username} in the system");
        }
        public OperationResponse DeleteCollaboratorFromVehicle(int vehicleId, string username)
        {
            //try to find existing user.
            var existingUser = _userData.GetUserRecordByUserName(username);
            if (existingUser.Id != default)
            {
                //user exists.
                //check if user is already a collaborator
                var userAccess = _userAccess.GetUserAccessByVehicleAndUserId(existingUser.Id, vehicleId);
                if (userAccess == null)
                {
                    return OperationResponse.Failed("User doesn't have access to this vehicle");
                }
                var result = _userAccess.DeleteUserAccess(existingUser.Id, vehicleId);
                if (result)
                {
                    return OperationResponse.Succeed("Collaborator Removed");
                }
                return OperationResponse.Failed();
            }
            return OperationResponse.Failed($"Unable to find user {username} in the system");
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
            List<int> userIds = new List<int> { userId };
            List<int> vehicleIds = new List<int>();
            var userHouseholds = _userHouseholdData.GetUserHouseholdByChildUserId(userId);
            if (userHouseholds.Any())
            {
                //add parent's user ids
                userIds.AddRange(userHouseholds.Select(x => x.Id.ParentUserId));
            }
            foreach(int userIdToCheck in userIds)
            {
                var accessibleVehicles = _userAccess.GetUserAccessByUserId(userIdToCheck);
                if (accessibleVehicles.Any())
                {
                    vehicleIds.AddRange(accessibleVehicles.Select(x => x.Id.VehicleId));
                }
            }
            if (vehicleIds.Any())
            {
                return results.Where(x => vehicleIds.Contains(x.Id)).ToList();
            }
            else
            {
                return new List<Vehicle>();
            }
        }
        public bool UserCanEditVehicle(int userId, int vehicleId, HouseholdPermission permission)
        {
            if (userId == -1)
            {
                return true;
            }
            List<int> userIds = new List<int> { userId };
            var userHouseholds = _userHouseholdData.GetUserHouseholdByChildUserId(userId);
            if (userHouseholds.Any())
            {
                //add parent's user ids
                userIds.AddRange(userHouseholds.Select(x => x.Id.ParentUserId));
            }
            foreach (int userIdToCheck in userIds)
            {
                var userAccess = _userAccess.GetUserAccessByVehicleAndUserId(userIdToCheck, vehicleId);
                if (userAccess != null && userAccess.Id.UserId == userId && userAccess.Id.VehicleId == vehicleId)
                {
                    //full collaborator, not through household
                    return true;
                }
                else if (userAccess != null && userAccess.Id.UserId == userIdToCheck && userAccess.Id.VehicleId == vehicleId)
                {
                    //every member in a household has permission to view vehicles
                    if (permission == HouseholdPermission.View)
                    {
                        return true;
                    } else
                    {
                        return userHouseholds.First(x => x.Id.ParentUserId == userIdToCheck && x.Id.ChildUserId == userId).Permissions.Contains(permission);
                    }
                }
            }
            return false;
        }
        public bool UserCanDirectlyEditVehicle(int userId, int vehicleId)
        {
            if (userId == -1)
            {
                return true;
            }
            var userAccess = _userAccess.GetUserAccessByVehicleAndUserId(userId, vehicleId);
            if (userAccess != null && userAccess.Id.UserId == userId && userAccess.Id.VehicleId == vehicleId)
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
        public List<UserHouseholdViewModel> GetHouseholdForParentUserId(int parentUserId)
        {
            var result = _userHouseholdData.GetUserHouseholdByParentUserId(parentUserId);
            var convertedResult = new List<UserHouseholdViewModel>();
            //convert useraccess to usercollaborator
            foreach (UserHousehold userHouseholdAccess in result)
            {
                var userCollaborator = new UserHouseholdViewModel
                {
                    UserName = _userData.GetUserRecordById(userHouseholdAccess.Id.ChildUserId).UserName,
                    UserHousehold = userHouseholdAccess
                };
                convertedResult.Add(userCollaborator);
            }
            return convertedResult;
        }
        public OperationResponse AddUserToHousehold(int parentUserId, string childUsername)
        {
            //attempting to add to root user
            if (parentUserId == -1)
            {
                return OperationResponse.Failed("Root user household not allowed");
            }
            //try to find existing user.
            var existingUser = _userData.GetUserRecordByUserName(childUsername);
            if (existingUser.Id != default)
            {
                //user exists.
                //check if user is trying to add themselves
                if (parentUserId == existingUser.Id)
                {
                    return OperationResponse.Failed("Cannot add user to their own household");
                }
                //check if user already belongs to the household
                var householdAccess = _userHouseholdData.GetUserHouseholdByParentAndChildUserId(parentUserId, existingUser.Id);
                if (householdAccess != null && householdAccess.Id.ChildUserId == existingUser.Id && householdAccess.Id.ParentUserId == parentUserId)
                {
                    return OperationResponse.Failed("User already belongs to this household");
                }
                //check if a circular dependency will exist
                var circularHouseholdAccess = _userHouseholdData.GetUserHouseholdByParentAndChildUserId(existingUser.Id, parentUserId);
                if (circularHouseholdAccess != null && circularHouseholdAccess.Id.ChildUserId == parentUserId && circularHouseholdAccess.Id.ParentUserId == existingUser.Id)
                {
                    return OperationResponse.Failed("Circular dependency is not allowed");
                }
                var result = _userHouseholdData.SaveUserHousehold(new UserHousehold { Id = new HouseholdAccess { ParentUserId = parentUserId, ChildUserId = existingUser.Id} });
                if (result)
                {
                    return OperationResponse.Succeed("User Added to Household");
                }
                return OperationResponse.Failed();
            }
            return OperationResponse.Failed($"Unable to find user {childUsername} in the system");
        }
        public bool UpdateUserHousehold(int parentUserId, int childUserId, List<HouseholdPermission> permissions)
        {
            var existingHousehold = _userHouseholdData.GetUserHouseholdByParentAndChildUserId(parentUserId, childUserId);
            if (existingHousehold != null && existingHousehold.Id.ChildUserId == childUserId && existingHousehold.Id.ParentUserId == parentUserId)
            {
                existingHousehold.Permissions = permissions;
                var result = _userHouseholdData.SaveUserHousehold(existingHousehold);
                return result;
            }
            return false;
        }
        public bool DeleteUserFromHousehold(int parentUserId, int childUserId)
        {
            var result = _userHouseholdData.DeleteUserHousehold(parentUserId, childUserId);
            return result;
        }
        public bool DeleteAllHouseholdByParentUserId(int parentUserId)
        {
            var result = _userHouseholdData.DeleteAllHouseholdRecordsByParentUserId(parentUserId);
            return result;
        }
        public bool DeleteAllHouseholdByChildUserId(int childUserId)
        {
            var result = _userHouseholdData.DeleteAllHouseholdRecordsByChildUserId(childUserId);
            return result;
        }
    }
}

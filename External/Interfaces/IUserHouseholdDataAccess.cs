using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IUserHouseholdDataAccess
    {
        List<UserHousehold> GetUserHouseholdByParentUserId(int parentUserId);
        List<UserHousehold> GetUserHouseholdByChildUserId(int childUserId);
        UserHousehold GetUserHouseholdByParentAndChildUserId(int parentUserId, int childUserId);
        bool SaveUserHousehold(UserHousehold userHousehold);
        bool DeleteUserHousehold(int parentUserId, int childUserId);
        bool DeleteAllHouseholdRecordsByParentUserId(int parentUserId);
        bool DeleteAllHouseholdRecordsByChildUserId(int childUserId);
    }
}
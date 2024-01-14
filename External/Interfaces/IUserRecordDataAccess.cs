using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IUserRecordDataAccess
    {
        public List<UserData> GetUsers();
        public UserData GetUserRecordByUserName(string userName);
        public UserData GetUserRecordByEmailAddress(string emailAddress);
        public UserData GetUserRecordById(int userId);
        public bool SaveUserRecord(UserData userRecord);
        public bool DeleteUserRecord(int userId);
    }
}
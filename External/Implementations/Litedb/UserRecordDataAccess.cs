using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class UserRecordDataAccess : IUserRecordDataAccess
    {
        private LiteDatabase db { get; set; }
        private static string tableName = "userrecords";
        public UserRecordDataAccess(ILiteDBInjection liteDB)
        {
            db = liteDB.GetLiteDB();
        }
        public List<UserData> GetUsers()
        {
            var table = db.GetCollection<UserData>(tableName);
            return table.FindAll().ToList();
        }
        public UserData GetUserRecordByUserName(string userName)
        {
            var table = db.GetCollection<UserData>(tableName);
            var userRecord = table.FindOne(Query.EQ(nameof(UserData.UserName), userName));
            return userRecord ?? new UserData();
        }
        public UserData GetUserRecordByEmailAddress(string emailAddress)
        {
            var table = db.GetCollection<UserData>(tableName);
            var userRecord = table.FindOne(Query.EQ(nameof(UserData.EmailAddress), emailAddress));
            return userRecord ?? new UserData();
        }
        public UserData GetUserRecordById(int userId)
        {
            var table = db.GetCollection<UserData>(tableName);
            var userRecord = table.FindById(userId);
            return userRecord ?? new UserData();
        }
        public bool SaveUserRecord(UserData userRecord)
        {
            var table = db.GetCollection<UserData>(tableName);
            table.Upsert(userRecord);
            db.Checkpoint();
            return true;
        }
        public bool DeleteUserRecord(int userId)
        {
            var table = db.GetCollection<UserData>(tableName);
            table.Delete(userId);
            db.Checkpoint();
            return true;
        }
    }
}
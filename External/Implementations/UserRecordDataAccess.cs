using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class UserRecordDataAccess : IUserRecordDataAccess
    {
        private static string dbName = StaticHelper.DbName;
        private static string tableName = "userrecords";
        public List<UserData> GetUsers()
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UserData>(tableName);
                return table.FindAll().ToList();
            };
        }
        public UserData GetUserRecordByUserName(string userName)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UserData>(tableName);
                var userRecord = table.FindOne(Query.EQ(nameof(UserData.UserName), userName));
                return userRecord ?? new UserData();
            };
        }
        public UserData GetUserRecordByEmailAddress(string emailAddress)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UserData>(tableName);
                var userRecord = table.FindOne(Query.EQ(nameof(UserData.EmailAddress), emailAddress));
                return userRecord ?? new UserData();
            };
        }
        public UserData GetUserRecordById(int userId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UserData>(tableName);
                var userRecord = table.FindById(userId);
                return userRecord ?? new UserData();
            };
        }
        public bool SaveUserRecord(UserData userRecord)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UserData>(tableName);
                table.Upsert(userRecord);
                return true;
            };
        }
        public bool DeleteUserRecord(int userId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UserData>(tableName);
                table.Delete(userId);
                return true;
            };
        }
    }
}
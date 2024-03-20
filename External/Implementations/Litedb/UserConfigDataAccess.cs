using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using CarCareTracker.Helper;

namespace CarCareTracker.External.Implementations
{
    public class UserConfigDataAccess : IUserConfigDataAccess
    {
        private ILiteDBHelper _liteDB { get; set; }
        private static string tableName = "userconfigrecords";
        public UserConfigDataAccess(ILiteDBHelper liteDB)
        {
           _liteDB = liteDB;
        }
        public UserConfigData GetUserConfig(int userId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<UserConfigData>(tableName);
            return table.FindById(userId);
        }
        public bool SaveUserConfig(UserConfigData userConfigData)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<UserConfigData>(tableName);
            table.Upsert(userConfigData);
            db.Checkpoint();
            return true;
        }
        public bool DeleteUserConfig(int userId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<UserConfigData>(tableName);
            table.Delete(userId);
            db.Checkpoint();
            return true;
        }
    }
}

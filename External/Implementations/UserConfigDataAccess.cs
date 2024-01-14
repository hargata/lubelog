using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CarCareTracker.External.Implementations
{
    public class UserConfigDataAccess: IUserConfigDataAccess
    {
        private static string dbName = StaticHelper.DbName;
        private static string tableName = "userconfigrecords";
        public UserConfigData GetUserConfig(int userId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UserConfigData>(tableName);
                return table.FindById(userId);
            };
        }
        public bool SaveUserConfig(UserConfigData userConfigData)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UserConfigData>(tableName);
                table.Upsert(userConfigData);
                return true;
            };
        }
        public bool DeleteUserConfig(int userId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UserConfigData>(tableName);
                table.Delete(userId);
                return true;
            };
        }
    }
}

using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class UserAccessDataAccess : IUserAccessDataAccess
    {
        private static string dbName = StaticHelper.DbName;
        private static string tableName = "useraccessrecords";
        /// <summary>
        /// Gets a list of vehicles user have access to.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<UserAccess> GetUserAccessByUserId(int userId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UserAccess>(tableName);
                return table.Find(x=>x.Id.UserId == userId).ToList();
            };
        }
        public UserAccess GetUserAccessByVehicleAndUserId(int userId, int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UserAccess>(tableName);
                return table.Find(x => x.Id.UserId == userId && x.Id.VehicleId == vehicleId).FirstOrDefault();
            };
        }
        public List<UserAccess> GetUserAccessByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UserAccess>(tableName);
                return table.Find(x => x.Id.VehicleId == vehicleId).ToList();
            };
        }
        public bool SaveUserAccess(UserAccess userAccess)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UserAccess>(tableName);
                table.Upsert(userAccess);
                return true;
            };
        }
        public bool DeleteUserAccess(int userId, int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UserAccess>(tableName);
                table.DeleteMany(x => x.Id.UserId == userId && x.Id.VehicleId == vehicleId);
                return true;
            };
        }
        /// <summary>
        /// Delete all access records when a vehicle is deleted.
        /// </summary>
        /// <param name="vehicleId"></param>
        /// <returns></returns>
        public bool DeleteAllAccessRecordsByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UserAccess>(tableName);
                table.DeleteMany(x=>x.Id.VehicleId == vehicleId);
                return true;
            };
        }
        /// <summary>
        /// Delee all access records when a user is deleted.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool DeleteAllAccessRecordsByUserId(int userId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UserAccess>(tableName);
                table.DeleteMany(x => x.Id.UserId == userId);
                return true;
            };
        }
    }
}
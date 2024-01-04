using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class CollisionRecordDataAccess : ICollisionRecordDataAccess
    {
        private static string dbName = "cartracker.db";
        private static string tableName = "collisionrecords";
        public List<CollisionRecord> GetCollisionRecordsByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<CollisionRecord>(tableName);
                var collisionRecords = table.Find(Query.EQ(nameof(CollisionRecord.VehicleId), vehicleId));
                return collisionRecords.ToList() ?? new List<CollisionRecord>();
            };
        }
        public CollisionRecord GetCollisionRecordById(int collisionRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<CollisionRecord>(tableName);
                return table.FindById(collisionRecordId);
            };
        }
        public bool DeleteCollisionRecordById(int collisionRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<CollisionRecord>(tableName);
                table.Delete(collisionRecordId);
                return true;
            };
        }
        public bool SaveCollisionRecordToVehicle(CollisionRecord collisionRecord)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<CollisionRecord>(tableName);
                table.Upsert(collisionRecord);
                return true;
            };
        }
        public bool DeleteAllCollisionRecordsByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<CollisionRecord>(tableName);
                var collisionRecords = table.DeleteMany(Query.EQ(nameof(CollisionRecord.VehicleId), vehicleId));
                return true;
            };
        }
    }
}

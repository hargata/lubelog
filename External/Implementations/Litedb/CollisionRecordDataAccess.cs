using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class CollisionRecordDataAccess : ICollisionRecordDataAccess
    {
        private LiteDatabase db { get; set; }
        private static string tableName = "collisionrecords";
        public CollisionRecordDataAccess(ILiteDBInjection liteDB)
        {
            db = liteDB.GetLiteDB();
        }
        public List<CollisionRecord> GetCollisionRecordsByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<CollisionRecord>(tableName);
            var collisionRecords = table.Find(Query.EQ(nameof(CollisionRecord.VehicleId), vehicleId));
            return collisionRecords.ToList() ?? new List<CollisionRecord>();
        }
        public CollisionRecord GetCollisionRecordById(int collisionRecordId)
        {
            var table = db.GetCollection<CollisionRecord>(tableName);
            return table.FindById(collisionRecordId);
        }
        public bool DeleteCollisionRecordById(int collisionRecordId)
        {
            var table = db.GetCollection<CollisionRecord>(tableName);
            table.Delete(collisionRecordId);
            return true;
        }
        public bool SaveCollisionRecordToVehicle(CollisionRecord collisionRecord)
        {
            var table = db.GetCollection<CollisionRecord>(tableName);
            table.Upsert(collisionRecord);
            return true;
        }
        public bool DeleteAllCollisionRecordsByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<CollisionRecord>(tableName);
            var collisionRecords = table.DeleteMany(Query.EQ(nameof(CollisionRecord.VehicleId), vehicleId));
            return true;
        }
    }
}

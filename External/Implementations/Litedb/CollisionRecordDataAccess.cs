using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class CollisionRecordDataAccess : ICollisionRecordDataAccess
    {
        private ILiteDBHelper _liteDB { get; }
        private static string tableName = "collisionrecords";
        public CollisionRecordDataAccess(ILiteDBHelper liteDB)
        {
           _liteDB = liteDB;
        }
        public List<CollisionRecord> GetCollisionRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<CollisionRecord>(tableName);
            var collisionRecords = table.Find(Query.EQ(nameof(CollisionRecord.VehicleId), vehicleId));
            return collisionRecords.ToList() ?? new List<CollisionRecord>();
        }
        public CollisionRecord GetCollisionRecordById(int collisionRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<CollisionRecord>(tableName);
            return table.FindById(collisionRecordId);
        }
        public bool DeleteCollisionRecordById(int collisionRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<CollisionRecord>(tableName);
            table.Delete(collisionRecordId);
            db.Checkpoint();
            return true;
        }
        public bool SaveCollisionRecordToVehicle(CollisionRecord collisionRecord)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<CollisionRecord>(tableName);
            table.Upsert(collisionRecord);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAllCollisionRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<CollisionRecord>(tableName);
            table.DeleteMany(Query.EQ(nameof(CollisionRecord.VehicleId), vehicleId));
            db.Checkpoint();
            return true;
        }
    }
}

using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class SupplyRecordDataAccess : ISupplyRecordDataAccess
    {
        private ILiteDBHelper _liteDB { get; }
        private static string tableName = "supplyrecords";
        public SupplyRecordDataAccess(ILiteDBHelper liteDB)
        {
           _liteDB = liteDB;
        }
        public List<SupplyRecord> GetSupplyRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<SupplyRecord>(tableName);
            var supplyRecords = table.Find(Query.EQ(nameof(SupplyRecord.VehicleId), vehicleId));
            return supplyRecords.ToList() ?? new List<SupplyRecord>();
        }
        public SupplyRecord GetSupplyRecordById(int supplyRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<SupplyRecord>(tableName);
            return table.FindById(supplyRecordId);
        }
        public bool DeleteSupplyRecordById(int supplyRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<SupplyRecord>(tableName);
            table.Delete(supplyRecordId);
            db.Checkpoint();
            return true;
        }
        public bool SaveSupplyRecordToVehicle(SupplyRecord supplyRecord)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<SupplyRecord>(tableName);
            table.Upsert(supplyRecord);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAllSupplyRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<SupplyRecord>(tableName);
            table.DeleteMany(Query.EQ(nameof(SupplyRecord.VehicleId), vehicleId));
            db.Checkpoint();
            return true;
        }
    }
}

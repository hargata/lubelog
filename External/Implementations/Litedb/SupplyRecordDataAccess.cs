using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class SupplyRecordDataAccess : ISupplyRecordDataAccess
    {
        private LiteDatabase db { get; set; }
        private static string tableName = "supplyrecords";
        public SupplyRecordDataAccess(ILiteDBInjection liteDB)
        {
            db = liteDB.GetLiteDB();
        }
        public List<SupplyRecord> GetSupplyRecordsByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<SupplyRecord>(tableName);
            var supplyRecords = table.Find(Query.EQ(nameof(SupplyRecord.VehicleId), vehicleId));
            return supplyRecords.ToList() ?? new List<SupplyRecord>();
        }
        public SupplyRecord GetSupplyRecordById(int supplyRecordId)
        {
            var table = db.GetCollection<SupplyRecord>(tableName);
            return table.FindById(supplyRecordId);
        }
        public bool DeleteSupplyRecordById(int supplyRecordId)
        {
            var table = db.GetCollection<SupplyRecord>(tableName);
            table.Delete(supplyRecordId);
            return true;
        }
        public bool SaveSupplyRecordToVehicle(SupplyRecord supplyRecord)
        {
            var table = db.GetCollection<SupplyRecord>(tableName);
            table.Upsert(supplyRecord);
            return true;
        }
        public bool DeleteAllSupplyRecordsByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<SupplyRecord>(tableName);
            var supplyRecords = table.DeleteMany(Query.EQ(nameof(SupplyRecord.VehicleId), vehicleId));
            return true;
        }
    }
}

using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class SupplyRecordDataAccess : ISupplyRecordDataAccess
    {
        private static string dbName = StaticHelper.DbName;
        private static string tableName = "supplyrecords";
        public List<SupplyRecord> GetSupplyRecordsByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<SupplyRecord>(tableName);
                var supplyRecords = table.Find(Query.EQ(nameof(SupplyRecord.VehicleId), vehicleId));
                return supplyRecords.ToList() ?? new List<SupplyRecord>();
            };
        }
        public SupplyRecord GetSupplyRecordById(int supplyRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<SupplyRecord>(tableName);
                return table.FindById(supplyRecordId);
            };
        }
        public bool DeleteSupplyRecordById(int supplyRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<SupplyRecord>(tableName);
                table.Delete(supplyRecordId);
                return true;
            };
        }
        public bool SaveSupplyRecordToVehicle(SupplyRecord supplyRecord)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<SupplyRecord>(tableName);
                table.Upsert(supplyRecord);
                return true;
            };
        }
        public bool DeleteAllSupplyRecordsByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<SupplyRecord>(tableName);
                var supplyRecords = table.DeleteMany(Query.EQ(nameof(SupplyRecord.VehicleId), vehicleId));
                return true;
            };
        }
    }
}

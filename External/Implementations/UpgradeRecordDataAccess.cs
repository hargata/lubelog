using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class UpgradeRecordDataAccess : IUpgradeRecordDataAccess
    {
        private static string dbName = StaticHelper.DbName;
        private static string tableName = "upgraderecords";
        public List<UpgradeRecord> GetUpgradeRecordsByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UpgradeRecord>(tableName);
                var upgradeRecords = table.Find(Query.EQ(nameof(UpgradeRecord.VehicleId), vehicleId));
                return upgradeRecords.ToList() ?? new List<UpgradeRecord>();
            };
        }
        public UpgradeRecord GetUpgradeRecordById(int upgradeRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UpgradeRecord>(tableName);
                return table.FindById(upgradeRecordId);
            };
        }
        public bool DeleteUpgradeRecordById(int upgradeRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UpgradeRecord>(tableName);
                table.Delete(upgradeRecordId);
                return true;
            };
        }
        public bool SaveUpgradeRecordToVehicle(UpgradeRecord upgradeRecord)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UpgradeRecord>(tableName);
                table.Upsert(upgradeRecord);
                return true;
            };
        }
        public bool DeleteAllUpgradeRecordsByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<UpgradeRecord>(tableName);
                var upgradeRecords = table.DeleteMany(Query.EQ(nameof(UpgradeRecord.VehicleId), vehicleId));
                return true;
            };
        }
    }
}

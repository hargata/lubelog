using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class UpgradeRecordDataAccess : IUpgradeRecordDataAccess
    {
        private LiteDatabase db { get; set; }
        public UpgradeRecordDataAccess(ILiteDBInjection liteDB)
        {
            db = liteDB.GetLiteDB();
        }
        private static string tableName = "upgraderecords";
        public List<UpgradeRecord> GetUpgradeRecordsByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<UpgradeRecord>(tableName);
            var upgradeRecords = table.Find(Query.EQ(nameof(UpgradeRecord.VehicleId), vehicleId));
            return upgradeRecords.ToList() ?? new List<UpgradeRecord>();
        }
        public UpgradeRecord GetUpgradeRecordById(int upgradeRecordId)
        {
            var table = db.GetCollection<UpgradeRecord>(tableName);
            return table.FindById(upgradeRecordId);
        }
        public bool DeleteUpgradeRecordById(int upgradeRecordId)
        {
            var table = db.GetCollection<UpgradeRecord>(tableName);
            table.Delete(upgradeRecordId);
            return true;
        }
        public bool SaveUpgradeRecordToVehicle(UpgradeRecord upgradeRecord)
        {
            var table = db.GetCollection<UpgradeRecord>(tableName);
            table.Upsert(upgradeRecord);
            return true;
        }
        public bool DeleteAllUpgradeRecordsByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<UpgradeRecord>(tableName);
            var upgradeRecords = table.DeleteMany(Query.EQ(nameof(UpgradeRecord.VehicleId), vehicleId));
            return true;
        }
    }
}

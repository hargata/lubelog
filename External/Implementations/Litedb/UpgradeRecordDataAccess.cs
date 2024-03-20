using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class UpgradeRecordDataAccess : IUpgradeRecordDataAccess
    {
        private ILiteDBInjection _liteDB { get; set; }
        public UpgradeRecordDataAccess(ILiteDBInjection liteDB)
        {
           _liteDB = liteDB;
        }
        private static string tableName = "upgraderecords";
        public List<UpgradeRecord> GetUpgradeRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<UpgradeRecord>(tableName);
            var upgradeRecords = table.Find(Query.EQ(nameof(UpgradeRecord.VehicleId), vehicleId));
            return upgradeRecords.ToList() ?? new List<UpgradeRecord>();
        }
        public UpgradeRecord GetUpgradeRecordById(int upgradeRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<UpgradeRecord>(tableName);
            return table.FindById(upgradeRecordId);
        }
        public bool DeleteUpgradeRecordById(int upgradeRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<UpgradeRecord>(tableName);
            table.Delete(upgradeRecordId);
            db.Checkpoint();
            return true;
        }
        public bool SaveUpgradeRecordToVehicle(UpgradeRecord upgradeRecord)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<UpgradeRecord>(tableName);
            table.Upsert(upgradeRecord);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAllUpgradeRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<UpgradeRecord>(tableName);
            var upgradeRecords = table.DeleteMany(Query.EQ(nameof(UpgradeRecord.VehicleId), vehicleId));
            db.Checkpoint();
            return true;
        }
    }
}

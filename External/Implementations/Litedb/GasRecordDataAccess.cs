using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class GasRecordDataAccess : IGasRecordDataAccess
    {
        private ILiteDBHelper _liteDB { get; set; }
        private static string tableName = "gasrecords";
        public GasRecordDataAccess(ILiteDBHelper liteDB)
        {
           _liteDB = liteDB;
        }
        public List<GasRecord> GetGasRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<GasRecord>(tableName);
            var gasRecords = table.Find(Query.EQ(nameof(GasRecord.VehicleId), vehicleId));
            return gasRecords.ToList() ?? new List<GasRecord>();
        }
        public GasRecord GetGasRecordById(int gasRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<GasRecord>(tableName);
            return table.FindById(gasRecordId);
        }
        public bool DeleteGasRecordById(int gasRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<GasRecord>(tableName);
            table.Delete(gasRecordId);
            db.Checkpoint();
            return true;
        }
        public bool SaveGasRecordToVehicle(GasRecord gasRecord)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<GasRecord>(tableName);
            table.Upsert(gasRecord);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAllGasRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<GasRecord>(tableName);
            var gasRecords = table.DeleteMany(Query.EQ(nameof(GasRecord.VehicleId), vehicleId));
            db.Checkpoint();
            return true;
        }
    }
}

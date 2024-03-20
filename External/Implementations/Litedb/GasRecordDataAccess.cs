using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class GasRecordDataAccess : IGasRecordDataAccess
    {
        private LiteDatabase db { get; set; }
        private static string tableName = "gasrecords";
        public GasRecordDataAccess(ILiteDBInjection liteDB)
        {
            db = liteDB.GetLiteDB();
        }
        public List<GasRecord> GetGasRecordsByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<GasRecord>(tableName);
            var gasRecords = table.Find(Query.EQ(nameof(GasRecord.VehicleId), vehicleId));
            return gasRecords.ToList() ?? new List<GasRecord>();
        }
        public GasRecord GetGasRecordById(int gasRecordId)
        {
            var table = db.GetCollection<GasRecord>(tableName);
            return table.FindById(gasRecordId);
        }
        public bool DeleteGasRecordById(int gasRecordId)
        {
            var table = db.GetCollection<GasRecord>(tableName);
            table.Delete(gasRecordId);
            return true;
        }
        public bool SaveGasRecordToVehicle(GasRecord gasRecord)
        {
            var table = db.GetCollection<GasRecord>(tableName);
            table.Upsert(gasRecord);
            return true;
        }
        public bool DeleteAllGasRecordsByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<GasRecord>(tableName);
            var gasRecords = table.DeleteMany(Query.EQ(nameof(GasRecord.VehicleId), vehicleId));
            return true;
        }
    }
}

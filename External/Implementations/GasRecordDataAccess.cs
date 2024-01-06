using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class GasRecordDataAccess: IGasRecordDataAccess
    {
        private static string dbName = "data/cartracker.db";
        private static string tableName = "gasrecords";
        public List<GasRecord> GetGasRecordsByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<GasRecord>(tableName);
                var gasRecords = table.Find(Query.EQ(nameof(GasRecord.VehicleId), vehicleId));
                return gasRecords.ToList() ?? new List<GasRecord>();
            };
        }
        public GasRecord GetGasRecordById(int gasRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<GasRecord>(tableName);
                return table.FindById(gasRecordId);
            };
        }
        public bool DeleteGasRecordById(int gasRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<GasRecord>(tableName);
                table.Delete(gasRecordId);
                return true;
            };
        }
        public bool SaveGasRecordToVehicle(GasRecord gasRecord)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<GasRecord>(tableName);
                table.Upsert(gasRecord);
                return true;
            };
        }
        public bool DeleteAllGasRecordsByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<GasRecord>(tableName);
                var gasRecords = table.DeleteMany(Query.EQ(nameof(GasRecord.VehicleId), vehicleId));
                return true;
            };
        }
    }
}

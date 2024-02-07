using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class OdometerRecordDataAccess : IOdometerRecordDataAccess
    {
        private static string dbName = StaticHelper.DbName;
        private static string tableName = "odometerrecords";
        public List<OdometerRecord> GetOdometerRecordsByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<OdometerRecord>(tableName);
                var odometerRecords = table.Find(Query.EQ(nameof(OdometerRecord.VehicleId), vehicleId));
                return odometerRecords.ToList() ?? new List<OdometerRecord>();
            };
        }
        public OdometerRecord GetOdometerRecordById(int odometerRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<OdometerRecord>(tableName);
                return table.FindById(odometerRecordId);
            };
        }
        public bool DeleteOdometerRecordById(int odometerRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<OdometerRecord>(tableName);
                table.Delete(odometerRecordId);
                return true;
            };
        }
        public bool SaveOdometerRecordToVehicle(OdometerRecord odometerRecord)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<OdometerRecord>(tableName);
                table.Upsert(odometerRecord);
                return true;
            };
        }
        public bool DeleteAllOdometerRecordsByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<OdometerRecord>(tableName);
                var odometerRecords = table.DeleteMany(Query.EQ(nameof(OdometerRecord.VehicleId), vehicleId));
                return true;
            };
        }
    }
}

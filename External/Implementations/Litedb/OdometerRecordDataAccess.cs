using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class OdometerRecordDataAccess : IOdometerRecordDataAccess
    {
        private LiteDatabase db { get; set; }
        private static string tableName = "odometerrecords";
        public OdometerRecordDataAccess(ILiteDBInjection liteDB)
        {
            db = liteDB.GetLiteDB();
        }
        public List<OdometerRecord> GetOdometerRecordsByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<OdometerRecord>(tableName);
            var odometerRecords = table.Find(Query.EQ(nameof(OdometerRecord.VehicleId), vehicleId));
            return odometerRecords.ToList() ?? new List<OdometerRecord>();
        }
        public OdometerRecord GetOdometerRecordById(int odometerRecordId)
        {
            var table = db.GetCollection<OdometerRecord>(tableName);
            return table.FindById(odometerRecordId);
        }
        public bool DeleteOdometerRecordById(int odometerRecordId)
        {
            var table = db.GetCollection<OdometerRecord>(tableName);
            table.Delete(odometerRecordId);
            db.Checkpoint();
            return true;
        }
        public bool SaveOdometerRecordToVehicle(OdometerRecord odometerRecord)
        {
            var table = db.GetCollection<OdometerRecord>(tableName);
            table.Upsert(odometerRecord);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAllOdometerRecordsByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<OdometerRecord>(tableName);
            var odometerRecords = table.DeleteMany(Query.EQ(nameof(OdometerRecord.VehicleId), vehicleId));
            db.Checkpoint();
            return true;
        }
    }
}

using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using CarCareTracker.Helper;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class OdometerRecordDataAccess : IOdometerRecordDataAccess
    {
        private ILiteDBHelper _liteDB { get; set; }
        private static string tableName = "odometerrecords";
        public OdometerRecordDataAccess(ILiteDBHelper liteDB)
        {
           _liteDB = liteDB;
        }
        public List<OdometerRecord> GetOdometerRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<OdometerRecord>(tableName);
            var odometerRecords = table.Find(Query.EQ(nameof(OdometerRecord.VehicleId), vehicleId));
            return odometerRecords.ToList() ?? new List<OdometerRecord>();
        }
        public OdometerRecord GetOdometerRecordById(int odometerRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<OdometerRecord>(tableName);
            return table.FindById(odometerRecordId);
        }
        public bool DeleteOdometerRecordById(int odometerRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<OdometerRecord>(tableName);
            table.Delete(odometerRecordId);
            db.Checkpoint();
            return true;
        }
        public bool SaveOdometerRecordToVehicle(OdometerRecord odometerRecord)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<OdometerRecord>(tableName);
            table.Upsert(odometerRecord);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAllOdometerRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<OdometerRecord>(tableName);
            var odometerRecords = table.DeleteMany(Query.EQ(nameof(OdometerRecord.VehicleId), vehicleId));
            db.Checkpoint();
            return true;
        }
    }
}

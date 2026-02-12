using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using CarCareTracker.Helper;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class InspectionRecordDataAccess : IInspectionRecordDataAccess
    {
        private ILiteDBHelper _liteDB { get; set; }
        private static string tableName = "inspectionrecords";
        public InspectionRecordDataAccess(ILiteDBHelper liteDB)
        {
           _liteDB = liteDB;
        }
        public List<InspectionRecord> GetInspectionRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<InspectionRecord>(tableName);
            var inspectionRecords = table.Find(Query.EQ(nameof(InspectionRecord.VehicleId), vehicleId));
            return inspectionRecords.ToList() ?? new List<InspectionRecord>();
        }
        public InspectionRecord GetInspectionRecordById(int inspectionRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<InspectionRecord>(tableName);
            return table.FindById(inspectionRecordId);
        }
        public bool DeleteInspectionRecordById(int inspectionRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<InspectionRecord>(tableName);
            table.Delete(inspectionRecordId);
            db.Checkpoint();
            return true;
        }
        public bool SaveInspectionRecordToVehicle(InspectionRecord inspectionRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<InspectionRecord>(tableName);
            table.Upsert(inspectionRecordId);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAllInspectionRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<InspectionRecord>(tableName);
            var planRecords = table.DeleteMany(Query.EQ(nameof(InspectionRecord.VehicleId), vehicleId));
            db.Checkpoint();
            return true;
        }
    }
}

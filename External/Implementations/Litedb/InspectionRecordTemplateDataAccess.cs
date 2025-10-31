using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using CarCareTracker.Helper;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class InspectionRecordTemplateDataAccess : IInspectionRecordTemplateDataAccess
    {
        private ILiteDBHelper _liteDB { get; set; }
        private static string tableName = "inspectionrecordtemplates";
        public InspectionRecordTemplateDataAccess(ILiteDBHelper liteDB)
        {
           _liteDB = liteDB;
        }
        public List<InspectionRecordInput> GetInspectionRecordTemplatesByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<InspectionRecordInput>(tableName);
            var inspectionRecords = table.Find(Query.EQ(nameof(InspectionRecordInput.VehicleId), vehicleId));
            return inspectionRecords.ToList() ?? new List<InspectionRecordInput>();
        }
        public InspectionRecordInput GetInspectionRecordTemplateById(int inspectionRecordTemplateId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<InspectionRecordInput>(tableName);
            return table.FindById(inspectionRecordTemplateId);
        }
        public bool DeleteInspectionRecordTemplateById(int inspectionRecordTemplateId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<InspectionRecordInput>(tableName);
            table.Delete(inspectionRecordTemplateId);
            db.Checkpoint();
            return true;
        }
        public bool SaveInspectionReportTemplateToVehicle(InspectionRecordInput inspectionRecordTemplate)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<InspectionRecordInput>(tableName);
            table.Upsert(inspectionRecordTemplate);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAllInspectionReportTemplatesByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<InspectionRecordInput>(tableName);
            var planRecords = table.DeleteMany(Query.EQ(nameof(InspectionRecordInput.VehicleId), vehicleId));
            db.Checkpoint();
            return true;
        }
    }
}

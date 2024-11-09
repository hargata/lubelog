using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class PlanRecordTemplateDataAccess : IPlanRecordTemplateDataAccess
    {
        private ILiteDBHelper _liteDB { get; }
        private static string tableName = "planrecordtemplates";
        public PlanRecordTemplateDataAccess(ILiteDBHelper liteDB)
        {
           _liteDB = liteDB;
        }
        public List<PlanRecordInput> GetPlanRecordTemplatesByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<PlanRecordInput>(tableName);
            var planRecords = table.Find(Query.EQ(nameof(PlanRecordInput.VehicleId), vehicleId));
            return planRecords.ToList() ?? new List<PlanRecordInput>();
        }
        public PlanRecordInput GetPlanRecordTemplateById(int planRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<PlanRecordInput>(tableName);
            return table.FindById(planRecordId);
        }
        public bool DeletePlanRecordTemplateById(int planRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<PlanRecordInput>(tableName);
            table.Delete(planRecordId);
            db.Checkpoint();
            return true;
        }
        public bool SavePlanRecordTemplateToVehicle(PlanRecordInput planRecord)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<PlanRecordInput>(tableName);
            table.Upsert(planRecord);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAllPlanRecordTemplatesByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<PlanRecord>(tableName);
            table.DeleteMany(Query.EQ(nameof(PlanRecordInput.VehicleId), vehicleId));
            db.Checkpoint();
            return true;
        }
    }
}

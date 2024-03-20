using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class PlanRecordTemplateDataAccess : IPlanRecordTemplateDataAccess
    {
        private LiteDatabase db { get; set; }
        private static string tableName = "planrecordtemplates";
        public PlanRecordTemplateDataAccess(ILiteDBInjection liteDB)
        {
            db = liteDB.GetLiteDB();
        }
        public List<PlanRecordInput> GetPlanRecordTemplatesByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<PlanRecordInput>(tableName);
            var planRecords = table.Find(Query.EQ(nameof(PlanRecordInput.VehicleId), vehicleId));
            return planRecords.ToList() ?? new List<PlanRecordInput>();
        }
        public PlanRecordInput GetPlanRecordTemplateById(int planRecordId)
        {
            var table = db.GetCollection<PlanRecordInput>(tableName);
            return table.FindById(planRecordId);
        }
        public bool DeletePlanRecordTemplateById(int planRecordId)
        {
            var table = db.GetCollection<PlanRecordInput>(tableName);
            table.Delete(planRecordId);
            return true;
        }
        public bool SavePlanRecordTemplateToVehicle(PlanRecordInput planRecord)
        {
            var table = db.GetCollection<PlanRecordInput>(tableName);
            table.Upsert(planRecord);
            return true;
        }
        public bool DeleteAllPlanRecordTemplatesByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<PlanRecord>(tableName);
            var planRecords = table.DeleteMany(Query.EQ(nameof(PlanRecordInput.VehicleId), vehicleId));
            return true;
        }
    }
}

using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class PlanRecordTemplateDataAccess : IPlanRecordTemplateDataAccess
    {
        private static string dbName = StaticHelper.DbName;
        private static string tableName = "planrecordtemplates";
        public List<PlanRecordInput> GetPlanRecordTemplatesByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<PlanRecordInput>(tableName);
                var planRecords = table.Find(Query.EQ(nameof(PlanRecordInput.VehicleId), vehicleId));
                return planRecords.ToList() ?? new List<PlanRecordInput>();
            };
        }
        public PlanRecordInput GetPlanRecordTemplateById(int planRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<PlanRecordInput>(tableName);
                return table.FindById(planRecordId);
            };
        }
        public bool DeletePlanRecordTemplateById(int planRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<PlanRecordInput>(tableName);
                table.Delete(planRecordId);
                return true;
            };
        }
        public bool SavePlanRecordTemplateToVehicle(PlanRecordInput planRecord)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<PlanRecordInput>(tableName);
                table.Upsert(planRecord);
                return true;
            };
        }
        public bool DeleteAllPlanRecordTemplatesByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<PlanRecord>(tableName);
                var planRecords = table.DeleteMany(Query.EQ(nameof(PlanRecordInput.VehicleId), vehicleId));
                return true;
            };
        }
    }
}

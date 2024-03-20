using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class PlanRecordDataAccess : IPlanRecordDataAccess
    {
        private LiteDatabase db { get; set; }
        private static string tableName = "planrecords";
        public PlanRecordDataAccess(ILiteDBInjection liteDB)
        {
            db = liteDB.GetLiteDB();
        }
        public List<PlanRecord> GetPlanRecordsByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<PlanRecord>(tableName);
            var planRecords = table.Find(Query.EQ(nameof(PlanRecord.VehicleId), vehicleId));
            return planRecords.ToList() ?? new List<PlanRecord>();
        }
        public PlanRecord GetPlanRecordById(int planRecordId)
        {
            var table = db.GetCollection<PlanRecord>(tableName);
            return table.FindById(planRecordId);
        }
        public bool DeletePlanRecordById(int planRecordId)
        {
            var table = db.GetCollection<PlanRecord>(tableName);
            table.Delete(planRecordId);
            return true;
        }
        public bool SavePlanRecordToVehicle(PlanRecord planRecord)
        {
            var table = db.GetCollection<PlanRecord>(tableName);
            table.Upsert(planRecord);
            return true;
        }
        public bool DeleteAllPlanRecordsByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<PlanRecord>(tableName);
            var planRecords = table.DeleteMany(Query.EQ(nameof(PlanRecord.VehicleId), vehicleId));
            return true;
        }
    }
}

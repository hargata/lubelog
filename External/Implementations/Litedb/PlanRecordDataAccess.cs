using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class PlanRecordDataAccess : IPlanRecordDataAccess
    {
        private static string dbName = StaticHelper.DbName;
        private static string tableName = "planrecords";
        public List<PlanRecord> GetPlanRecordsByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<PlanRecord>(tableName);
                var planRecords = table.Find(Query.EQ(nameof(PlanRecord.VehicleId), vehicleId));
                return planRecords.ToList() ?? new List<PlanRecord>();
            };
        }
        public PlanRecord GetPlanRecordById(int planRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<PlanRecord>(tableName);
                return table.FindById(planRecordId);
            };
        }
        public bool DeletePlanRecordById(int planRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<PlanRecord>(tableName);
                table.Delete(planRecordId);
                return true;
            };
        }
        public bool SavePlanRecordToVehicle(PlanRecord planRecord)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<PlanRecord>(tableName);
                table.Upsert(planRecord);
                return true;
            };
        }
        public bool DeleteAllPlanRecordsByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<PlanRecord>(tableName);
                var planRecords = table.DeleteMany(Query.EQ(nameof(PlanRecord.VehicleId), vehicleId));
                return true;
            };
        }
    }
}

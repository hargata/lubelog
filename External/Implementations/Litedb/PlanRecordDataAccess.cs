using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class PlanRecordDataAccess : IPlanRecordDataAccess
    {
        private ILiteDBInjection _liteDB { get; set; }
        private static string tableName = "planrecords";
        public PlanRecordDataAccess(ILiteDBInjection liteDB)
        {
           _liteDB = liteDB;
        }
        public List<PlanRecord> GetPlanRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<PlanRecord>(tableName);
            var planRecords = table.Find(Query.EQ(nameof(PlanRecord.VehicleId), vehicleId));
            return planRecords.ToList() ?? new List<PlanRecord>();
        }
        public PlanRecord GetPlanRecordById(int planRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<PlanRecord>(tableName);
            return table.FindById(planRecordId);
        }
        public bool DeletePlanRecordById(int planRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<PlanRecord>(tableName);
            table.Delete(planRecordId);
            db.Checkpoint();
            return true;
        }
        public bool SavePlanRecordToVehicle(PlanRecord planRecord)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<PlanRecord>(tableName);
            table.Upsert(planRecord);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAllPlanRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<PlanRecord>(tableName);
            var planRecords = table.DeleteMany(Query.EQ(nameof(PlanRecord.VehicleId), vehicleId));
            db.Checkpoint();
            return true;
        }
    }
}

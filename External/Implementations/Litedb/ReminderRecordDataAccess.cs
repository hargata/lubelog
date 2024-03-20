using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class ReminderRecordDataAccess : IReminderRecordDataAccess
    {
        private LiteDatabase db { get; set; }
        private static string tableName = "reminderrecords";
        public ReminderRecordDataAccess(ILiteDBInjection liteDB)
        {
            db = liteDB.GetLiteDB();
        }
        public List<ReminderRecord> GetReminderRecordsByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<ReminderRecord>(tableName);
            var reminderRecords = table.Find(Query.EQ(nameof(ReminderRecord.VehicleId), vehicleId));
            return reminderRecords.ToList() ?? new List<ReminderRecord>();
        }
        public ReminderRecord GetReminderRecordById(int reminderRecordId)
        {
            var table = db.GetCollection<ReminderRecord>(tableName);
            return table.FindById(reminderRecordId);
        }
        public bool DeleteReminderRecordById(int reminderRecordId)
        {
            var table = db.GetCollection<ReminderRecord>(tableName);
            table.Delete(reminderRecordId);
            db.Checkpoint();
            return true;
        }
        public bool SaveReminderRecordToVehicle(ReminderRecord reminderRecord)
        {
            var table = db.GetCollection<ReminderRecord>(tableName);
            table.Upsert(reminderRecord);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAllReminderRecordsByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<ReminderRecord>(tableName);
            var reminderRecords = table.DeleteMany(Query.EQ(nameof(ReminderRecord.VehicleId), vehicleId));
            db.Checkpoint();
            return true;
        }
    }
}

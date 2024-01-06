using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class ReminderRecordDataAccess : IReminderRecordDataAccess
    {
        private static string dbName = StaticHelper.DbName;
        private static string tableName = "reminderrecords";
        public List<ReminderRecord> GetReminderRecordsByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<ReminderRecord>(tableName);
                var reminderRecords = table.Find(Query.EQ(nameof(ReminderRecord.VehicleId), vehicleId));
                return reminderRecords.ToList() ?? new List<ReminderRecord>();
            };
        }
        public ReminderRecord GetReminderRecordById(int reminderRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<ReminderRecord>(tableName);
                return table.FindById(reminderRecordId);
            };
        }
        public bool DeleteReminderRecordById(int reminderRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<ReminderRecord>(tableName);
                table.Delete(reminderRecordId);
                return true;
            };
        }
        public bool SaveReminderRecordToVehicle(ReminderRecord reminderRecord)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<ReminderRecord>(tableName);
                table.Upsert(reminderRecord);
                return true;
            };
        }
        public bool DeleteAllReminderRecordsByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<ReminderRecord>(tableName);
                var reminderRecords = table.DeleteMany(Query.EQ(nameof(ReminderRecord.VehicleId), vehicleId));
                return true;
            };
        }
    }
}

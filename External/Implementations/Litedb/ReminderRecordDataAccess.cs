using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using CarCareTracker.Helper;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class ReminderRecordDataAccess : IReminderRecordDataAccess
    {
        private ILiteDBHelper _liteDB { get; set; }
        private static string tableName = "reminderrecords";
        public ReminderRecordDataAccess(ILiteDBHelper liteDB)
        {
           _liteDB = liteDB;
        }
        public List<ReminderRecord> GetReminderRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<ReminderRecord>(tableName);
            var reminderRecords = table.Find(Query.EQ(nameof(ReminderRecord.VehicleId), vehicleId));
            return reminderRecords.ToList() ?? new List<ReminderRecord>();
        }
        public ReminderRecord GetReminderRecordById(int reminderRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<ReminderRecord>(tableName);
            return table.FindById(reminderRecordId);
        }
        public bool DeleteReminderRecordById(int reminderRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<ReminderRecord>(tableName);
            table.Delete(reminderRecordId);
            db.Checkpoint();
            return true;
        }
        public bool SaveReminderRecordToVehicle(ReminderRecord reminderRecord)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<ReminderRecord>(tableName);
            table.Upsert(reminderRecord);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAllReminderRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<ReminderRecord>(tableName);
            var reminderRecords = table.DeleteMany(Query.EQ(nameof(ReminderRecord.VehicleId), vehicleId));
            db.Checkpoint();
            return true;
        }
    }
}

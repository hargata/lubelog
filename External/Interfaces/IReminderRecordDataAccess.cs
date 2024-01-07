using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IReminderRecordDataAccess
    {
        public List<ReminderRecord> GetReminderRecordsByVehicleId(int vehicleId);
        public ReminderRecord GetReminderRecordById(int reminderRecordId);
        public bool DeleteReminderRecordById(int reminderRecordId);
        public bool SaveReminderRecordToVehicle(ReminderRecord reminderRecord);
        public bool DeleteAllReminderRecordsByVehicleId(int vehicleId);
    }
}

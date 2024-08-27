namespace CarCareTracker.Models
{
    public class VehicleInfo
    {
        public Vehicle VehicleData { get; set; } = new Vehicle();
        public int VeryUrgentReminderCount { get; set; }
        public int UrgentReminderCount { get; set;}
        public int NotUrgentReminderCount { get; set; }
        public int PastDueReminderCount { get; set; }
        public ReminderExportModel NextReminder { get; set; }
    }
}

namespace CarCareTracker.Models
{
    public class KioskReminderViewModel: ReminderRecordViewModel
    {
        public Vehicle VehicleData { get; set; }
        public int CurrentOdometer { get; set; }
    }
}

namespace CarCareTracker.Models
{
    public class ReminderMakeUpForVehicle
    {
        public int NotUrgentCount { get; set; }
        public int UrgentCount { get; set; }
        public int VeryUrgentCount { get; set; }
        public int PastDueCount { get; set; }
    }
}

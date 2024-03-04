namespace CarCareTracker.Models
{
    public class ReminderUrgencyConfig
    {
        public int UrgentDays { get; set; } = 30;
        public int VeryUrgentDays { get; set; } = 7;
        public int UrgentDistance { get; set; } = 100;
        public int VeryUrgentDistance { get; set; } = 50;
    }
}

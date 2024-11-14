namespace CarCareTracker.Models
{
    public class ReminderRecordViewModel
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime Date { get; set; }
        public decimal Mileage { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        /// <summary>
        /// Reason why this reminder is urgent
        /// </summary>
        public ReminderMetric Metric { get; set; } = ReminderMetric.Date;
        public ReminderUrgency Urgency { get; set; } = ReminderUrgency.NotUrgent;
        /// <summary>
        /// Recurring Reminders
        /// </summary>
        public bool IsRecurring { get; set; } = false;
        public List<string> Tags { get; set; } = new List<string>();
    }
}

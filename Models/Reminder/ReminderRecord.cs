namespace CarCareTracker.Models
{
    public class ReminderRecord
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime Date { get; set; }
        public int Mileage { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public ReminderMetric Metric { get; set; } = ReminderMetric.Date;
    }
}

namespace CarCareTracker.Models
{
    public class ReminderRecordInput
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Date { get; set; } = DateTime.Now.AddDays(1).ToShortDateString();
        public int Mileage { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public ReminderMetric Metric { get; set; } = ReminderMetric.Date;
        public ReminderRecord ToReminderRecord() { return new ReminderRecord { 
            Id = Id, 
            VehicleId = VehicleId, 
            Date = DateTime.Parse(Date), 
            Mileage = Mileage, 
            Description = Description, 
            Metric = Metric,
            Notes = Notes }; }
    }
}

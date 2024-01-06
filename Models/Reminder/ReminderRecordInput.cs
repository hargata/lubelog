namespace CarCareTracker.Models
{
    public class ReminderRecordInput
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Date { get; set; }
        public int Mileage { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public string Notes { get; set; }
        public ReminderRecord ToReminderRecord() { return new ReminderRecord { Id = Id, VehicleId = VehicleId, Date = DateTime.Parse(Date), Cost = Cost, Mileage = Mileage, Description = Description, Notes = Notes }; }
    }
}

namespace CarCareTracker.Models
{
    public class StickerViewModel
    {
        public Vehicle VehicleData { get; set; } = new Vehicle();
        public List<ReminderRecord> ReminderRecords { get; set; } = new List<ReminderRecord>();
        public List<GenericRecord> GenericRecords { get; set; } = new List<GenericRecord>();
    }
}

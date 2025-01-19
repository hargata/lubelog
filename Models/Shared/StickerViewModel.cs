namespace CarCareTracker.Models
{
    public class StickerViewModel
    {
        public ImportMode RecordType { get; set; }
        public Vehicle VehicleData { get; set; } = new Vehicle();
        public List<ReminderRecord> ReminderRecords { get; set; } = new List<ReminderRecord>();
        public List<GenericRecord> GenericRecords { get; set; } = new List<GenericRecord>();
        public List<SupplyRecord> SupplyRecords { get; set; } = new List<SupplyRecord>();
    }
}

namespace CarCareTracker.Models
{
    public class InspectionRecordTemplate
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Description { get; set; }
        public List<InspectionRecordTemplateField> Fields { get; set; } = new List<InspectionRecordTemplateField>();
        public List<int> ReminderRecordId { get; set; } = new List<int>();
    }
}

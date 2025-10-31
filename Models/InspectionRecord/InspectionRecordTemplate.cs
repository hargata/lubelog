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
    
    public class InspectionRecordTemplateField
    {
        public string Description { get; set; }
        public InspectionFieldType FieldType { get;set; }
        public List<InspectionRecordTemplateFieldOption> Options { get; set; } = new List<InspectionRecordTemplateFieldOption>();
        public ImportMode ActionItemType { get; set; }
        public string ActionItemDescription { get; set; }
        public PlanPriority ActionItemPriority { get; set; }
        public bool HasNotes { get; set; }
    }
    public class InspectionRecordTemplateFieldOption
    {
        public string Description { get; set; }
        public bool IsFail { get; set; }
    }
}

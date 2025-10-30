namespace CarCareTracker.Models
{
    public class InspectionRecordTemplate
    {
        public string Name { get; set; }
        public List<InspectionFields> Fields { get; set; }
    }
    public enum InspectionFieldType
    {
        Text = 0,
        Check = 1,
        Radio = 2
    }
    public class InspectionFields
    {
        public string Label { get; set; }
        public InspectionFieldType FieldType { get;set; }
        public List<InspectionFieldOptions> Options { get; set; }
        public ImportMode ActionItemType { get; set; }
        public string ActionItemName { get; set; }
        public PlanPriority ActionItemPriority { get; set; }
    }
    public class InspectionFieldOptions
    {
        public string Label { get; set; }
        public bool IsFail { get; set; }
    }
}

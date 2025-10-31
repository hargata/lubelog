namespace CarCareTracker.Models
{
    public class InspectionRecordTemplateField
    {
        public string Description { get; set; }
        public InspectionFieldType FieldType { get; set; }
        public List<InspectionRecordTemplateFieldOption> Options { get; set; } = new List<InspectionRecordTemplateFieldOption>();
        public ImportMode ActionItemType { get; set; }
        public string ActionItemDescription { get; set; }
        public PlanPriority ActionItemPriority { get; set; }
        public bool HasNotes { get; set; }
        public string Notes { get; set; }
        public InspectionRecordResult ToInspectionRecordResult()
        {
            return new InspectionRecordResult
            {
                Description = Description,
                Values = Options.Where(x => x.IsSelected).Select(y => y.Description).ToList(),
                Failed = Options.Any(x => x.IsSelected && x.IsFail),
                Notes = HasNotes ? Notes : string.Empty
            };
        }
    }
    public class InspectionRecordTemplateFieldOption
    {
        public string Description { get; set; }
        public bool IsFail { get; set; }
        public bool IsSelected { get; set; }
    }
    public class InspectionRecordResult
    {
        public string Description { get; set; }
        public List<string> Values { get; set; }
        public bool Failed { get; set; }
        public string Notes { get; set; }
    }
}

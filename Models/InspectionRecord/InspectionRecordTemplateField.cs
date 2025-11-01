﻿namespace CarCareTracker.Models
{
    public class InspectionRecordTemplateField
    {
        public string Description { get; set; }
        public InspectionFieldType FieldType { get; set; } = InspectionFieldType.Text;
        public List<InspectionRecordTemplateFieldOption> Options { get; set; } = new List<InspectionRecordTemplateFieldOption>();
        public ImportMode ActionItemType { get; set; } = ImportMode.ServiceRecord;
        public string ActionItemDescription { get; set; }
        public PlanPriority ActionItemPriority { get; set; } = PlanPriority.Normal;
        public bool HasNotes { get; set; }
        public bool HasActionItem { get; set; }
        public string Notes { get; set; }
        public InspectionRecordResult ToInspectionRecordResult()
        {
            return new InspectionRecordResult
            {
                Description = Description,
                Values = Options.Where(x => x.IsSelected).Select(y => y.Description).ToList(),
                Failed = (FieldType == InspectionFieldType.Radio && Options.Any(x => x.IsSelected && x.IsFail)) || (FieldType == InspectionFieldType.Check && Options.Any(x=> !x.IsSelected && x.IsFail)),
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

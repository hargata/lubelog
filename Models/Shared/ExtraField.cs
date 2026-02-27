namespace CarCareTracker.Models
{
    public class ExtraField
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public ExtraFieldType FieldType { get; set; } = ExtraFieldType.Text;
    }
}

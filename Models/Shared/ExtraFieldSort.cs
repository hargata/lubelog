namespace CarCareTracker.Models
{
    public class ExtraFieldSort
    {
        public string Name { get; set; }
        public ExtraFieldType FieldType { get; set; } = ExtraFieldType.Text;
        public override bool Equals(object obj)
        {
            if (obj is not ExtraFieldSort other) return false;
            return string.Equals(Name ?? "", other.Name ?? "", StringComparison.OrdinalIgnoreCase)
                   && FieldType == other.FieldType;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Name?.ToLowerInvariant() ?? "", FieldType);
        }
    }
}

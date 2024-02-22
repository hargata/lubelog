namespace CarCareTracker.Models
{
    public class GenericRecordEditModel
    {
        public ImportMode DataType { get; set; }
        public List<int> RecordIds { get; set; } = new List<int>();
    }
}

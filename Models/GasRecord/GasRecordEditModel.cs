namespace CarCareTracker.Models
{
    public class GasRecordEditModel
    {
        public List<int> RecordIds { get; set; } = new List<int>();
        public GasRecord EditRecord { get; set; } = new GasRecord();
    }
}

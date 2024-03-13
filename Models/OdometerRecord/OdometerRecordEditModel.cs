namespace CarCareTracker.Models
{
    public class OdometerRecordEditModel
    {
        public List<int> RecordIds { get; set; } = new List<int>();
        public OdometerRecord EditRecord { get; set; } = new OdometerRecord();
    }
}

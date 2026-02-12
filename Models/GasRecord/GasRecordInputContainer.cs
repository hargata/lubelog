namespace CarCareTracker.Models
{
    public class GasRecordInputContainer
    {
        public bool UseKwh { get; set; }
        public bool UseHours { get; set; }
        public GasRecordInput GasRecord { get; set; }
    }
}

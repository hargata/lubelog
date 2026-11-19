namespace CarCareTracker.Models
{
    public class GasRecordInputContainer
    {
        public bool UseKwh { get; set; }
        public bool UseHours { get; set; }
        public List<AdditionalFuelType> AdditionalFuelTypes { get; set; } = new List<AdditionalFuelType>();
        public GasRecordInput GasRecord { get; set; } = new GasRecordInput();
    }
}

namespace CarCareTracker.Models
{
    public class GasRecordViewModelContainer
    {
        public bool UseKwh { get; set; }
        public bool UseHours { get; set; }
        public List<AdditionalFuelType> AdditionalFuelTypes { get; set; } = new List<AdditionalFuelType>();
        public List<GasRecordViewModel> GasRecords { get; set; } = new List<GasRecordViewModel>();
    }
}

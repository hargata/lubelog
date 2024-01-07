namespace CarCareTracker.Models
{
    public class GasRecordViewModelContainer
    {
       public bool UseKwh { get; set; }
       public List<GasRecordViewModel> GasRecords { get; set; } = new List<GasRecordViewModel>();
    }
}

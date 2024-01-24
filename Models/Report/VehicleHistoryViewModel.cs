namespace CarCareTracker.Models
{
    public class VehicleHistoryViewModel
    {
        public Vehicle VehicleData { get; set; }
        public List<GenericReportModel> VehicleHistory { get; set; }
        public string Odometer { get; set; }
        public string MPG { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalGasCost { get; set; }
    }
}

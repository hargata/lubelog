namespace CarCareTracker.Models
{
    public class VehicleHistoryViewModel
    {
        public Vehicle VehicleData { get; set; }
        public List<GenericReportModel> VehicleHistory { get; set; }
        public ReportParameter ReportParameters { get; set; }
        public string Odometer { get; set; } = string.Empty;
        public string MPG { get; set; } = string.Empty;
        public decimal TotalCost { get; set; }
        public decimal TotalGasCost { get; set; }
        public string DaysOwned { get; set; } = string.Empty;
        public string DistanceTraveled { get; set; } = string.Empty;
        public decimal TotalCostPerMile { get; set; }
        public decimal TotalGasCostPerMile { get; set; }
        public string DistanceUnit { get; set; } = string.Empty;
        public decimal TotalDepreciation { get; set; }
        public decimal DepreciationPerDay { get; set; }
        public decimal DepreciationPerMile { get; set; }
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
    }
}

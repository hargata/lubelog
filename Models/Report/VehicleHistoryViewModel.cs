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
        public string DaysOwned { get; set; }
        public string DistanceTraveled { get; set; }
        public decimal TotalCostPerMile { get; set; }
        public decimal TotalGasCostPerMile { get; set; }
        public string DistanceUnit { get; set; }
        public decimal TotalDepreciation { get; set; }
        public decimal DepreciationPerDay { get; set; }
        public decimal DepreciationPerMile { get; set; }
    }
}

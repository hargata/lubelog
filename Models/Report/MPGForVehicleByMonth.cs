namespace CarCareTracker.Models
{
    public class MPGForVehicleByMonth
    {
        public List<CostForVehicleByMonth> CostData { get; set; } = new List<CostForVehicleByMonth>();
        public List<CostForVehicleByMonth> SortedCostData { get; set; } = new List<CostForVehicleByMonth>();
        public string Unit { get; set; }
    }
}

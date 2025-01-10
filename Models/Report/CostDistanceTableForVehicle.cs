namespace CarCareTracker.Models
{
    public class CostDistanceTableForVehicle
    {
        public string DistanceUnit { get; set; } = "mi.";
        public List<CostForVehicleByMonth> CostData { get; set; } = new List<CostForVehicleByMonth>();
    }
}

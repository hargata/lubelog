namespace CarCareTracker.Models
{
    public class CostForVehicleByMonth
    {
        public int Year { get; set; }
        public int MonthId { get; set; }
        public string MonthName { get; set; }
        public decimal Cost { get; set; }
        public int DistanceTraveled { get; set; }
        public decimal CostPerDistanceTraveled { get { if (DistanceTraveled > 0) { return Cost / DistanceTraveled; } else { return 0M; } } }
    }
}

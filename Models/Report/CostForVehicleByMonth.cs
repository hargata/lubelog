namespace CarCareTracker.Models
{
    public class CostForVehicleByMonth
    {
        public int MonthId { get; set; }
        public string MonthName { get; set; }
        public decimal Cost { get; set; }
        public int MaxMileage { get; set; }
        public int MinMileage { get; set; }
        public int DistanceTraveled { get; set; }
    }
}

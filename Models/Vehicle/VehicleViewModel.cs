namespace CarCareTracker.Models
{
    public class VehicleViewModel
    {
        public int Id { get; set; }
        public string ImageLocation { get; set; } = "/defaults/noimage.png";
        public int Year { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string SoldDate { get; set; } = string.Empty;
        public bool IsElectric { get; set; } = false;
        public bool IsDiesel { get; set; } = false;
        public bool UseHours { get; set; } = false;
        public bool OdometerOptional { get; set; } = false;
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<string> Tags { get; set; } = new List<string>();
        public string VehicleIdentifier { get; set; } = "LicensePlate";
        //Dashboard Metric Attributes
        public List<DashboardMetric> DashboardMetrics { get; set; } = new List<DashboardMetric>();
        public int LastReportedMileage { get; set; }
        public bool HasReminders { get; set; } = false;
        public decimal CostPerMile { get; set; }
        public decimal TotalCost { get; set; }
        public string DistanceUnit { get; set; } = string.Empty;
    }
}

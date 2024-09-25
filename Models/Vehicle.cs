namespace CarCareTracker.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string ImageLocation { get; set; } = "/defaults/noimage.png";
        public int Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string LicensePlate { get; set; }
        public string PurchaseDate { get; set; }
        public string SoldDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SoldPrice { get; set; }
        public bool IsElectric { get; set; } = false;
        public bool IsDiesel { get; set; } = false;
        public bool UseHours { get; set; } = false;
        public bool OdometerOptional { get; set; } = false;
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<string> Tags { get; set; } = new List<string>();
        public bool HasOdometerAdjustment { get; set; } = false;
        /// <summary>
        /// Primarily used for vehicles with odometer units different from user's settings.
        /// </summary>
        public string OdometerMultiplier { get; set; } = "1";
        /// <summary>
        /// Primarily used for vehicles where the odometer does not reflect actual mileage.
        /// </summary>
        public string OdometerDifference { get; set; } = "0";
        public List<DashboardMetric> DashboardMetrics { get; set; } = new List<DashboardMetric>();
    }
}

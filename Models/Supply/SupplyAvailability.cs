namespace CarCareTracker.Models
{
    public class SupplyAvailability
    {
        public bool Missing { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Required { get; set; }
        public decimal InStock { get; set; }
        public bool Insufficient { get { return Required > InStock; } }
    }
}

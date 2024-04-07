namespace CarCareTracker.Models
{
    public class SupplyUsageViewModel
    {
        public List<SupplyRecord> Supplies { get; set; } = new List<SupplyRecord>();
        public List<SupplyUsage> Usage { get; set; } = new List<SupplyUsage>();
    }
}

namespace CarCareTracker.Models
{
    public class SupplyUsageHistory {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }
    }
}

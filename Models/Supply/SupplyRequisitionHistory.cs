namespace CarCareTracker.Models
{
    public class SupplyRequisitionHistory
    {
        public string CostInputId { get; set; } = string.Empty;
        public List<SupplyUsageHistory> RequisitionHistory { get; set; } = new List<SupplyUsageHistory>();
    }
}

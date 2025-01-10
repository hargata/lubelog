namespace CarCareTracker.Models
{
    public class SupplyRequisitionHistory
    {
        public string CostInputId { get; set; }
        public List<SupplyUsageHistory> RequisitionHistory { get; set; } = new List<SupplyUsageHistory>();
    }
}

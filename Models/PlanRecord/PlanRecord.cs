namespace CarCareTracker.Models
{
    public class PlanRecord
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public ImportMode ImportMode { get; set; }
        public PlanPriority Priority { get; set; }
        public PlanProgress Progress { get; set; }
        public decimal Cost { get; set; }
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<SupplyUsageHistory> RequisitionHistory { get; set; } = new List<SupplyUsageHistory>();
    }
}

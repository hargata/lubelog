namespace CarCareTracker.Models
{
    public class UpgradeRecordInput
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Date { get; set; } = DateTime.Now.ToShortDateString();
        public int Mileage { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public string Notes { get; set; }
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<SupplyUsage> Supplies { get; set; } = new List<SupplyUsage>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<SupplyUsageHistory> RequisitionHistory { get; set; } = new List<SupplyUsageHistory>();
        public UpgradeRecord ToUpgradeRecord() { return new UpgradeRecord { 
            Id = Id, 
            VehicleId = VehicleId, 
            Date = DateTime.Parse(Date), 
            Cost = Cost, 
            Mileage = Mileage, 
            Description = Description, 
            Notes = Notes, 
            Files = Files, 
            Tags = Tags, 
            ExtraFields = ExtraFields,
            RequisitionHistory = RequisitionHistory
        }; 
        }
    }
}

namespace CarCareTracker.Models
{
    public class SupplyRecordInput
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Date { get; set; } = DateTime.Now.ToShortDateString();
        public string PartNumber { get; set; } = string.Empty;
        public string PartSupplier { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<SupplyUsageHistory> RequisitionHistory { get; set; } = new List<SupplyUsageHistory>();
        public SupplyRecord ToSupplyRecord() { return new SupplyRecord { 
            Id = Id, 
            VehicleId = VehicleId, 
            Date = DateTime.Parse(Date), 
            Cost = Cost, 
            PartNumber = PartNumber,
            PartSupplier = PartSupplier,
            Quantity = Quantity,
            Description = Description, 
            Notes = Notes, 
            Files = Files,
            Tags = Tags,
            ExtraFields = ExtraFields,
            RequisitionHistory = RequisitionHistory
        }; }
    }
}

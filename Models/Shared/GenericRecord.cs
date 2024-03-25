namespace CarCareTracker.Models
{
    public class GenericRecord
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime Date { get; set; }
        public decimal Mileage { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public string Notes { get; set; }
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<string> Tags { get; set;} = new List<string>();
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<SupplyUsageHistory> RequisitionHistory { get; set; } = new List<SupplyUsageHistory>();
    }
}

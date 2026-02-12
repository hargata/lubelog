namespace CarCareTracker.Models
{
    /// <summary>
    /// Generic Model used for vehicle history report.
    /// </summary>
    public class GenericReportModel
    {
        public ImportMode DataType { get; set; }
        public DateTime Date { get; set; }
        public int Odometer { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<SupplyUsageHistory> RequisitionHistory { get; set; } = new List<SupplyUsageHistory>();
    }
}

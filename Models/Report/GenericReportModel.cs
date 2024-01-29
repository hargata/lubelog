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
        public string Description { get; set; }
        public string Notes { get; set; }
        public decimal Cost { get; set; }
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
    }
}

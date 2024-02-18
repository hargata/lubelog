namespace CarCareTracker.Models
{
    public class SupplyRecord
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        /// <summary>
        /// When the part or supplies were purchased.
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Part number can be alphanumeric.
        /// </summary>
        public string PartNumber { get; set; }
        /// <summary>
        /// Where the part/supplies were purchased from.
        /// </summary>
        public string PartSupplier { get; set; }
        /// <summary>
        /// Amount purchased, can be partial quantities such as fluids.
        /// </summary>
        public decimal Quantity { get; set; }
        /// <summary>
        /// Description of the part/supplies purchased.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// How much it costs
        /// </summary>
        public decimal Cost { get; set; }
        /// <summary>
        /// Additional notes.
        /// </summary>
        public string Notes { get; set; }
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<SupplyUsageHistory> UsageHistory { get; set; } = new List<SupplyUsageHistory>();
    }
}

namespace CarCareTracker.Models
{
    public class GasRecordInput
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Date { get; set; } = DateTime.Now.ToShortDateString();
        /// <summary>
        /// American moment
        /// </summary>
        public int Mileage { get; set; }
        /// <summary>
        /// Wtf is a kilometer?
        /// </summary>
        public decimal Gallons { get; set; }
        public decimal Cost { get; set; }
        public bool IsFillToFull { get; set; } = true;
        public bool MissedFuelUp { get; set; } = false;
        public string Notes { get; set; } = string.Empty;
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<SupplyUsage> Supplies { get; set; } = new List<SupplyUsage>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<SupplyUsageHistory> RequisitionHistory { get; set; } = new List<SupplyUsageHistory>();
        public List<SupplyUsageHistory> DeletedRequisitionHistory { get; set; } = new List<SupplyUsageHistory>();
        public bool CopySuppliesAttachment { get; set; } = false;
        public GasRecord ToGasRecord() { return new GasRecord { 
            Id = Id, 
            Cost = Cost, 
            Date = DateTime.Parse(Date), 
            Gallons = Gallons, 
            Mileage = Mileage, 
            VehicleId = VehicleId, 
            Files = Files,
            IsFillToFull = IsFillToFull,
            MissedFuelUp = MissedFuelUp,
            Notes = Notes,
            Tags = Tags,
            ExtraFields = ExtraFields,
            RequisitionHistory = RequisitionHistory
        }; }
    }
}

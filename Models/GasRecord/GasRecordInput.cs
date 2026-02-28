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
        /// <summary>
        /// State of Charge at fill-up (0-100). 0 = not tracked. See GasRecord.SoC for details.
        /// </summary>
        private int _soC = 100;
        private bool _soCExplicitlySet = false;
        public int SoC
        {
            get => _soC;
            set { _soC = value; _soCExplicitlySet = true; }
        }
        /// <summary>
        /// Legacy field for backward compatibility with form submissions that still send isFillToFull.
        /// If SoC was explicitly set (e.g. EV form sending soC=80), IsFillToFull is ignored.
        /// </summary>
        public bool IsFillToFull
        {
            get => SoC == 100;
            set
            {
                // Only apply if SoC was NOT explicitly set — avoids overwriting soC=80 with isFillToFull=false.
                if (!_soCExplicitlySet)
                {
                    _soC = value ? 100 : 0;
                }
            }
        }
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
            SoC = SoC,
            MissedFuelUp = MissedFuelUp,
            Notes = Notes,
            Tags = Tags,
            ExtraFields = ExtraFields,
            RequisitionHistory = RequisitionHistory
        }; }
    }
}

namespace CarCareTracker.Models
{
    public class GasRecordViewModel
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int MonthId { get; set; }
        public string Date { get; set; } = string.Empty;
        /// <summary>
        /// American moment
        /// </summary>
        public int Mileage { get; set; }
        /// <summary>
        /// Wtf is a kilometer?
        /// </summary>
        public decimal Gallons { get; set; }
        public decimal Cost { get; set; }
        public int DeltaMileage { get; set; }
        public decimal MilesPerGallon { get; set; }
        public decimal CostPerGallon { get; set; }
        /// <summary>
        /// State of Charge at fill-up (0-100). 0 = not tracked. 100 = full. Other = partial (e.g. 80 for EV).
        /// </summary>
        public int SoC { get; set; } = 100;
        public bool IsFillToFull => SoC == 100;
        public bool MissedFuelUp { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        /// <summary>
        /// True if this record should be included in average efficiency calculation.
        /// SoC=0 (not tracked) records are excluded from the average (but their consumption
        /// is accumulated into the next SoC-tracked calculation in GasHelper).
        /// </summary>
        public bool IncludeInAverage { get { return MilesPerGallon > 0 || (SoC == 0 && !MissedFuelUp) || (Mileage == default && !MissedFuelUp); } }
    }
}

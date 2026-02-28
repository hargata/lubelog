namespace CarCareTracker.Models
{
    public class GasRecord
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime Date { get; set; }
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
        /// State of Charge at fill-up (1-100). 0 = not tracked (skip efficiency calculation).
        /// Legacy "IsFillToFull=true" records (which lack a SoC field) are migrated on read:
        ///   IsFillToFull=true  -> SoC=100 (full charge)
        ///   IsFillToFull=false -> SoC=0   (not tracked)
        /// EV drivers charging to e.g. 80% consistently should set SoC=80 on every charge.
        /// Efficiency is then calculated between consecutive entries with the same SoC value.
        /// </summary>
        private int _soC = -1; // -1 = not yet migrated; will be computed from IsFillToFull on first access.
        public int SoC
        {
            get
            {
                if (_soC == -1)
                {
                    // Migrate from legacy IsFillToFull field
                    _soC = _isFillToFull ? 100 : 0;
                }
                return _soC;
            }
            set => _soC = value;
        }

        /// <summary>
        /// Legacy field kept for JSON backward compatibility. New code should use SoC.
        /// When SoC is explicitly set (>= 0), IsFillToFull reflects SoC == 100.
        /// When reading old JSON that has IsFillToFull but no SoC, this seeds the SoC migration.
        /// </summary>
        private bool _isFillToFull = true;
        public bool IsFillToFull
        {
            get => SoC == 100;
            set
            {
                _isFillToFull = value;
                // Only apply migration if SoC hasn't been explicitly set yet.
                if (_soC == -1)
                {
                    _soC = value ? 100 : 0;
                }
            }
        }

        public bool MissedFuelUp { get; set; } = false;
        public string Notes { get; set; } = string.Empty;
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<SupplyUsageHistory> RequisitionHistory { get; set; } = new List<SupplyUsageHistory>();
    }
}

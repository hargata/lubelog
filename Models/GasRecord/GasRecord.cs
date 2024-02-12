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
        public bool IsFillToFull { get; set; } = true;
        public bool MissedFuelUp { get; set; } = false;
        public string Notes { get; set; }
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<string> Tags { get; set; } = new List<string>();
        public Dictionary<string, string> ExtraFields { get; set; } = new Dictionary<string, string>();
    }
}

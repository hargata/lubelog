namespace CarCareTracker.Models
{
    public class GasRecordInput
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Date { get; set; }
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
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public GasRecord ToGasRecord() { return new GasRecord { 
            Id = Id, 
            Cost = Cost, 
            Date = DateTime.Parse(Date), 
            Gallons = Gallons, 
            Mileage = Mileage, 
            VehicleId = VehicleId, 
            Files = Files,
            IsFillToFull = IsFillToFull,
            MissedFuelUp = MissedFuelUp
        }; }
    }
}

namespace CarCareTracker.Models
{
    public class OdometerRecord
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime Date { get; set; }
        public int InitialMileage { get; set; }
        public int Mileage { get; set; }
        public int DistanceTraveled { get { return Mileage - InitialMileage; } }
        public string Notes { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<int> EquipmentRecordId { get; set; } = new List<int>();
    }
}

namespace CarCareTracker.Models
{
    public class OdometerRecord
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime Date { get; set; }
        public int Mileage { get; set; }
        public string Notes { get; set; }
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
    }
}

namespace CarCareTracker.Models
{
    public class InspectionRecord
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime Date { get; set; }
        public int Mileage { get; set; }
        public decimal Cost { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<InspectionRecordResult> Results { get; set; } = new List<InspectionRecordResult>();
        public bool Failed { get { return Results.Any(x => x.Failed); } }
    }
}

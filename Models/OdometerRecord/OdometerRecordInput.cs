namespace CarCareTracker.Models
{
    public class OdometerRecordInput
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Date { get; set; } = DateTime.Now.ToShortDateString();
        public int InitialMileage { get; set; }
        public int Mileage { get; set; }
        public string Notes { get; set; }
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public OdometerRecord ToOdometerRecord() { return new OdometerRecord { Id = Id, VehicleId = VehicleId, Date = DateTime.Parse(Date), Mileage = Mileage, Notes = Notes, Files = Files, Tags = Tags, ExtraFields = ExtraFields, InitialMileage = InitialMileage }; }
    }
}

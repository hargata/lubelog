namespace CarCareTracker.Models
{
    public class InspectionRecordInput
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Date { get; set; } = DateTime.Now.ToShortDateString();
        public int Mileage { get; set; }
        public decimal Cost { get; set; }
        public string Description { get; set; }
        public List<int> ReminderRecordId { get; set; } = new List<int>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<InspectionRecordTemplateField> Fields { get; set; } = new List<InspectionRecordTemplateField>();
        public InspectionRecord ToInspectionRecord()
        {
            return new InspectionRecord
            {
                Id = Id,
                VehicleId = VehicleId,
                Date = DateTime.Parse(Date),
                Cost = Cost,
                Mileage = Mileage,
                Description = Description,
                Results = Fields.Select(x => x.ToInspectionRecordResult()).ToList(),
                Files = Files,
                Tags = Tags
            };
        }
    }
}

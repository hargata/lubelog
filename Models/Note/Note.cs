namespace CarCareTracker.Models
{
    public class Note
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string NoteText { get; set; } = string.Empty;
        public bool Pinned { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
    }
}

namespace CarCareTracker.Models
{
    public class Note
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Description { get; set; }
        public string NoteText { get; set; }
        public bool Pinned { get; set; }
    }
}

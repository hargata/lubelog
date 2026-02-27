namespace CarCareTracker.Models
{
    public class UserCollaboratorViewModel
    {
        public List<int> VehicleIds { get; set; } = new List<int>();
        public List<string> CommonCollaborators { get; set; } = new List<string>();
        public List<string> PartialCollaborators { get; set; } = new List<string>();
    }
}
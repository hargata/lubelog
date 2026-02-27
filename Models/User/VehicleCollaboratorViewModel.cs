namespace CarCareTracker.Models
{
    public class VehicleCollaboratorViewModel
    {
        public List<UserCollaborator> Collaborators { get; set; } = new List<UserCollaborator>();
        public bool CanModifyCollaborators { get; set; } = true;
    }
}
namespace CarCareTracker.Models
{
    public class VehicleCollaboratorViewModel
    {
        public List<UserCollaborator> Collaborators { get; set; }
        public bool CanModifyCollaborators { get; set; } = true;
    }
}
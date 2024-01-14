namespace CarCareTracker.Models
{
    public class UserCollaborator
    {
        public string UserName { get; set; }
        public UserAccessType AccessType { get; set; }
        public UserVehicle UserVehicle { get; set; }
    }
}

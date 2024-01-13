namespace CarCareTracker.Models
{
    public class UserAccess
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int VehicleId { get; set; }
        public UserAccessType AccessType { get; set; }
    }
}

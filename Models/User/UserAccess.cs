namespace CarCareTracker.Models
{
    public class UserVehicle
    {
        public int UserId { get; set; }
        public int VehicleId { get; set; }
    }
    public class UserAccess
    {
        public UserVehicle Id { get; set; }
        public UserAccessType AccessType { get; set; }
    }
}

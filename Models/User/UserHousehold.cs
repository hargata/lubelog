namespace CarCareTracker.Models
{
    public class HouseholdAccess
    {
        public int ParentUserId { get; set; }
        public int ChildUserId { get; set; }
    }
    public class UserHousehold
    {
        public HouseholdAccess Id { get; set; }
    }
}

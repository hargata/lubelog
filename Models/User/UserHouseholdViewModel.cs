namespace CarCareTracker.Models
{
    public class UserHouseholdViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public UserHousehold UserHousehold { get; set; } = new UserHousehold();
    }
}
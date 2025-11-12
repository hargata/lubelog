namespace CarCareTracker.Models
{
    public class UserHouseholdAdminViewModel
    {
        public List<UserHouseholdViewModel> Households { get; set; }
        public int ParentUserId { get; set; }
    }
}
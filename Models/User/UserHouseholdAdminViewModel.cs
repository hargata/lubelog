namespace CarCareTracker.Models
{
    public class UserHouseholdAdminViewModel
    {
        public List<UserHouseholdViewModel> Households { get; set; } = new List<UserHouseholdViewModel>();
        public int ParentUserId { get; set; }
    }
}
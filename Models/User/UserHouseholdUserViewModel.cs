namespace CarCareTracker.Models
{
    public class UserHouseholdUserViewModel
    {
        /// <summary>
        /// users that belongs in this user's household
        /// </summary>
        public List<UserHouseholdViewModel> Households { get; set; } = new List<UserHouseholdViewModel>();
        /// <summary>
        /// households that this user belongs to
        /// </summary>
        public List<UserHouseholdViewModel> UserHouseholds { get; set; } = new List<UserHouseholdViewModel>();
    }
}
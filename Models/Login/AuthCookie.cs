namespace CarCareTracker.Models
{
    public class AuthCookie
    {
        public UserData UserData { get; set; }
        public DateTime ExpiresOn { get; set; }
    }
}

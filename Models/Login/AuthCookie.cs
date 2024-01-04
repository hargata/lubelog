namespace CarCareTracker.Models
{
    public class AuthCookie
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public DateTime ExpiresOn { get; set; }
    }
}

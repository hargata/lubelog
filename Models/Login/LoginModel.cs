namespace CarCareTracker.Models
{
    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string EmailAddress { get; set; }
        public string Token { get; set; }
        public bool IsPersistent { get; set; } = false;
    }
}

namespace CarCareTracker.Models
{
    public class LoginModel
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public bool IsPersistent { get; set; } = false;
    }
}

namespace CarCareTracker.Models
{
    public class OpenIDRegistrationModel
    {
        public bool RegistrationDisabled { get; set; } = false;
        public string EmailAddress { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string LogOutUrl { get; set; } = string.Empty;
    }
}

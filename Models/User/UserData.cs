namespace CarCareTracker.Models
{
    public class UserData
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsRootUser { get; set; } = false;
    }
}
namespace CarCareTracker.Models
{
    public class ApiUser
    {
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsRoot { get; set; }
    }
}

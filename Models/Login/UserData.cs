namespace CarCareTracker.Models
{
    public class UserData
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
    }
}
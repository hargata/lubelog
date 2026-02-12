namespace CarCareTracker.Models
{
    public class Token
    {
        public int Id { get; set; }
        public string Body { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
    }
}

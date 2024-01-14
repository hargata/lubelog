namespace CarCareTracker.Models
{
    public class MailConfig
    {
        public string EmailServer { get; set; }
        public string EmailFrom { get; set; }
        public bool UseSSL { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

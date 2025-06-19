namespace CarCareTracker.Models
{
    public class ServerSettingsViewModel
    {
        public string LocaleInfo { get; set; }
        public string PostgresConnection { get; set; }
        public string AllowedFileExtensions { get; set; }
        public string CustomLogoURL { get; set; }
        public string CustomSmallLogoURL { get; set; }
        public string MessageOfTheDay { get; set; }
        public string WebHookURL { get; set; }
        public bool CustomWidgetsEnabled { get; set; }
        public bool InvariantAPIEnabled { get; set; }
        public MailConfig SMTPConfig { get; set; } = new MailConfig();
        public OpenIDConfig OIDCConfig { get; set; } = new OpenIDConfig();

    }
}

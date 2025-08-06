namespace CarCareTracker.Models
{
    public class ServerSettingsViewModel
    {
        public string LocaleOverride { get; set; }
        public string PostgresConnection { get; set; }
        public string AllowedFileExtensions { get; set; }
        public string CustomLogoURL { get; set; }
        public string CustomSmallLogoURL { get; set; }
        public string MessageOfTheDay { get; set; }
        public string WebHookURL { get; set; }
        public string Domain { get; set; }
        public bool CustomWidgetsEnabled { get; set; }
        public bool InvariantAPIEnabled { get; set; }
        public MailConfig SMTPConfig { get; set; } = new MailConfig();
        public OpenIDConfig OIDCConfig { get; set; } = new OpenIDConfig();
        public bool OpenRegistration { get; set; }
        public bool DisableRegistration { get; set; }
        public ReminderUrgencyConfig ReminderUrgencyConfig { get; set; } = new ReminderUrgencyConfig();
        public string DefaultReminderEmail { get; set; } = string.Empty;
        public bool EnableRootUserOIDC { get; set; }
        public bool EnableAuth { get; set; }
        public List<string> AvailableLocales { get; set; }
    }
}

namespace CarCareTracker.Models
{
    public class ServerSettingsViewModel
    {
        public string LocaleOverride { get; set; } = string.Empty;
        public string LocaleDateTimeOverride { get; set; } = string.Empty;
        public string PostgresConnection { get; set; } = string.Empty;
        public string AllowedFileExtensions { get; set; } = string.Empty;
        public string CustomLogoURL { get; set; } = string.Empty;
        public string CustomSmallLogoURL { get; set; } = string.Empty;
        public string MessageOfTheDay { get; set; } = string.Empty;
        public string WebHookURL { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public bool CustomWidgetsEnabled { get; set; }
        public bool InvariantAPIEnabled { get; set; }
        public bool WebSocketEnabled { get; set; }
        public MailConfig SMTPConfig { get; set; } = new MailConfig();
        public OpenIDConfig OIDCConfig { get; set; } = new OpenIDConfig();
        public bool OpenRegistration { get; set; }
        public bool DisableRegistration { get; set; }
        public ReminderUrgencyConfig ReminderUrgencyConfig { get; set; } = new ReminderUrgencyConfig();
        public string DefaultReminderEmail { get; set; } = string.Empty;
        public bool EnableRootUserOIDC { get; set; }
        public bool EnableAuth { get; set; }
        public List<string> AvailableLocales { get; set; } = new List<string>();
        public string CookieLifeSpan { get; set; } = string.Empty;
        public KestrelAppConfig KestrelAppConfig { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace CarCareTracker.Models
{
    public class ServerConfig
    {
        [JsonPropertyName("POSTGRES_CONNECTION")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PostgresConnection { get; set; }

        [JsonPropertyName("LUBELOGGER_ALLOWED_FILE_EXTENSIONS")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AllowedFileExtensions { get; set; }

        [JsonPropertyName("LUBELOGGER_LOGO_URL")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CustomLogoURL { get; set; }

        [JsonPropertyName("LUBELOGGER_LOGO_SMALL_URL")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CustomSmallLogoURL { get; set; }

        [JsonPropertyName("LUBELOGGER_MOTD")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MessageOfTheDay { get; set; }

        [JsonPropertyName("LUBELOGGER_WEBHOOK")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? WebHookURL { get; set; }

        [JsonPropertyName("LUBELOGGER_DOMAIN")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ServerURL { get; set; }

        [JsonPropertyName("LUBELOGGER_CUSTOM_WIDGETS")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? CustomWidgetsEnabled { get; set; }

        [JsonPropertyName("LUBELOGGER_INVARIANT_API")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? InvariantAPIEnabled { get; set; }

        [JsonPropertyName("MailConfig")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MailConfig? SMTPConfig { get; set; }

        [JsonPropertyName("OpenIDConfig")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenIDConfig? OIDCConfig { get; set; }

        [JsonPropertyName("ReminderUrgencyConfig")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ReminderUrgencyConfig? ReminderUrgencyConfig { get; set; }

        [JsonPropertyName("LUBELOGGER_OPEN_REGISTRATION")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? OpenRegistration { get; set; }

        [JsonPropertyName("DisableRegistration")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? DisableRegistration { get; set; }

        [JsonPropertyName("DefaultReminderEmail")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DefaultReminderEmail { get; set; } = string.Empty;

        [JsonPropertyName("EnableRootUserOIDC")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? EnableRootUserOIDC { get; set; }

        [JsonPropertyName("LUBELOGGER_LOCALE_OVERRIDE")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? LocaleOverride { get; set; } = string.Empty;
        [JsonPropertyName("LUBELOGGER_LOCALE_DT_OVERRIDE")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? LocaleDateTimeOverride { get; set; } = string.Empty;
        [JsonPropertyName("LUBELOGGER_COOKIE_LIFESPAN")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CookieLifeSpan { get; set; } = string.Empty;
    }
}
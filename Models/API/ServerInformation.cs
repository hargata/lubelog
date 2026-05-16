namespace CarCareTracker.Models
{
    /// <summary>
    /// Response object representing server locale configuration
    /// </summary>
    public class ServerInformation
    {
        public string CurrentVersion { get; set; } = string.Empty;
        public string Locale { get; set; } = string.Empty;
        public string CurrencySymbol { get; set; } = string.Empty;
        public string DecimalSeparator { get; set; } = string.Empty;
        public string DateFormat { get; set; } = string.Empty;
    }
}
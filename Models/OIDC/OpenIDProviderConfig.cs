namespace CarCareTracker.Models
{
    /// <summary>
    /// Imported data from .well-known endpoint
    /// </summary>
    public class OpenIDProviderConfig
    {
        public string authorization_endpoint { get; set; } = string.Empty;
        public string token_endpoint { get; set; } = string.Empty;
        public string userinfo_endpoint { get; set; } = string.Empty;
        public string jwks_uri { get; set; } = string.Empty;
        public string end_session_endpoint { get; set; } = string.Empty;
    }
}

namespace CarCareTracker.Models
{
    /// <summary>
    /// Imported data from .well-known endpoint
    /// </summary>
    public class OpenIDProviderConfig
    {
        public string authorization_endpoint { get; set; }
        public string token_endpoint { get; set; }
        public string userinfo_endpoint { get; set; }
        public string jwks_uri { get; set; }
        public string end_session_endpoint { get; set; }
    }
}

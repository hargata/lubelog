using System.Text.Json.Serialization;

namespace CarCareTracker.Models
{
    public class KestrelAppConfig
    {
        public KestrelAppConfigEndpoints Endpoints { get; set; } = new KestrelAppConfigEndpoints();
    }
    public class KestrelAppConfigEndpoints
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public KestrelAppConfigHttpEndpoint? Http { get; set; } = new KestrelAppConfigHttpEndpoint();
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public KestrelAppConfigHttpsEndpoint? HttpsInlineCertFile { get; set; } = new KestrelAppConfigHttpsEndpoint();
    }
    public class KestrelAppConfigHttpEndpoint
    {
        public string? Url { get; set; } = string.Empty;
    }
    public class KestrelAppConfigHttpsEndpoint
    {
        public string? Url { get; set; } = string.Empty;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public KestrelAppConfigHttpsCertificate? Certificate { get; set; } = new KestrelAppConfigHttpsCertificate();
    }
    public class KestrelAppConfigHttpsCertificate
    {
        public string? Path { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Password { get; set; }
    }
}

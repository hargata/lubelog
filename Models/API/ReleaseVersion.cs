namespace CarCareTracker.Models
{
    /// <summary>
    /// For deserializing GitHub response for latest version
    /// </summary>
    public class ReleaseResponse
    {
        public string tag_name { get; set; }
    }
    /// <summary>
    /// For returning the version numbers via API.
    /// </summary>
    public class ReleaseVersion
    {
        public string CurrentVersion { get; set; }
        public string LatestVersion { get; set; }
    }
}

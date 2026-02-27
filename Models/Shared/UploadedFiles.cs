namespace CarCareTracker.Models
{
    public class UploadedFiles
    {
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsPending { get; set; }
    }
}

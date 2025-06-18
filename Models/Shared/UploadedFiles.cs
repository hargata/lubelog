namespace CarCareTracker.Models
{
    public class UploadedFiles
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public FileUploadType UploadType { get; set; } = FileUploadType.File;
        public string FileExtension { get { return Path.GetExtension(Location); } }
        public bool IsPending { get; set; }
    }
}

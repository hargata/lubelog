namespace CarCareTracker.Models
{
    public class VehicleImageMap
    {
        public string ImageLink { get; set; } = string.Empty;
        public int Height { get; set; }
        public int Width { get; set; }
        public List<ImageMap> Map { get; set; } = new List<ImageMap>();
    }
    public class ImageMap
    {
        public string Tags { get; set; } = string.Empty;
        public string Coordinates { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal Opacity { get; set; }
        public string Shape { get; set; } = string.Empty;
    }
}

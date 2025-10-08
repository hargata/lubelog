namespace CarCareTracker.Models
{
    public class VehicleImageMap
    {
        public string ImageLink { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public List<ImageMap> Map { get; set; } = new List<ImageMap>();
    }
    public class ImageMap
    {
        public string Tags { get; set; }
        public string Coordinates { get; set; }
        public string Color { get; set; }
        public decimal Opacity { get; set; }
        public string Shape { get; set; }
    }
}

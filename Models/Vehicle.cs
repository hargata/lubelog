namespace CarCareTracker.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string ImageLocation { get; set; } = "/defaults/noimage.png";
        public int Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string LicensePlate { get; set; }
        public bool IsElectric { get; set; } = false;
    }
}

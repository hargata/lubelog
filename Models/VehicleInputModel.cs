namespace CarCareTracker.Models
{
    public class VehicleInputModel
    {
        public int Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string LicensePlate { get; set; }
        public IFormFile Image { get; set; }
        public List<string> Errors { get; set; }
    }
}

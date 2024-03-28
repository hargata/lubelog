namespace CarCareTracker.Models
{
    public class VehicleViewModel
    {
        public int Id { get; set; }
        public string ImageLocation { get; set; } = "/defaults/noimage.png";
        public int Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string LicensePlate { get; set; }
        public string SoldDate { get; set; }
        public bool IsElectric { get; set; } = false;
        public bool UseHours { get; set; } = false;
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<string> Tags { get; set; } = new List<string>();
        public int LastReportedMileage;
        public bool HasReminders = false;
    }
}

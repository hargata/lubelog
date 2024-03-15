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
        public string PurchaseDate { get; set; }
        public string SoldDate { get; set; }
        public bool IsElectric { get; set; } = false;
        public bool UseHours { get; set; } = false;
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<string> Tags { get; set; } = new List<string>();
        public string OdometerModifier { get; set; } = "1";
        public string OdometerAdjustment { get; set; } = "0";
    }
}

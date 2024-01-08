namespace CarCareTracker.Models
{
    /// <summary>
    /// Import model used for importing Gas records.
    /// </summary>
    public class ImportModel
    {
        public string Date { get; set; }
        public string Odometer { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string FuelConsumed { get; set; }
        public string Cost { get; set; }
        public string Price { get; set; }
        public string PartialFuelUp { get; set; }
        public string IsFillToFull { get; set; }
    }
}

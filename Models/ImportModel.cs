namespace CarCareTracker.Models
{
    /// <summary>
    /// Import model used for importing records via CSV.
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
        public string MissedFuelUp { get; set; }
    }

    public class ServiceRecordExportModel
    {
        public string Date { get; set; }
        public string Odometer { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string Cost { get; set; }
    }
    public class TaxRecordExportModel
    {
        public string Date { get; set; }
        public string Odometer { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string Cost { get; set; }
    }
    public class GasRecordExportModel
    {
        public string Date { get; set; }
        public string Odometer { get; set; }
        public string FuelConsumed { get; set; }
        public string Cost { get; set; }
        public string FuelEconomy { get; set; }
    }
    public class ReminderExportModel
    {
        public string Description { get; set; }
        public string Urgency { get; set; }
        public string Metric { get; set; }
        public string Notes { get; set; }
    }
}

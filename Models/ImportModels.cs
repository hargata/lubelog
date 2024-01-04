namespace CarCareTracker.Models
{
    /// <summary>
    /// Import model used for importing Gas records.
    /// </summary>
    public class GasRecordImport
    {
        public DateTime Date { get; set; }
        public int Odometer { get; set; }
        public decimal FuelConsumed { get; set; }
        public decimal Cost { get; set; }
    }
    /// <summary>
    /// Import model used for importing Service and Repair records.
    /// </summary>
    public class ServiceRecordImport
    {
        public DateTime Date { get; set; }
        public int Odometer { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public decimal Cost { get; set; }
    }
    /// <summary>
    /// Import model used for importing tax records.
    /// </summary>
    public class TaxRecordImport
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public decimal Cost { get; set; }
    }
}

namespace CarCareTracker.Models
{
    public class GasRecordImport
    {
        public DateTime Date { get; set; }
        public int Odometer { get; set; }
        public decimal FuelConsumed { get; set; }
        public decimal Cost { get; set; }
    }
}

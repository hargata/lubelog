namespace CarCareTracker.Models
{
    public class AdditionalFuelViewModel
    {
        public AdditionalFuelType FuelType { get; set; }
        public decimal FuelConsumed { get; set; }
        public decimal Cost { get; set; }
        public decimal CostPerUnit { get; set; }
        public bool IsFillToFull { get; set; }
        /// <summary>
        /// Distance traveled since this fuel type was last refueled.
        /// </summary>
        public int DeltaMileage { get; set; }
        public decimal FuelEconomy { get; set; }
        public bool IncludeInAverage { get; set; }
    }
}

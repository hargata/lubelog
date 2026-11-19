namespace CarCareTracker.Models
{
    /// <summary>
    /// Consumption and cost of a fluid other than the vehicle's primary fuel(e.g.: DEF), recorded as part of a Gas Record.
    /// </summary>
    public class AdditionalFuel
    {
        public AdditionalFuelType FuelType { get; set; }
        public decimal FuelConsumed { get; set; }
        public decimal Cost { get; set; }
        public bool IsFillToFull { get; set; } = true;
    }
}

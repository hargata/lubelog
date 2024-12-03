namespace CarCareTracker.Models.API.v2
{
    public class GasRecordApiModel
    {
        public string Date { get; set; }
        public int Odometer { get; set; } = -1;
        public decimal FuelConsumed { get; set; }
        public decimal Cost { get; set; } = (decimal)-1.0;
        public decimal FuelEconomy { get; set; } = (decimal)-1.0;
        public bool IsFillToFull { get; set; }
        public bool MissedFuelUp { get; set; }
        public string Notes { get; set; }
        public string Tags { get; set; }
        public List<ExtraField> ExtraFields { get; set; }
    }
}

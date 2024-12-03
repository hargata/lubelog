namespace CarCareTracker.Models.API.v2
{
    public class OdometerRecordApiModel
    {
        public string Date { get; set; }
        public int InitialOdometer { get; set; }
        public int Odometer { get; set; } = -1;
        public string Notes { get; set; }
        public string Tags { get; set; }
        public List<ExtraField> ExtraFields { get; set; }
    }
}

namespace CarCareTracker.Models
{
    public class ReportHeader
    {
        public int MaxOdometer { get; set; }
        public int DistanceTraveled { get; set; }
        public decimal TotalCost { get; set; }
        public string AverageMPG { get; set; } = string.Empty;
    }
}

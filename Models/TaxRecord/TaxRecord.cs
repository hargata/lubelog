namespace CarCareTracker.Models
{
    public class TaxRecord
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public string Notes { get; set; }
        public bool IsRecurring { get; set; } = false;
        public ReminderMonthInterval RecurringInterval { get; set; } = ReminderMonthInterval.OneYear;
        public int CustomMonthInterval { get; set; } = 0;
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
    }
}

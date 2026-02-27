namespace CarCareTracker.Models
{
    public class TaxRecordInput
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public List<int> ReminderRecordId { get; set; } = new List<int>();
        public string Date { get; set; } = DateTime.Now.ToShortDateString();
        public string Description { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool IsRecurring { get; set; } = false;
        public ReminderMonthInterval RecurringInterval { get; set; } = ReminderMonthInterval.ThreeMonths;
        public int CustomMonthInterval { get; set; } = 0;
        public ReminderIntervalUnit CustomMonthIntervalUnit { get; set; } = ReminderIntervalUnit.Months;
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public TaxRecord ToTaxRecord() { return new TaxRecord { 
            Id = Id, 
            VehicleId = VehicleId, 
            Date = DateTime.Parse(Date), 
            Cost = Cost, 
            Description = Description, 
            Notes = Notes, 
            IsRecurring = IsRecurring,
            RecurringInterval = RecurringInterval,
            CustomMonthInterval = CustomMonthInterval,
            CustomMonthIntervalUnit = CustomMonthIntervalUnit,
            Files = Files,
            Tags = Tags,
            ExtraFields = ExtraFields
        }; }
    }
}

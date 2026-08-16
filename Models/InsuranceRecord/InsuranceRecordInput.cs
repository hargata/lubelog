namespace CarCareTracker.Models
{
    public class InsuranceRecordInput
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public List<int> ReminderRecordId { get; set; } = new List<int>();
        public string Date { get; set; } = DateTime.Now.ToShortDateString();
        public string Description { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public decimal TotalPremium { get; set; } = 0;
        public List<InsurancePayment> Payments { get; set; } = new List<InsurancePayment>();
        public string Notes { get; set; } = string.Empty;
        public bool IsRecurring { get; set; } = false;
        public ReminderMonthInterval RecurringInterval { get; set; } = ReminderMonthInterval.OneYear;
        public int CustomMonthInterval { get; set; } = 0;
        public ReminderIntervalUnit CustomMonthIntervalUnit { get; set; } = ReminderIntervalUnit.Months;
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public InsuranceRecord ToInsuranceRecord() { return new InsuranceRecord {
            Id = Id,
            VehicleId = VehicleId,
            Date = DateTime.Parse(Date),
            Cost = (Payments != null && Payments.Any()) ? Payments.Sum(x => x.Amount) : Cost,
            TotalPremium = TotalPremium,
            Payments = Payments ?? new List<InsurancePayment>(),
            Description = Description,
            Provider = Provider,
            PolicyNumber = PolicyNumber,
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

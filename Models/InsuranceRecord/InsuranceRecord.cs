namespace CarCareTracker.Models
{
    public class InsuranceRecord
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string PolicyNumber { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public decimal TotalPremium { get; set; } = 0;
        public List<InsurancePayment> Payments { get; set; } = new List<InsurancePayment>();
        public decimal AmountPaid { get { return Payments.Sum(x => x.Amount); } }
        public decimal RemainingBalance { get { return TotalPremium - AmountPaid; } }
        public bool IsPaidInFull { get { return TotalPremium > 0 && AmountPaid >= TotalPremium; } }
        public string Notes { get; set; } = string.Empty;
        public bool IsRecurring { get; set; } = false;
        public ReminderMonthInterval RecurringInterval { get; set; } = ReminderMonthInterval.OneYear;
        public int CustomMonthInterval { get; set; } = 0;
        public ReminderIntervalUnit CustomMonthIntervalUnit { get; set; } = ReminderIntervalUnit.Months;
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<string> Tags { get; set; } = new List<string>();
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
    }
}

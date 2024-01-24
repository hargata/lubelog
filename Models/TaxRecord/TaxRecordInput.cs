namespace CarCareTracker.Models
{
    public class TaxRecordInput
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Date { get; set; } = DateTime.Now.ToShortDateString();
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public string Notes { get; set; }
        public bool IsRecurring { get; set; } = false;
        public ReminderMonthInterval RecurringInterval { get; set; } = ReminderMonthInterval.ThreeMonths;
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public TaxRecord ToTaxRecord() { return new TaxRecord { 
            Id = Id, 
            VehicleId = VehicleId, 
            Date = DateTime.Parse(Date), 
            Cost = Cost, 
            Description = Description, 
            Notes = Notes, 
            IsRecurring = IsRecurring,
            RecurringInterval = RecurringInterval,
            Files = Files }; }
    }
}

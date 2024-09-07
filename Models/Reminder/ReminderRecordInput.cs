namespace CarCareTracker.Models
{
    public class ReminderRecordInput
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Date { get; set; } = DateTime.Now.AddDays(1).ToShortDateString();
        public int Mileage { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public bool IsRecurring { get; set; } = false;
        public bool UseCustomThresholds { get; set; } = false;
        public ReminderUrgencyConfig CustomThresholds { get; set; } = new ReminderUrgencyConfig();
        public int CustomMileageInterval { get; set; } = 0;
        public int CustomMonthInterval { get; set; } = 0;
        public ReminderMileageInterval ReminderMileageInterval { get; set; } = ReminderMileageInterval.FiveThousandMiles;
        public ReminderMonthInterval ReminderMonthInterval { get; set; } = ReminderMonthInterval.OneYear;
        public ReminderMetric Metric { get; set; } = ReminderMetric.Date;
        public List<string> Tags { get; set; } = new List<string>();
        public ReminderRecord ToReminderRecord()
        {
            return new ReminderRecord
            {
                Id = Id,
                VehicleId = VehicleId,
                Date = DateTime.Parse(string.IsNullOrWhiteSpace(Date) ? DateTime.Now.AddDays(1).ToShortDateString() : Date),
                Mileage = Mileage,
                Description = Description,
                Metric = Metric,
                IsRecurring = IsRecurring,
                UseCustomThresholds = UseCustomThresholds,
                CustomThresholds = CustomThresholds,
                ReminderMileageInterval = ReminderMileageInterval,
                ReminderMonthInterval = ReminderMonthInterval,
                CustomMileageInterval = CustomMileageInterval,
                CustomMonthInterval = CustomMonthInterval,
                Notes = Notes,
                Tags = Tags
            };
        }
    }
}

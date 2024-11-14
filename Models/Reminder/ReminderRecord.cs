﻿namespace CarCareTracker.Models
{
    public class ReminderRecord
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public DateTime Date { get; set; }
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
    }
}

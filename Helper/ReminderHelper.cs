using CarCareTracker.Models;

namespace CarCareTracker.Helper
{
    public interface IReminderHelper
    {
        ReminderRecord GetUpdatedRecurringReminderRecord(ReminderRecord existingReminder);
        List<ReminderRecordViewModel> GetReminderRecordViewModels(List<ReminderRecord> reminders, int currentMileage, DateTime dateCompare);
    }
    public class ReminderHelper: IReminderHelper
    {
        private readonly IConfigHelper _config;
        public ReminderHelper(IConfigHelper config)
        {
            _config = config;
        }
        public ReminderRecord GetUpdatedRecurringReminderRecord(ReminderRecord existingReminder)
        {
            if (existingReminder.Metric == ReminderMetric.Both)
            {
                if (existingReminder.ReminderMonthInterval != ReminderMonthInterval.Other)
                {
                    existingReminder.Date = existingReminder.Date.AddMonths((int)existingReminder.ReminderMonthInterval);
                } else
                {
                    existingReminder.Date = existingReminder.Date.AddMonths(existingReminder.CustomMonthInterval);
                }
               
                if (existingReminder.ReminderMileageInterval != ReminderMileageInterval.Other)
                {
                    existingReminder.Mileage += (int)existingReminder.ReminderMileageInterval;
                }
                else
                {
                    existingReminder.Mileage += existingReminder.CustomMileageInterval;
                }
            }
            else if (existingReminder.Metric == ReminderMetric.Odometer)
            {
                if (existingReminder.ReminderMileageInterval != ReminderMileageInterval.Other)
                {
                    existingReminder.Mileage += (int)existingReminder.ReminderMileageInterval;
                } else
                {
                    existingReminder.Mileage += existingReminder.CustomMileageInterval;
                }
            }
            else if (existingReminder.Metric == ReminderMetric.Date)
            {
                if (existingReminder.ReminderMonthInterval != ReminderMonthInterval.Other)
                {
                    existingReminder.Date = existingReminder.Date.AddMonths((int)existingReminder.ReminderMonthInterval);
                }
                else
                {
                    existingReminder.Date = existingReminder.Date.AddMonths(existingReminder.CustomMonthInterval);
                }
            }
            return existingReminder;
        }
        public List<ReminderRecordViewModel> GetReminderRecordViewModels(List<ReminderRecord> reminders, int currentMileage, DateTime dateCompare)
        {
            List<ReminderRecordViewModel> reminderViewModels = new List<ReminderRecordViewModel>();
            var reminderUrgencyConfig = _config.GetReminderUrgencyConfig();
            foreach (var reminder in reminders)
            {
                var reminderViewModel = new ReminderRecordViewModel()
                {
                    Id = reminder.Id,
                    VehicleId = reminder.VehicleId,
                    Date = reminder.Date,
                    Mileage = reminder.Mileage,
                    Description = reminder.Description,
                    Notes = reminder.Notes,
                    Metric = reminder.Metric,
                    IsRecurring = reminder.IsRecurring,
                    Tags = reminder.Tags
                };
                if (reminder.Metric == ReminderMetric.Both)
                {
                    if (reminder.Date < dateCompare)
                    {
                        reminderViewModel.Urgency = ReminderUrgency.PastDue;
                        reminderViewModel.Metric = ReminderMetric.Date;
                    }
                    else if (reminder.Mileage < currentMileage)
                    {
                        reminderViewModel.Urgency = ReminderUrgency.PastDue;
                        reminderViewModel.Metric = ReminderMetric.Odometer;
                    }
                    else if (reminder.Date < dateCompare.AddDays(reminderUrgencyConfig.VeryUrgentDays))
                    {
                        //if less than a week from today or less than 50 miles from current mileage then very urgent.
                        reminderViewModel.Urgency = ReminderUrgency.VeryUrgent;
                        //have to specify by which metric this reminder is urgent.
                        reminderViewModel.Metric = ReminderMetric.Date;
                    }
                    else if (reminder.Mileage < currentMileage + reminderUrgencyConfig.VeryUrgentDistance)
                    {
                        reminderViewModel.Urgency = ReminderUrgency.VeryUrgent;
                        reminderViewModel.Metric = ReminderMetric.Odometer;
                    }
                    else if (reminder.Date < dateCompare.AddDays(reminderUrgencyConfig.UrgentDays))
                    {
                        reminderViewModel.Urgency = ReminderUrgency.Urgent;
                        reminderViewModel.Metric = ReminderMetric.Date;
                    }
                    else if (reminder.Mileage < currentMileage + reminderUrgencyConfig.UrgentDistance)
                    {
                        reminderViewModel.Urgency = ReminderUrgency.Urgent;
                        reminderViewModel.Metric = ReminderMetric.Odometer;
                    }
                }
                else if (reminder.Metric == ReminderMetric.Date)
                {
                    if (reminder.Date < dateCompare)
                    {
                        reminderViewModel.Urgency = ReminderUrgency.PastDue;
                    }
                    else if (reminder.Date < dateCompare.AddDays(reminderUrgencyConfig.VeryUrgentDays))
                    {
                        reminderViewModel.Urgency = ReminderUrgency.VeryUrgent;
                    }
                    else if (reminder.Date < dateCompare.AddDays(reminderUrgencyConfig.UrgentDays))
                    {
                        reminderViewModel.Urgency = ReminderUrgency.Urgent;
                    }
                }
                else if (reminder.Metric == ReminderMetric.Odometer)
                {
                    if (reminder.Mileage < currentMileage)
                    {
                        reminderViewModel.Urgency = ReminderUrgency.PastDue;
                        reminderViewModel.Metric = ReminderMetric.Odometer;
                    }
                    else if (reminder.Mileage < currentMileage + reminderUrgencyConfig.VeryUrgentDistance)
                    {
                        reminderViewModel.Urgency = ReminderUrgency.VeryUrgent;
                    }
                    else if (reminder.Mileage < currentMileage + reminderUrgencyConfig.UrgentDistance)
                    {
                        reminderViewModel.Urgency = ReminderUrgency.Urgent;
                    }
                }
                reminderViewModels.Add(reminderViewModel);
            }
            return reminderViewModels;
        }
    }
}

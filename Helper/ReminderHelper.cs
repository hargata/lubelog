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
        public ReminderRecord GetUpdatedRecurringReminderRecord(ReminderRecord existingReminder)
        {
            if (existingReminder.Metric == ReminderMetric.Both)
            {
                existingReminder.Date = existingReminder.Date.AddMonths((int)existingReminder.ReminderMonthInterval);
                existingReminder.Mileage += (int)existingReminder.ReminderMileageInterval;
            }
            else if (existingReminder.Metric == ReminderMetric.Odometer)
            {
                existingReminder.Mileage += (int)existingReminder.ReminderMileageInterval;
            }
            else if (existingReminder.Metric == ReminderMetric.Date)
            {
                existingReminder.Date = existingReminder.Date.AddMonths((int)existingReminder.ReminderMonthInterval);
            }
            return existingReminder;
        }
        public List<ReminderRecordViewModel> GetReminderRecordViewModels(List<ReminderRecord> reminders, int currentMileage, DateTime dateCompare)
        {
            List<ReminderRecordViewModel> reminderViewModels = new List<ReminderRecordViewModel>();
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
                    IsRecurring = reminder.IsRecurring
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
                    else if (reminder.Date < dateCompare.AddDays(7))
                    {
                        //if less than a week from today or less than 50 miles from current mileage then very urgent.
                        reminderViewModel.Urgency = ReminderUrgency.VeryUrgent;
                        //have to specify by which metric this reminder is urgent.
                        reminderViewModel.Metric = ReminderMetric.Date;
                    }
                    else if (reminder.Mileage < currentMileage + 50)
                    {
                        reminderViewModel.Urgency = ReminderUrgency.VeryUrgent;
                        reminderViewModel.Metric = ReminderMetric.Odometer;
                    }
                    else if (reminder.Date < dateCompare.AddDays(30))
                    {
                        reminderViewModel.Urgency = ReminderUrgency.Urgent;
                        reminderViewModel.Metric = ReminderMetric.Date;
                    }
                    else if (reminder.Mileage < currentMileage + 100)
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
                    else if (reminder.Date < dateCompare.AddDays(7))
                    {
                        reminderViewModel.Urgency = ReminderUrgency.VeryUrgent;
                    }
                    else if (reminder.Date < dateCompare.AddDays(30))
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
                    else if (reminder.Mileage < currentMileage + 50)
                    {
                        reminderViewModel.Urgency = ReminderUrgency.VeryUrgent;
                    }
                    else if (reminder.Mileage < currentMileage + 100)
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

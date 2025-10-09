using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public partial class VehicleController
    {
        private List<ReminderRecordViewModel> GetRemindersAndUrgency(int vehicleId, DateTime dateCompare)
        {
            var currentMileage = _vehicleLogic.GetMaxMileage(vehicleId);
            var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
            List<ReminderRecordViewModel> results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, dateCompare);
            return results;
        }
        private bool GetAndUpdateVehicleUrgentOrPastDueReminders(int vehicleId)
        {
            var result = GetRemindersAndUrgency(vehicleId, DateTime.Now);
            //check if user wants auto-refresh past-due reminders
            if (_config.GetUserConfig(User).EnableAutoReminderRefresh)
            {
                //check for past due reminders that are eligible for recurring.
                var pastDueAndRecurring = result.Where(x => x.Urgency == ReminderUrgency.PastDue && x.IsRecurring);
                if (pastDueAndRecurring.Any())
                {
                    foreach (ReminderRecordViewModel reminderRecord in pastDueAndRecurring)
                    {
                        //update based on recurring intervals.
                        //pull reminderRecord based on ID
                        var existingReminder = _reminderRecordDataAccess.GetReminderRecordById(reminderRecord.Id);
                        existingReminder = _reminderHelper.GetUpdatedRecurringReminderRecord(existingReminder, null, null);
                        //save to db.
                        _reminderRecordDataAccess.SaveReminderRecordToVehicle(existingReminder);
                        //set urgency to not urgent so it gets excluded in count.
                        reminderRecord.Urgency = ReminderUrgency.NotUrgent;
                    }
                }
            }
            //check for very urgent or past due reminders that were not eligible for recurring.
            var pastDueAndUrgentReminders = result.Where(x => x.Urgency == ReminderUrgency.VeryUrgent || x.Urgency == ReminderUrgency.PastDue);
            if (pastDueAndUrgentReminders.Any())
            {
                return true;
            }
            return false;
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetVehicleHaveUrgentOrPastDueReminders(int vehicleId)
        {
            var result = GetAndUpdateVehicleUrgentOrPastDueReminders(vehicleId);
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetReminderRecordsByVehicleId(int vehicleId)
        {
            var result = GetRemindersAndUrgency(vehicleId, DateTime.Now);
            result = result.OrderByDescending(x => x.Urgency).ToList();
            return PartialView("Reminder/_ReminderRecords", result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetRecurringReminderRecordsByVehicleId(int vehicleId)
        {
            var result = GetRemindersAndUrgency(vehicleId, DateTime.Now);
            result.RemoveAll(x => !x.IsRecurring);
            result = result.OrderByDescending(x => x.Urgency).ThenBy(x => x.Description).ToList();
            return PartialView("_RecurringReminderSelector", result);
        }
        [HttpPost]
        public IActionResult PushbackRecurringReminderRecord(int reminderRecordId)
        {
            var result = PushbackRecurringReminderRecordWithChecks(reminderRecordId, null, null);
            return Json(result);
        }
        private bool PushbackRecurringReminderRecordWithChecks(int reminderRecordId, DateTime? currentDate, int? currentMileage)
        {
            try
            {
                var existingReminder = _reminderRecordDataAccess.GetReminderRecordById(reminderRecordId);
                if (existingReminder is not null && existingReminder.Id != default && existingReminder.IsRecurring)
                {
                    existingReminder = _reminderHelper.GetUpdatedRecurringReminderRecord(existingReminder, currentDate, currentMileage);
                    //save to db.
                    var reminderUpdateResult = _reminderRecordDataAccess.SaveReminderRecordToVehicle(existingReminder);
                    if (!reminderUpdateResult)
                    {
                        _logger.LogError("Unable to update reminder either because the reminder no longer exists or is no longer recurring");
                        return false;
                    }
                    return true;
                }
                else
                {
                    _logger.LogError("Unable to update reminder because it no longer exists.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }
        [HttpPost]
        public IActionResult SaveReminderRecordToVehicleId(ReminderRecordInput reminderRecord)
        {
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), reminderRecord.VehicleId))
            {
                return Json(false);
            }
            var result = _reminderRecordDataAccess.SaveReminderRecordToVehicle(reminderRecord.ToReminderRecord());
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromReminderRecord(reminderRecord.ToReminderRecord(), reminderRecord.Id == default ? "reminderrecord.add" : "reminderrecord.update", User.Identity.Name));
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult GetAddReminderRecordPartialView(ReminderRecordInput? reminderModel)
        {
            if (reminderModel is not null)
            {
                return PartialView("Reminder/_ReminderRecordModal", reminderModel);
            }
            else
            {
                return PartialView("Reminder/_ReminderRecordModal", new ReminderRecordInput());
            }
        }
        [HttpGet]
        public IActionResult GetReminderRecordForEditById(int reminderRecordId)
        {
            var result = _reminderRecordDataAccess.GetReminderRecordById(reminderRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), result.VehicleId))
            {
                return Redirect("/Error/Unauthorized");
            }
            //convert to Input object.
            var convertedResult = new ReminderRecordInput
            {
                Id = result.Id,
                Date = result.Date.ToShortDateString(),
                Description = result.Description,
                Notes = result.Notes,
                VehicleId = result.VehicleId,
                Mileage = result.Mileage,
                Metric = result.Metric,
                IsRecurring = result.IsRecurring,
                FixedIntervals = result.FixedIntervals,
                UseCustomThresholds = result.UseCustomThresholds,
                CustomThresholds = result.CustomThresholds,
                ReminderMileageInterval = result.ReminderMileageInterval,
                ReminderMonthInterval = result.ReminderMonthInterval,
                CustomMileageInterval = result.CustomMileageInterval,
                CustomMonthInterval = result.CustomMonthInterval,
                CustomMonthIntervalUnit = result.CustomMonthIntervalUnit,
                Tags = result.Tags
            };
            return PartialView("Reminder/_ReminderRecordModal", convertedResult);
        }
        private bool DeleteReminderRecordWithChecks(int reminderRecordId)
        {
            var existingRecord = _reminderRecordDataAccess.GetReminderRecordById(reminderRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
            {
                return false;
            }
            var result = _reminderRecordDataAccess.DeleteReminderRecordById(existingRecord.Id);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromReminderRecord(existingRecord, "reminderrecord.delete", User.Identity.Name));
            }
            return result;
        }
        [HttpPost]
        public IActionResult DeleteReminderRecordById(int reminderRecordId)
        {
            var result = DeleteReminderRecordWithChecks(reminderRecordId);
            return Json(result);
        }
    }
}

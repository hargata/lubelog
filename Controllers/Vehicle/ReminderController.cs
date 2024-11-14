﻿using CarCareTracker.Filter;
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
            return PartialView("_ReminderRecords", result);
        }
        [HttpGet]
        public IActionResult GetRecurringReminderRecordsByVehicleId(int vehicleId)
        {
            var result = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
            result.RemoveAll(x => !x.IsRecurring);
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
            var result = _reminderRecordDataAccess.SaveReminderRecordToVehicle(reminderRecord.ToReminderRecord());
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), reminderRecord.VehicleId, User.Identity.Name, $"{(reminderRecord.Id == default ? "Created" : "Edited")} Reminder - Description: {reminderRecord.Description}");
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult GetAddReminderRecordPartialView(ReminderRecordInput? reminderModel)
        {
            if (reminderModel is not null)
            {
                return PartialView("_ReminderRecordModal", reminderModel);
            }
            else
            {
                return PartialView("_ReminderRecordModal", new ReminderRecordInput());
            }
        }
        [HttpGet]
        public IActionResult GetReminderRecordForEditById(int reminderRecordId)
        {
            var result = _reminderRecordDataAccess.GetReminderRecordById(reminderRecordId);
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
                UseCustomThresholds = result.UseCustomThresholds,
                CustomThresholds = result.CustomThresholds,
                ReminderMileageInterval = result.ReminderMileageInterval,
                ReminderMonthInterval = result.ReminderMonthInterval,
                CustomMileageInterval = result.CustomMileageInterval,
                CustomMonthInterval = result.CustomMonthInterval,
                Tags = result.Tags
            };
            return PartialView("_ReminderRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeleteReminderRecordById(int reminderRecordId)
        {
            var result = _reminderRecordDataAccess.DeleteReminderRecordById(reminderRecordId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Reminder - Id: {reminderRecordId}");
            }
            return Json(result);
        }
    }
}

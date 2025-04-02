﻿using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public partial class VehicleController
    {
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetTaxRecordsByVehicleId(int vehicleId)
        {
            var result = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
            bool _useDescending = _config.GetUserConfig(User).UseDescending;
            if (_useDescending)
            {
                result = result.OrderByDescending(x => x.Date).ToList();
            }
            else
            {
                result = result.OrderBy(x => x.Date).ToList();
            }
            return PartialView("_TaxRecords", result);
        }

        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        public IActionResult CheckRecurringTaxRecords(int vehicleId)
        {
            try
            {
                var result = _vehicleLogic.UpdateRecurringTaxes(vehicleId);
                return Json(result);
            } catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(false);
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveTaxRecordToVehicleId(TaxRecordInput taxRecord)
        {
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), taxRecord.VehicleId))
            {
                return Json(false);
            }
            //move files from temp.
            taxRecord.Files = taxRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            //push back any reminders
            if (taxRecord.ReminderRecordId.Any())
            {
                foreach (int reminderRecordId in taxRecord.ReminderRecordId)
                {
                    PushbackRecurringReminderRecordWithChecks(reminderRecordId, DateTime.Parse(taxRecord.Date), null);
                }
            }
            var result = _taxRecordDataAccess.SaveTaxRecordToVehicle(taxRecord.ToTaxRecord());
            _vehicleLogic.UpdateRecurringTaxes(taxRecord.VehicleId);
            if (result)
            {
                await _notificationService.NotifyAsync(WebHookPayload.FromTaxRecord(taxRecord.ToTaxRecord(), taxRecord.Id == default ? "taxrecord.add" : "taxrecord.update", User.Identity.Name));
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddTaxRecordPartialView()
        {
            return PartialView("_TaxRecordModal", new TaxRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.TaxRecord).ExtraFields });
        }
        [HttpGet]
        public IActionResult GetTaxRecordForEditById(int taxRecordId)
        {
            var result = _taxRecordDataAccess.GetTaxRecordById(taxRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), result.VehicleId))
            {
                return Redirect("/Error/Unauthorized");
            }
            //convert to Input object.
            var convertedResult = new TaxRecordInput
            {
                Id = result.Id,
                Cost = result.Cost,
                Date = result.Date.ToShortDateString(),
                Description = result.Description,
                Notes = result.Notes,
                VehicleId = result.VehicleId,
                IsRecurring = result.IsRecurring,
                RecurringInterval = result.RecurringInterval,
                CustomMonthInterval = result.CustomMonthInterval,
                CustomMonthIntervalUnit = result.CustomMonthIntervalUnit,
                Files = result.Files,
                Tags = result.Tags,
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.TaxRecord).ExtraFields)
            };
            return PartialView("_TaxRecordModal", convertedResult);
        }
        private async Task<bool> DeleteTaxRecordWithChecks(int taxRecordId)
        {
            var existingRecord = _taxRecordDataAccess.GetTaxRecordById(taxRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
            {
                return false;
            }
            var result = _taxRecordDataAccess.DeleteTaxRecordById(existingRecord.Id);
            if (result)
            {
                await _notificationService.NotifyAsync(WebHookPayload.FromTaxRecord(existingRecord, "taxrecord.delete", User.Identity.Name));
            }
            return result;
        }
        [HttpPost]
        public async Task<IActionResult> DeleteTaxRecordById(int taxRecordId)
        {
            var result = await DeleteTaxRecordWithChecks(taxRecordId);
            return Json(result);
        }
    }
}

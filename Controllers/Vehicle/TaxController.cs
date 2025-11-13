using CarCareTracker.Filter;
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
            return PartialView("Tax/_TaxRecords", result);
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
        public IActionResult SaveTaxRecordToVehicleId(TaxRecordInput taxRecord)
        {
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), taxRecord.VehicleId, HouseholdPermission.Edit))
            {
                return Json(OperationResponse.Failed("Access Denied"));
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
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromTaxRecord(taxRecord.ToTaxRecord(), taxRecord.Id == default ? "taxrecord.add" : "taxrecord.update", User.Identity.Name));
            }
            return Json(OperationResponse.Conditional(result, string.Empty, StaticHelper.GenericErrorMessage));
        }
        [HttpGet]
        public IActionResult GetAddTaxRecordPartialView()
        {
            return PartialView("Tax/_TaxRecordModal", new TaxRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.TaxRecord).ExtraFields });
        }
        [HttpGet]
        public IActionResult GetTaxRecordForEditById(int taxRecordId)
        {
            var result = _taxRecordDataAccess.GetTaxRecordById(taxRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), result.VehicleId, HouseholdPermission.View))
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
            return PartialView("Tax/_TaxRecordModal", convertedResult);
        }
        private OperationResponse DeleteTaxRecordWithChecks(int taxRecordId)
        {
            var existingRecord = _taxRecordDataAccess.GetTaxRecordById(taxRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Delete))
            {
                return OperationResponse.Failed("Access Denied");
            }
            var result = _taxRecordDataAccess.DeleteTaxRecordById(existingRecord.Id);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromTaxRecord(existingRecord, "taxrecord.delete", User.Identity.Name));
            }
            return OperationResponse.Conditional(result, string.Empty, StaticHelper.GenericErrorMessage);
        }
        [HttpPost]
        public IActionResult DeleteTaxRecordById(int taxRecordId)
        {
            var result = DeleteTaxRecordWithChecks(taxRecordId);
            return Json(result);
        }
    }
}

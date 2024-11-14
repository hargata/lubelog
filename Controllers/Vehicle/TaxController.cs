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
            return PartialView("_TaxRecords", result);
        }
        private void UpdateRecurringTaxes(int vehicleId)
        {
            var result = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
            var recurringFees = result.Where(x => x.IsRecurring);
            if (recurringFees.Any())
            {
                foreach (TaxRecord recurringFee in recurringFees)
                {
                    var newDate = new DateTime();
                    if (recurringFee.RecurringInterval != ReminderMonthInterval.Other)
                    {
                        newDate = recurringFee.Date.AddMonths((int)recurringFee.RecurringInterval);
                    }
                    else
                    {
                        newDate = recurringFee.Date.AddMonths(recurringFee.CustomMonthInterval);
                    }
                    if (DateTime.Now > newDate)
                    {
                        recurringFee.IsRecurring = false;
                        var newRecurringFee = new TaxRecord()
                        {
                            VehicleId = recurringFee.VehicleId,
                            Date = newDate,
                            Description = recurringFee.Description,
                            Cost = recurringFee.Cost,
                            IsRecurring = true,
                            Notes = recurringFee.Notes,
                            RecurringInterval = recurringFee.RecurringInterval,
                            CustomMonthInterval = recurringFee.CustomMonthInterval,
                            Files = recurringFee.Files,
                            Tags = recurringFee.Tags,
                            ExtraFields = recurringFee.ExtraFields
                        };
                        _taxRecordDataAccess.SaveTaxRecordToVehicle(recurringFee);
                        _taxRecordDataAccess.SaveTaxRecordToVehicle(newRecurringFee);
                    }
                }
            }
        }
        [HttpPost]
        public IActionResult SaveTaxRecordToVehicleId(TaxRecordInput taxRecord)
        {
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
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), taxRecord.VehicleId, User.Identity.Name, $"{(taxRecord.Id == default ? "Created" : "Edited")} Tax Record - Description: {taxRecord.Description}");
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
                Files = result.Files,
                Tags = result.Tags,
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.TaxRecord).ExtraFields)
            };
            return PartialView("_TaxRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeleteTaxRecordById(int taxRecordId)
        {
            var result = _taxRecordDataAccess.DeleteTaxRecordById(taxRecordId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Tax Record - Id: {taxRecordId}");
            }
            return Json(result);
        }
    }
}

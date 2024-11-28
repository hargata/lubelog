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

        /// <summary>
        /// Saves a new tax record or updates an existing one
        /// Additional records may be generated if it's a recurring record with date in the past
        /// </summary>
        /// <param name="taxRecordInput"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult SaveTaxRecordToVehicleId(TaxRecordInput taxRecordInput)
        {
            TaxRecord taxRecord = taxRecordInput.ToTaxRecord();

            // Move files from temp.
            taxRecord.Files = taxRecord.Files.Select(x => new UploadedFiles
            {
                Name = x.Name,
                Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/")
            }).ToList();
            
            // Push back any reminders
            if (taxRecordInput.ReminderRecordId.Count != 0)
            {
                foreach (int reminderRecordId in taxRecordInput.ReminderRecordId)
                {
                    PushbackRecurringReminderRecordWithChecks(reminderRecordId, taxRecord.Date, null);
                }
            }

            DateTime currentDate = taxRecord.Date;
            bool result = false;

            // When it's recurring and in the past, insert all records since then
            if (taxRecord.Id == default // records are only created if it's a new record
                && taxRecord.IsRecurring 
                && currentDate < DateTime.Now)
            {
                result = CreatePastTaxRecords(taxRecord, currentDate, result);
            }
            else
            {
                result = _taxRecordDataAccess.SaveTaxRecordToVehicle(taxRecord);
            }

            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(),
                taxRecordInput.VehicleId,
                User.Identity.Name,
                $"{(taxRecordInput.Id == default ? "Created" : "Edited")} Tax Record - Description: {taxRecordInput.Description}");
            }
            return Json(result);
        }

        [HttpGet]
        public IActionResult GetAddTaxRecordPartialView()
        {
            return PartialView("_TaxRecordModal", new TaxRecordInput()
            {
                ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.TaxRecord).ExtraFields
            });
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

        private bool DeleteTaxRecordWithChecks(int taxRecordId)
        {
            var existingRecord = _taxRecordDataAccess.GetTaxRecordById(taxRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
            {
                return false;
            }
            var result = _taxRecordDataAccess.DeleteTaxRecordById(existingRecord.Id);
            return result;
        }

        [HttpPost]
        public IActionResult DeleteTaxRecordById(int taxRecordId)
        {
            var result = DeleteTaxRecordWithChecks(taxRecordId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Tax Record - Id: {taxRecordId}");
            }
            return Json(result);
        }

        /// <summary>
        /// Generates tax records based on a recurring tax record which starts in the past
        /// </summary>
        /// <param name="taxRecord"></param>
        /// <param name="currentDate"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool CreatePastTaxRecords(TaxRecord taxRecord, DateTime currentDate, bool result)
        {
            while (currentDate < DateTime.Now)
            {
                // next date based on the current one
                DateTime nextDate = taxRecord.RecurringInterval != ReminderMonthInterval.Other
                    ? currentDate.AddMonths((int)taxRecord.RecurringInterval)
                    : currentDate.AddMonths(taxRecord.CustomMonthInterval);

                var previousTaxRecord = new TaxRecord
                {
                    VehicleId = taxRecord.VehicleId,
                    Date = currentDate,
                    Description = taxRecord.Description,
                    Cost = taxRecord.Cost,
                    IsRecurring = nextDate >= DateTime.Now, // only the newest/latest taxRecord is recurring / when nextDate is in the future, this record is the newest/latest one
                    Notes = taxRecord.Notes,
                    RecurringInterval = taxRecord.RecurringInterval,
                    CustomMonthInterval = taxRecord.CustomMonthInterval,
                    Files = taxRecord.Files,
                    ExtraFields = taxRecord.ExtraFields
                };

                currentDate = taxRecord.RecurringInterval != ReminderMonthInterval.Other
                    ? currentDate.AddMonths((int)taxRecord.RecurringInterval)
                    : currentDate.AddMonths(taxRecord.CustomMonthInterval);

                result = _taxRecordDataAccess.SaveTaxRecordToVehicle(previousTaxRecord);
            }
            return result;
        }
    }
}

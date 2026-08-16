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
        public IActionResult GetInsuranceRecordsByVehicleId(int vehicleId)
        {
            var result = _insuranceRecordDataAccess.GetInsuranceRecordsByVehicleId(vehicleId);
            bool _useDescending = _config.GetUserConfig(User).UseDescending;
            if (_useDescending)
            {
                result = result.OrderByDescending(x => x.Date).ToList();
            }
            else
            {
                result = result.OrderBy(x => x.Date).ToList();
            }
            return PartialView("Insurance/_InsuranceRecords", result);
        }

        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        public IActionResult CheckRecurringInsuranceRecords(int vehicleId)
        {
            try
            {
                var result = _vehicleLogic.UpdateRecurringInsurance(vehicleId);
                return Json(result);
            } catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(false);
            }
        }
        [HttpPost]
        public IActionResult SaveInsuranceRecordToVehicleId(InsuranceRecordInput insuranceRecord)
        {
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), insuranceRecord.VehicleId, HouseholdPermission.Edit))
            {
                return Json(OperationResponse.Failed("Access Denied"));
            }
            //move files from temp.
            insuranceRecord.Files = insuranceRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            //push back any reminders
            if (insuranceRecord.ReminderRecordId.Any())
            {
                foreach (int reminderRecordId in insuranceRecord.ReminderRecordId)
                {
                    PushbackRecurringReminderRecordWithChecks(reminderRecordId, DateTime.Parse(insuranceRecord.Date), null);
                }
            }
            var result = _insuranceRecordDataAccess.SaveInsuranceRecordToVehicle(insuranceRecord.ToInsuranceRecord());
            _vehicleLogic.UpdateRecurringInsurance(insuranceRecord.VehicleId);
            if (result)
            {
                _eventLogic.PublishEvent(GetUserID(), WebHookPayload.FromInsuranceRecord(insuranceRecord.ToInsuranceRecord(), insuranceRecord.Id == default ? "insurancerecord.add" : "insurancerecord.update", User.Identity?.Name ?? string.Empty));
            }
            return Json(OperationResponse.Conditional(result, string.Empty, StaticHelper.GenericErrorMessage));
        }
        [HttpGet]
        public IActionResult GetAddInsuranceRecordPartialView()
        {
            return PartialView("Insurance/_InsuranceRecordModal", new InsuranceRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.InsuranceRecord).ExtraFields });
        }
        [HttpGet]
        public IActionResult GetInsuranceRecordForEditById(int insuranceRecordId)
        {
            var result = _insuranceRecordDataAccess.GetInsuranceRecordById(insuranceRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), result.VehicleId, HouseholdPermission.View))
            {
                return Redirect("/Error/Unauthorized");
            }
            //convert to Input object.
            var convertedResult = new InsuranceRecordInput
            {
                Id = result.Id,
                Cost = result.Cost,
                TotalPremium = result.TotalPremium,
                Payments = result.Payments,
                Date = result.Date.ToShortDateString(),
                Description = result.Description,
                Provider = result.Provider,
                PolicyNumber = result.PolicyNumber,
                Notes = result.Notes,
                VehicleId = result.VehicleId,
                IsRecurring = result.IsRecurring,
                RecurringInterval = result.RecurringInterval,
                CustomMonthInterval = result.CustomMonthInterval,
                CustomMonthIntervalUnit = result.CustomMonthIntervalUnit,
                Files = result.Files,
                Tags = result.Tags,
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.InsuranceRecord).ExtraFields)
            };
            return PartialView("Insurance/_InsuranceRecordModal", convertedResult);
        }
        private OperationResponse DeleteInsuranceRecordWithChecks(int insuranceRecordId)
        {
            var existingRecord = _insuranceRecordDataAccess.GetInsuranceRecordById(insuranceRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Delete))
            {
                return OperationResponse.Failed("Access Denied");
            }
            var result = _insuranceRecordDataAccess.DeleteInsuranceRecordById(existingRecord.Id);
            if (result)
            {
                _eventLogic.PublishEvent(GetUserID(), WebHookPayload.FromInsuranceRecord(existingRecord, "insurancerecord.delete", User.Identity?.Name ?? string.Empty));
            }
            return OperationResponse.Conditional(result, string.Empty, StaticHelper.GenericErrorMessage);
        }
        [HttpPost]
        public IActionResult DeleteInsuranceRecordById(int insuranceRecordId)
        {
            var result = DeleteInsuranceRecordWithChecks(insuranceRecordId);
            return Json(result);
        }
    }
}

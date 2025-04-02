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
        public IActionResult GetServiceRecordsByVehicleId(int vehicleId)
        {
            var result = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
            bool _useDescending = _config.GetUserConfig(User).UseDescending;
            if (_useDescending)
            {
                result = result.OrderByDescending(x => x.Date).ThenByDescending(x => x.Mileage).ToList();
            }
            else
            {
                result = result.OrderBy(x => x.Date).ThenBy(x => x.Mileage).ToList();
            }
            return PartialView("_ServiceRecords", result);
        }
        [HttpPost]
        public async Task<IActionResult> SaveServiceRecordToVehicleId(ServiceRecordInput serviceRecord)
        {
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), serviceRecord.VehicleId))
            {
                return Json(false);
            }
            if (serviceRecord.Id == default && _config.GetUserConfig(User).EnableAutoOdometerInsert)
            {
                _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                {
                    Date = DateTime.Parse(serviceRecord.Date),
                    VehicleId = serviceRecord.VehicleId,
                    Mileage = serviceRecord.Mileage,
                    Notes = $"Auto Insert From Service Record: {serviceRecord.Description}"
                });
            }
            //move files from temp.
            serviceRecord.Files = serviceRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            if (serviceRecord.Supplies.Any())
            {
                serviceRecord.RequisitionHistory.AddRange(RequisitionSupplyRecordsByUsage(serviceRecord.Supplies, DateTime.Parse(serviceRecord.Date), serviceRecord.Description));
                if (serviceRecord.CopySuppliesAttachment)
                {
                    serviceRecord.Files.AddRange(GetSuppliesAttachments(serviceRecord.Supplies));
                }
            }
            if (serviceRecord.DeletedRequisitionHistory.Any())
            {
                _vehicleLogic.RestoreSupplyRecordsByUsage(serviceRecord.DeletedRequisitionHistory, serviceRecord.Description);
            }
            //push back any reminders
            if (serviceRecord.ReminderRecordId.Any())
            {
                foreach (int reminderRecordId in serviceRecord.ReminderRecordId)
                {
                    PushbackRecurringReminderRecordWithChecks(reminderRecordId, DateTime.Parse(serviceRecord.Date), serviceRecord.Mileage);
                }
            }
            var result = _serviceRecordDataAccess.SaveServiceRecordToVehicle(serviceRecord.ToServiceRecord());
            if (result)
            {
                await _notificationService.NotifyAsync(WebHookPayload.FromGenericRecord(serviceRecord.ToServiceRecord(), serviceRecord.Id == default ? "servicerecord.add" : "servicerecord.update", User.Identity.Name));
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddServiceRecordPartialView()
        {
            return PartialView("_ServiceRecordModal", new ServiceRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.ServiceRecord).ExtraFields });
        }
        [HttpGet]
        public IActionResult GetServiceRecordForEditById(int serviceRecordId)
        {
            var result = _serviceRecordDataAccess.GetServiceRecordById(serviceRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), result.VehicleId))
            {
                return Redirect("/Error/Unauthorized");
            }
            //convert to Input object.
            var convertedResult = new ServiceRecordInput
            {
                Id = result.Id,
                Cost = result.Cost,
                Date = result.Date.ToShortDateString(),
                Description = result.Description,
                Mileage = result.Mileage,
                Notes = result.Notes,
                VehicleId = result.VehicleId,
                Files = result.Files,
                Tags = result.Tags,
                RequisitionHistory = result.RequisitionHistory,
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.ServiceRecord).ExtraFields)
            };
            return PartialView("_ServiceRecordModal", convertedResult);
        }
        private async Task<bool> DeleteServiceRecordWithChecks(int serviceRecordId)
        {
            var existingRecord = _serviceRecordDataAccess.GetServiceRecordById(serviceRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
            {
                return false;
            }
            //restore any requisitioned supplies.
            if (existingRecord.RequisitionHistory.Any())
            {
                _vehicleLogic.RestoreSupplyRecordsByUsage(existingRecord.RequisitionHistory, existingRecord.Description);
            }
            var result = _serviceRecordDataAccess.DeleteServiceRecordById(existingRecord.Id);
            if (result)
            {
                await _notificationService.NotifyAsync(WebHookPayload.FromGenericRecord(existingRecord, "servicerecord.delete", User.Identity.Name));
            }
            return result;
        }
        [HttpPost]
        public async Task<IActionResult> DeleteServiceRecordById(int serviceRecordId)
        {
            var result = await DeleteServiceRecordWithChecks(serviceRecordId);
            return Json(result);
        }
    }
}

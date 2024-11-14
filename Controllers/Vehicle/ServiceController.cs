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
        public IActionResult SaveServiceRecordToVehicleId(ServiceRecordInput serviceRecord)
        {
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
                serviceRecord.RequisitionHistory = RequisitionSupplyRecordsByUsage(serviceRecord.Supplies, DateTime.Parse(serviceRecord.Date), serviceRecord.Description);
                if (serviceRecord.CopySuppliesAttachment)
                {
                    serviceRecord.Files.AddRange(GetSuppliesAttachments(serviceRecord.Supplies));
                }
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
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), serviceRecord.VehicleId, User.Identity.Name, $"{(serviceRecord.Id == default ? "Created" : "Edited")} Service Record - Description: {serviceRecord.Description}");
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
        [HttpPost]
        public IActionResult DeleteServiceRecordById(int serviceRecordId)
        {
            var result = _serviceRecordDataAccess.DeleteServiceRecordById(serviceRecordId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Service Record - Id: {serviceRecordId}");
            }
            return Json(result);
        }
    }
}

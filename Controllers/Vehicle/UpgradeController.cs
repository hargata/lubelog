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
        public IActionResult GetUpgradeRecordsByVehicleId(int vehicleId)
        {
            var result = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
            bool _useDescending = _config.GetUserConfig(User).UseDescending;
            if (_useDescending)
            {
                result = result.OrderByDescending(x => x.Date).ThenByDescending(x => x.Mileage).ToList();
            }
            else
            {
                result = result.OrderBy(x => x.Date).ThenBy(x => x.Mileage).ToList();
            }
            return PartialView("Upgrade/_UpgradeRecords", result);
        }
        [HttpPost]
        public IActionResult SaveUpgradeRecordToVehicleId(UpgradeRecordInput upgradeRecord)
        {
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), upgradeRecord.VehicleId))
            {
                return Json(false);
            }
            if (upgradeRecord.Id == default && _config.GetUserConfig(User).EnableAutoOdometerInsert)
            {
                _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                {
                    Date = DateTime.Parse(upgradeRecord.Date),
                    VehicleId = upgradeRecord.VehicleId,
                    Mileage = upgradeRecord.Mileage,
                    Notes = $"Auto Insert From Upgrade Record: {upgradeRecord.Description}"
                });
            }
            //move files from temp.
            upgradeRecord.Files = upgradeRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            if (upgradeRecord.Supplies.Any())
            {
                upgradeRecord.RequisitionHistory.AddRange(RequisitionSupplyRecordsByUsage(upgradeRecord.Supplies, DateTime.Parse(upgradeRecord.Date), upgradeRecord.Description));
                if (upgradeRecord.CopySuppliesAttachment)
                {
                    upgradeRecord.Files.AddRange(GetSuppliesAttachments(upgradeRecord.Supplies));
                }
            }
            if (upgradeRecord.DeletedRequisitionHistory.Any())
            {
                _vehicleLogic.RestoreSupplyRecordsByUsage(upgradeRecord.DeletedRequisitionHistory, upgradeRecord.Description);
            }
            //push back any reminders
            if (upgradeRecord.ReminderRecordId.Any())
            {
                foreach (int reminderRecordId in upgradeRecord.ReminderRecordId)
                {
                    PushbackRecurringReminderRecordWithChecks(reminderRecordId, DateTime.Parse(upgradeRecord.Date), upgradeRecord.Mileage);
                }
            }
            var result = _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(upgradeRecord.ToUpgradeRecord());
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromGenericRecord(upgradeRecord.ToUpgradeRecord(), upgradeRecord.Id == default ? "upgraderecord.add" : "upgraderecord.update", User.Identity.Name));
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddUpgradeRecordPartialView()
        {
            return PartialView("Upgrade/_UpgradeRecordModal", new UpgradeRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.UpgradeRecord).ExtraFields });
        }
        [HttpGet]
        public IActionResult GetUpgradeRecordForEditById(int upgradeRecordId)
        {
            var result = _upgradeRecordDataAccess.GetUpgradeRecordById(upgradeRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), result.VehicleId))
            {
                return Redirect("/Error/Unauthorized");
            }
            //convert to Input object.
            var convertedResult = new UpgradeRecordInput
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
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.UpgradeRecord).ExtraFields)
            };
            return PartialView("Upgrade/_UpgradeRecordModal", convertedResult);
        }
        private bool DeleteUpgradeRecordWithChecks(int upgradeRecordId)
        {
            var existingRecord = _upgradeRecordDataAccess.GetUpgradeRecordById(upgradeRecordId);
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
            var result = _upgradeRecordDataAccess.DeleteUpgradeRecordById(existingRecord.Id);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromGenericRecord(existingRecord, "upgraderecord.delete", User.Identity.Name));
            }
            return result;
        }
        [HttpPost]
        public IActionResult DeleteUpgradeRecordById(int upgradeRecordId)
        {
            var result = DeleteUpgradeRecordWithChecks(upgradeRecordId);
            return Json(result);
        }
    }
}

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
            return PartialView("_UpgradeRecords", result);
        }
        [HttpPost]
        public IActionResult SaveUpgradeRecordToVehicleId(UpgradeRecordInput upgradeRecord)
        {
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
            if (upgradeRecord.Supplies.Count != 0)
            {
                upgradeRecord.RequisitionHistory = RequisitionSupplyRecordsByUsage(upgradeRecord.Supplies, DateTime.Parse(upgradeRecord.Date), upgradeRecord.Description);
                if (upgradeRecord.CopySuppliesAttachment)
                {
                    upgradeRecord.Files.AddRange(GetSuppliesAttachments(upgradeRecord.Supplies));
                }
            }
            //push back any reminders
            if (upgradeRecord.ReminderRecordId.Count != 0)
            {
                foreach (int reminderRecordId in upgradeRecord.ReminderRecordId)
                {
                    PushbackRecurringReminderRecordWithChecks(reminderRecordId, DateTime.Parse(upgradeRecord.Date), upgradeRecord.Mileage);
                }
            }
            var result = _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(upgradeRecord.ToUpgradeRecord());
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), upgradeRecord.VehicleId, User.Identity.Name, $"{(upgradeRecord.Id == default ? "Created" : "Edited")} Upgrade Record - Description: {upgradeRecord.Description}");
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddUpgradeRecordPartialView()
        {
            return PartialView("_UpgradeRecordModal", new UpgradeRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.UpgradeRecord).ExtraFields });
        }
        [HttpGet]
        public IActionResult GetUpgradeRecordForEditById(int upgradeRecordId)
        {
            var result = _upgradeRecordDataAccess.GetUpgradeRecordById(upgradeRecordId);
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
            return PartialView("_UpgradeRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeleteUpgradeRecordById(int upgradeRecordId)
        {
            var result = _upgradeRecordDataAccess.DeleteUpgradeRecordById(upgradeRecordId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Upgrade Record - Id: {upgradeRecordId}");
            }
            return Json(result);
        }
    }
}

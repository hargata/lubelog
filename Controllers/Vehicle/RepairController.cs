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
        public IActionResult GetCollisionRecordsByVehicleId(int vehicleId)
        {
            var result = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
            bool _useDescending = _config.GetUserConfig(User).UseDescending;
            if (_useDescending)
            {
                result = result.OrderByDescending(x => x.Date).ThenByDescending(x => x.Mileage).ToList();
            }
            else
            {
                result = result.OrderBy(x => x.Date).ThenBy(x => x.Mileage).ToList();
            }
            return PartialView("_CollisionRecords", result);
        }
        [HttpPost]
        public IActionResult SaveCollisionRecordToVehicleId(CollisionRecordInput collisionRecord)
        {
            if (collisionRecord.Id == default && _config.GetUserConfig(User).EnableAutoOdometerInsert)
            {
                _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                {
                    Date = DateTime.Parse(collisionRecord.Date),
                    VehicleId = collisionRecord.VehicleId,
                    Mileage = collisionRecord.Mileage,
                    Notes = $"Auto Insert From Repair Record: {collisionRecord.Description}"
                });
            }
            //move files from temp.
            collisionRecord.Files = collisionRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            if (collisionRecord.Supplies.Any())
            {
                collisionRecord.RequisitionHistory = RequisitionSupplyRecordsByUsage(collisionRecord.Supplies, DateTime.Parse(collisionRecord.Date), collisionRecord.Description);
                if (collisionRecord.CopySuppliesAttachment)
                {
                    collisionRecord.Files.AddRange(GetSuppliesAttachments(collisionRecord.Supplies));
                }
            }
            //push back any reminders
            if (collisionRecord.ReminderRecordId.Any())
            {
                foreach (int reminderRecordId in collisionRecord.ReminderRecordId)
                {
                    PushbackRecurringReminderRecordWithChecks(reminderRecordId, DateTime.Parse(collisionRecord.Date), collisionRecord.Mileage);
                }
            }
            var result = _collisionRecordDataAccess.SaveCollisionRecordToVehicle(collisionRecord.ToCollisionRecord());
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), collisionRecord.VehicleId, User.Identity.Name, $"{(collisionRecord.Id == default ? "Created" : "Edited")} Repair Record - Description: {collisionRecord.Description}");
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddCollisionRecordPartialView()
        {
            return PartialView("_CollisionRecordModal", new CollisionRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.RepairRecord).ExtraFields });
        }
        [HttpGet]
        public IActionResult GetCollisionRecordForEditById(int collisionRecordId)
        {
            var result = _collisionRecordDataAccess.GetCollisionRecordById(collisionRecordId);
            //convert to Input object.
            var convertedResult = new CollisionRecordInput
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
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.RepairRecord).ExtraFields)
            };
            return PartialView("_CollisionRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeleteCollisionRecordById(int collisionRecordId)
        {
            var result = _collisionRecordDataAccess.DeleteCollisionRecordById(collisionRecordId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Repair Record - Id: {collisionRecordId}");
            }
            return Json(result);
        }
    }
}

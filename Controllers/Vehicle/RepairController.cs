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
            return PartialView("Collision/_CollisionRecords", result);
        }
        [HttpPost]
        public IActionResult SaveCollisionRecordToVehicleId(CollisionRecordInput collisionRecord)
        {
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), collisionRecord.VehicleId))
            {
                return Json(false);
            }
            //move files from temp.
            collisionRecord.Files = collisionRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            if (collisionRecord.Supplies.Any())
            {
                collisionRecord.RequisitionHistory.AddRange(RequisitionSupplyRecordsByUsage(collisionRecord.Supplies, DateTime.Parse(collisionRecord.Date), collisionRecord.Description));
                if (collisionRecord.CopySuppliesAttachment)
                {
                    collisionRecord.Files.AddRange(GetSuppliesAttachments(collisionRecord.Supplies));
                }
            }
            if (collisionRecord.DeletedRequisitionHistory.Any())
            {
                _vehicleLogic.RestoreSupplyRecordsByUsage(collisionRecord.DeletedRequisitionHistory, collisionRecord.Description);
            }
            //push back any reminders
            if (collisionRecord.ReminderRecordId.Any())
            {
                foreach (int reminderRecordId in collisionRecord.ReminderRecordId)
                {
                    PushbackRecurringReminderRecordWithChecks(reminderRecordId, DateTime.Parse(collisionRecord.Date), collisionRecord.Mileage);
                }
            }
            var convertedRecord = collisionRecord.ToCollisionRecord();
            var result = _collisionRecordDataAccess.SaveCollisionRecordToVehicle(convertedRecord);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromGenericRecord(convertedRecord, collisionRecord.Id == default ? "repairrecord.add" : "repairrecord.update", User.Identity.Name));
            }
            if (convertedRecord.Id != default && collisionRecord.Id == default && _config.GetUserConfig(User).EnableAutoOdometerInsert)
            {
                _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                {
                    Date = DateTime.Parse(collisionRecord.Date),
                    VehicleId = collisionRecord.VehicleId,
                    Mileage = collisionRecord.Mileage,
                    Notes = $"Auto Insert From Repair Record: {collisionRecord.Description}",
                    Files = StaticHelper.CreateAttachmentFromRecord(ImportMode.RepairRecord, convertedRecord.Id, convertedRecord.Description)
                });
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddCollisionRecordPartialView()
        {
            return PartialView("Collision/_CollisionRecordModal", new CollisionRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.RepairRecord).ExtraFields });
        }
        [HttpGet]
        public IActionResult GetCollisionRecordForEditById(int collisionRecordId)
        {
            var result = _collisionRecordDataAccess.GetCollisionRecordById(collisionRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), result.VehicleId))
            {
                return Redirect("/Error/Unauthorized");
            }
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
            return PartialView("Collision/_CollisionRecordModal", convertedResult);
        }
        private bool DeleteCollisionRecordWithChecks(int collisionRecordId)
        {
            var existingRecord = _collisionRecordDataAccess.GetCollisionRecordById(collisionRecordId);
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
            var result = _collisionRecordDataAccess.DeleteCollisionRecordById(existingRecord.Id);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromGenericRecord(existingRecord, "repairrecord.delete", User.Identity.Name));
            }
            return result;
        }
        [HttpPost]
        public IActionResult DeleteCollisionRecordById(int collisionRecordId)
        {
            var result = DeleteCollisionRecordWithChecks(collisionRecordId);
            return Json(result);
        }
    }
}

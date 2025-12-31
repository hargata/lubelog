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
        public IActionResult GetEquipmentRecordsByVehicleId(int vehicleId)
        {
            var result = new List<EquipmentRecordViewModel>();
            var equipmentRecords = _equipmentRecordDataAccess.GetEquipmentRecordsByVehicleId(vehicleId);
            //convert to viewmodel and calculate sum of distance traveled
            var odometerRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
            foreach(EquipmentRecord equipmentRecord in equipmentRecords)
            {
                var viewModel = new EquipmentRecordViewModel
                {
                    Id = equipmentRecord.Id,
                    VehicleId = equipmentRecord.VehicleId,
                    Description = equipmentRecord.Description,
                    IsEquipped = equipmentRecord.IsEquipped,
                    Notes = equipmentRecord.Notes,
                    Tags = equipmentRecord.Tags,
                    Files = equipmentRecord.Files,
                    ExtraFields = equipmentRecord.ExtraFields,
                    DistanceTraveled = 0
                };
                var linkedOdometerRecords = odometerRecords.Where(x => x.EquipmentRecordId.Contains(equipmentRecord.Id));
                if (linkedOdometerRecords.Any())
                {
                    viewModel.DistanceTraveled = linkedOdometerRecords.Sum(x => x.DistanceTraveled);
                }
                result.Add(viewModel);
            }
            result = result.OrderByDescending(x => x.IsEquipped).ToList();
            return PartialView("Equipment/_EquipmentRecords", result);
        }
        [HttpPost]
        public IActionResult SaveEquipmentRecordToVehicleId(EquipmentRecordInput equipmentRecord)
        {
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), equipmentRecord.VehicleId, HouseholdPermission.Edit))
            {
                return Json(OperationResponse.Failed("Access Denied"));
            }
            //move files from temp.
            equipmentRecord.Files = equipmentRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var convertedRecord = equipmentRecord.ToEquipmentRecord();
            var result = _equipmentRecordDataAccess.SaveEquipmentRecordToVehicle(convertedRecord);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromEquipmentRecord(convertedRecord, equipmentRecord.Id == default ? "equipmentrecord.add" : "equipmentrecord.update", User.Identity.Name));
            }
            return Json(OperationResponse.Conditional(result, string.Empty, StaticHelper.GenericErrorMessage));
        }
        [HttpGet]
        public IActionResult GetAddEquipmentRecordPartialView()
        {
            return PartialView("Equipment/_EquipmentRecordModal", new EquipmentRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.EquipmentRecord).ExtraFields });
        }
        [HttpGet]
        public IActionResult GetEquipmentRecordForEditById(int equipmentRecordId)
        {
            var result = _equipmentRecordDataAccess.GetEquipmentRecordById(equipmentRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), result.VehicleId, HouseholdPermission.View))
            {
                return Redirect("/Error/Unauthorized");
            }
            //convert to Input object.
            var convertedResult = new EquipmentRecordInput
            {
                Id = result.Id,
                Description = result.Description,
                IsEquipped = result.IsEquipped,
                Notes = result.Notes,
                VehicleId = result.VehicleId,
                Files = result.Files,
                Tags = result.Tags,
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.EquipmentRecord).ExtraFields)
            };
            return PartialView("Equipment/_EquipmentRecordModal", convertedResult);
        }
        private OperationResponse DeleteEquipmentRecordWithChecks(int equipmentRecordId)
        {
            var existingRecord = _equipmentRecordDataAccess.GetEquipmentRecordById(equipmentRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Delete))
            {
                return OperationResponse.Failed("Access Denied");
            }
            //delete link to odometer record
            var odometerRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(existingRecord.VehicleId);
            var linkedOdometerRecords = odometerRecords.Where(x => x.EquipmentRecordId.Contains(equipmentRecordId));
            if (linkedOdometerRecords.Any())
            {
                foreach(OdometerRecord linkedOdometerRecord in linkedOdometerRecords)
                {
                    linkedOdometerRecord.EquipmentRecordId.RemoveAll(x=>x ==  equipmentRecordId);
                    _odometerRecordDataAccess.SaveOdometerRecordToVehicle(linkedOdometerRecord);
                }
            }
            var result = _equipmentRecordDataAccess.DeleteEquipmentRecordById(existingRecord.Id);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromEquipmentRecord(existingRecord, "equipmentrecord.delete", User.Identity.Name));
            }
            return OperationResponse.Conditional(result, string.Empty, StaticHelper.GenericErrorMessage);
        }
        [HttpPost]
        public IActionResult DeleteEquipmentRecordById(int collisionRecordId)
        {
            var result = DeleteEquipmentRecordWithChecks(collisionRecordId);
            return Json(result);
        }
    }
}

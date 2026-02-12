using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public partial class VehicleController
    {
        [HttpPost]
        public IActionResult ForceRecalculateDistanceByVehicleId(int vehicleId)
        {
            //security check
            if (!_userLogic.UserCanEditVehicle(GetUserID(), vehicleId, HouseholdPermission.Edit))
            {
                return Json(OperationResponse.Failed("Access Denied"));
            }
            var result = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
            result = _odometerLogic.AutoConvertOdometerRecord(result);
            return Json(OperationResponse.Conditional(result.Any(), string.Empty, StaticHelper.GenericErrorMessage));
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetOdometerRecordsByVehicleId(int vehicleId)
        {
            var result = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
            //determine if conversion is needed.
            if (result.All(x => x.InitialMileage == default))
            {
                result = _odometerLogic.AutoConvertOdometerRecord(result);
            }
            bool _useDescending = _config.GetUserConfig(User).UseDescending;
            if (_useDescending)
            {
                result = result.OrderByDescending(x => x.Date).ThenByDescending(x => x.Mileage).ToList();
            }
            else
            {
                result = result.OrderBy(x => x.Date).ThenBy(x => x.Mileage).ToList();
            }
            return PartialView("Odometer/_OdometerRecords", result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetPaginatedOdometerRecordsByVehicleId(int vehicleId, int pageSize, int page)
        {
            var result = new List<OdometerRecord>();
            //determine if conversion is needed.
            
            bool _useDescending = _config.GetUserConfig(User).UseDescending;

            if (_useDescending) 
            {
                result = _odometerRecordDataAccess.GetPaginatedOdometerRecordsByVehicleId(vehicleId, pageSize, page, SortDirection.Descending);
            }
            else
            {
                result = _odometerRecordDataAccess.GetPaginatedOdometerRecordsByVehicleId(vehicleId, pageSize, page, SortDirection.Ascending);
            }

            if (result.All(x => x.InitialMileage == default))
            {
                result = _odometerLogic.AutoConvertOdometerRecord(result);
            }
            return PartialView("Odometer/_OdometerRecords", result);
        }
        [HttpPost]
        public IActionResult SaveOdometerRecordToVehicleId(OdometerRecordInput odometerRecord)
        {
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), odometerRecord.VehicleId, HouseholdPermission.Edit))
            {
                return Json(OperationResponse.Failed("Access Denied"));
            }
            //move files from temp.
            odometerRecord.Files = odometerRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var convertedRecord = odometerRecord.ToOdometerRecord();
            var result = _odometerRecordDataAccess.SaveOdometerRecordToVehicle(convertedRecord);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromOdometerRecord(convertedRecord, odometerRecord.Id == default ? "odometerrecord.add" : "odometerrecord.update", User.Identity?.Name ?? string.Empty));
            }
            return Json(OperationResponse.Conditional(result, string.Empty, StaticHelper.GenericErrorMessage));
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetAddOdometerRecordPartialView(int vehicleId)
        {
            return PartialView("Odometer/_OdometerRecordModal", new OdometerRecordInput() { 
                InitialMileage = _odometerLogic.GetLastOdometerRecordMileage(vehicleId, new List<OdometerRecord>()), 
                ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.OdometerRecord).ExtraFields,
                EquipmentRecords = _equipmentRecordDataAccess.GetEquipmentRecordsByVehicleId(vehicleId).OrderByDescending(x => x.IsEquipped).ThenBy(x => x.Description).ToList()
            });
        }
        [HttpPost]
        public IActionResult GetOdometerRecordsEditModal(List<int> recordIds, int vehicleId)
        {
            var extraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.OdometerRecord).ExtraFields;
            var equipmentRecords = _equipmentRecordDataAccess.GetEquipmentRecordsByVehicleId(vehicleId).OrderByDescending(x => x.IsEquipped).ThenBy(x => x.Description).ToList();
            foreach(EquipmentRecord equipmentRecord in equipmentRecords)
            {
                equipmentRecord.IsEquipped = false;
            }
            return PartialView("Odometer/_OdometerRecordsModal", new OdometerRecordEditModel { 
                RecordIds = recordIds, 
                EditRecord = new OdometerRecord { ExtraFields = extraFields },
                EquipmentRecords = equipmentRecords
            });
        }
        [HttpPost]
        public IActionResult SaveMultipleOdometerRecords(OdometerRecordEditModel editModel)
        {
            var dateIsEdited = editModel.EditRecord.Date != default;
            var initialMileageIsEdited = editModel.EditRecord.InitialMileage != default;
            var mileageIsEdited = editModel.EditRecord.Mileage != default;
            var noteIsEdited = !string.IsNullOrWhiteSpace(editModel.EditRecord.Notes);
            var tagsIsEdited = editModel.EditRecord.Tags.Any();
            var extraFieldIsEdited = editModel.EditRecord.ExtraFields.Any();
            var equipmentIsEdited = editModel.EditEquipment;
            //handle clear overrides
            if (tagsIsEdited && editModel.EditRecord.Tags.Contains("---"))
            {
                editModel.EditRecord.Tags = new List<string>();
            }
            if (noteIsEdited && editModel.EditRecord.Notes == "---")
            {
                editModel.EditRecord.Notes = "";
            }
            bool result = false;
            foreach (int recordId in editModel.RecordIds)
            {
                var existingRecord = _odometerRecordDataAccess.GetOdometerRecordById(recordId);
                //security check
                if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Edit))
                {
                    return Json(OperationResponse.Failed("Access Denied"));
                }
                if (dateIsEdited)
                {
                    existingRecord.Date = editModel.EditRecord.Date;
                }
                if (initialMileageIsEdited)
                {
                    existingRecord.InitialMileage = editModel.EditRecord.InitialMileage;
                }
                if (mileageIsEdited)
                {
                    existingRecord.Mileage = editModel.EditRecord.Mileage;
                }
                if (noteIsEdited)
                {
                    existingRecord.Notes = editModel.EditRecord.Notes;
                }
                if (tagsIsEdited)
                {
                    existingRecord.Tags = editModel.EditRecord.Tags;
                }
                if (extraFieldIsEdited)
                {
                    foreach (ExtraField extraField in editModel.EditRecord.ExtraFields)
                    {
                        if (existingRecord.ExtraFields.Any(x => x.Name == extraField.Name))
                        {
                            var insertIndex = existingRecord.ExtraFields.FindIndex(x => x.Name == extraField.Name);
                            existingRecord.ExtraFields.RemoveAll(x => x.Name == extraField.Name);
                            existingRecord.ExtraFields.Insert(insertIndex, extraField);
                        }
                        else
                        {
                            existingRecord.ExtraFields.Add(extraField);
                        }
                    }
                }
                if (equipmentIsEdited)
                {
                    existingRecord.EquipmentRecordId = editModel.EditRecord.EquipmentRecordId;
                }
                result = _odometerRecordDataAccess.SaveOdometerRecordToVehicle(existingRecord);
            }
            return Json(OperationResponse.Conditional(result, string.Empty, StaticHelper.GenericErrorMessage));
        }
        [HttpGet]
        public IActionResult GetOdometerRecordForEditById(int odometerRecordId)
        {
            var result = _odometerRecordDataAccess.GetOdometerRecordById(odometerRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), result.VehicleId, HouseholdPermission.View))
            {
                return Redirect("/Error/Unauthorized");
            }
            //check for equipment
            var equipmentRecords = _equipmentRecordDataAccess.GetEquipmentRecordsByVehicleId(result.VehicleId).OrderByDescending(x => x.IsEquipped).ThenBy(x => x.Description).ToList();
            foreach(EquipmentRecord equipmentRecord in equipmentRecords)
            {
                equipmentRecord.IsEquipped = result.EquipmentRecordId.Contains(equipmentRecord.Id);
            }
            //convert to Input object.
            var convertedResult = new OdometerRecordInput
            {
                Id = result.Id,
                Date = result.Date.ToShortDateString(),
                InitialMileage = result.InitialMileage,
                Mileage = result.Mileage,
                Notes = result.Notes,
                VehicleId = result.VehicleId,
                Files = result.Files,
                Tags = result.Tags,
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.OdometerRecord).ExtraFields),
                EquipmentRecords = equipmentRecords
            };
            return PartialView("Odometer/_OdometerRecordModal", convertedResult);
        }
        private OperationResponse DeleteOdometerRecordWithChecks(int odometerRecordId)
        {
            var existingRecord = _odometerRecordDataAccess.GetOdometerRecordById(odometerRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Delete))
            {
                return OperationResponse.Failed("Access Denied");
            }
            var result = _odometerRecordDataAccess.DeleteOdometerRecordById(existingRecord.Id);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromOdometerRecord(existingRecord, "odometerrecord.delete", User.Identity?.Name ?? string.Empty));
            }
            return OperationResponse.Conditional(result, string.Empty, StaticHelper.GenericErrorMessage);
        }
        [HttpPost]
        public IActionResult DeleteOdometerRecordById(int odometerRecordId)
        {
            var result = DeleteOdometerRecordWithChecks(odometerRecordId);
            return Json(result);
        }
        [HttpPost]
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { true, true, HouseholdPermission.Edit })]
        public IActionResult DuplicateDistanceToOtherVehicles(List<int> recordIds, List<int> vehicleIds, bool shiftOdometer)
        {
            bool result = false;
            if (!recordIds.Any() || !vehicleIds.Any())
            {
                return Json(result);
            }
            List<OdometerRecord> odometerRecords = new List<OdometerRecord>();
            foreach (int recordId in recordIds)
            {
                odometerRecords.Add(_odometerRecordDataAccess.GetOdometerRecordById(recordId));
            }
            int totalDistance = odometerRecords.Sum(x => x.DistanceTraveled);
            DateTime lastDate = odometerRecords.Max(x => x.Date);
            foreach (int vehicleId in vehicleIds)
            {
                var currentOdometer = 0;
                //get closest odometer record to the last date
                var targetVehicleOdometerRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
                var odometerRecordsBefore = targetVehicleOdometerRecords.Where(x=>x.Date <= lastDate);
                if (odometerRecordsBefore.Any())
                {
                    currentOdometer = odometerRecordsBefore.Max(x => x.Mileage);
                }
                var newOdometer = currentOdometer + totalDistance;
                result = _odometerRecordDataAccess.SaveOdometerRecordToVehicle(new OdometerRecord
                {
                    VehicleId = vehicleId,
                    Date = lastDate,
                    InitialMileage = currentOdometer,
                    Mileage = newOdometer,
                    Notes = "Auto Insert From Distance Export."
                });
                if (shiftOdometer)
                {
                    var odometerRecordsAfter = targetVehicleOdometerRecords.Where(x => x.Date > lastDate);
                    //get any odometer records to shift
                    if (odometerRecordsAfter.Any())
                    {
                        //shift these odometer records
                        foreach (OdometerRecord odometerRecordToShift in odometerRecordsAfter)
                        {
                            odometerRecordToShift.Mileage += totalDistance;
                            _odometerRecordDataAccess.SaveOdometerRecordToVehicle(odometerRecordToShift);
                        }
                        //auto recalculate distance
                        var newTargetVehicleOdometerRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
                        _odometerLogic.AutoConvertOdometerRecord(newTargetVehicleOdometerRecords);
                    }
                }
            }
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.Generic($"Duplicated distance from OdometerRecord - Ids: {string.Join(",", recordIds)} - to Vehicle Ids: {string.Join(",", vehicleIds)}", "bulk.duplicate.distance.to.vehicles", User.Identity?.Name ?? string.Empty, string.Join(",", vehicleIds)));
            }
            return Json(OperationResponse.Conditional(result, string.Empty, StaticHelper.GenericErrorMessage));
        }
    }
}

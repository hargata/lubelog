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
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromOdometerRecord(convertedRecord, odometerRecord.Id == default ? "odometerrecord.add" : "odometerrecord.update", User.Identity.Name));
            }
            return Json(OperationResponse.Conditional(result, string.Empty, StaticHelper.GenericErrorMessage));
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetAddOdometerRecordPartialView(int vehicleId)
        {
            return PartialView("Odometer/_OdometerRecordModal", new OdometerRecordInput() { InitialMileage = _odometerLogic.GetLastOdometerRecordMileage(vehicleId, new List<OdometerRecord>()), ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.OdometerRecord).ExtraFields });
        }
        [HttpPost]
        public IActionResult GetOdometerRecordsEditModal(List<int> recordIds)
        {
            var extraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.OdometerRecord).ExtraFields;
            return PartialView("Odometer/_OdometerRecordsModal", new OdometerRecordEditModel { RecordIds = recordIds, EditRecord = new OdometerRecord { ExtraFields = extraFields } });
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
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.OdometerRecord).ExtraFields)
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
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromOdometerRecord(existingRecord, "odometerrecord.delete", User.Identity.Name));
            }
            return OperationResponse.Conditional(result, string.Empty, StaticHelper.GenericErrorMessage);
        }
        [HttpPost]
        public IActionResult DeleteOdometerRecordById(int odometerRecordId)
        {
            var result = DeleteOdometerRecordWithChecks(odometerRecordId);
            return Json(result);
        }
    }
}

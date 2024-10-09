using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public partial class VehicleController
    {
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        public IActionResult ForceRecalculateDistanceByVehicleId(int vehicleId)
        {
            var result = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
            result = _odometerLogic.AutoConvertOdometerRecord(result);
            return Json(result.Any());
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
            return PartialView("_OdometerRecords", result);
        }
        [HttpPost]
        public IActionResult SaveOdometerRecordToVehicleId(OdometerRecordInput odometerRecord)
        {
            //move files from temp.
            odometerRecord.Files = odometerRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var result = _odometerRecordDataAccess.SaveOdometerRecordToVehicle(odometerRecord.ToOdometerRecord());
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), odometerRecord.VehicleId, User.Identity.Name, $"{(odometerRecord.Id == default ? "Created" : "Edited")} Odometer Record - Mileage: {odometerRecord.Mileage.ToString()}");
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddOdometerRecordPartialView(int vehicleId)
        {
            return PartialView("_OdometerRecordModal", new OdometerRecordInput() { InitialMileage = _odometerLogic.GetLastOdometerRecordMileage(vehicleId, new List<OdometerRecord>()), ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.OdometerRecord).ExtraFields });
        }
        [HttpPost]
        public IActionResult GetOdometerRecordsEditModal(List<int> recordIds)
        {
            return PartialView("_OdometerRecordsModal", new OdometerRecordEditModel { RecordIds = recordIds });
        }
        [HttpPost]
        public IActionResult SaveMultipleOdometerRecords(OdometerRecordEditModel editModel)
        {
            var dateIsEdited = editModel.EditRecord.Date != default;
            var initialMileageIsEdited = editModel.EditRecord.InitialMileage != default;
            var mileageIsEdited = editModel.EditRecord.Mileage != default;
            var noteIsEdited = !string.IsNullOrWhiteSpace(editModel.EditRecord.Notes);
            var tagsIsEdited = editModel.EditRecord.Tags.Any();
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
                result = _odometerRecordDataAccess.SaveOdometerRecordToVehicle(existingRecord);
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetOdometerRecordForEditById(int odometerRecordId)
        {
            var result = _odometerRecordDataAccess.GetOdometerRecordById(odometerRecordId);
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
            return PartialView("_OdometerRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeleteOdometerRecordById(int odometerRecordId)
        {
            var result = _odometerRecordDataAccess.DeleteOdometerRecordById(odometerRecordId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Odometer Record - Id: {odometerRecordId}");
            }
            return Json(result);
        }
    }
}

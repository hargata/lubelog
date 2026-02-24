using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public partial class APIController
    {
        [HttpGet]
        [Route("/api/vehicle/equipmentrecords/all")]
        public IActionResult AllEquipmentRecords(MethodParameter parameters)
        {
            List<int> vehicleIds = new List<int>();
            var vehicles = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehicles = _userLogic.FilterUserVehicles(vehicles, GetUserID());
            }
            vehicleIds.AddRange(vehicles.Select(x => x.Id));
            List<EquipmentRecordViewModel> vehicleRecords = new List<EquipmentRecordViewModel>();
            foreach (int vehicleId in vehicleIds)
            {
                var equipmentRecords = _equipmentRecordDataAccess.GetEquipmentRecordsByVehicleId(vehicleId);
                var odometerRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
                var convertedRecords = _equipmentHelper.GetEquipmentRecordViewModels(equipmentRecords, odometerRecords);
                vehicleRecords.AddRange(convertedRecords);
            }
            if (parameters.Id != default)
            {
                vehicleRecords.RemoveAll(x => x.Id != parameters.Id);
            }
            if (!string.IsNullOrWhiteSpace(parameters.Tags))
            {
                var tagsFilter = parameters.Tags.Split(' ').Distinct();
                vehicleRecords.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
            }
            var result = vehicleRecords.Select(x => new EquipmentRecordAPIExportModel { VehicleId = x.VehicleId.ToString(), Id = x.Id.ToString(), Description = x.Description, IsEquipped = x.IsEquipped.ToString(), DistanceTraveled = x.DistanceTraveled.ToString(), Notes = x.Notes, ExtraFields = x.ExtraFields, Files = x.Files, Tags = string.Join(' ', x.Tags) });
            if (_config.GetInvariantApi() || Request.Headers.ContainsKey("culture-invariant"))
            {
                return Json(result, StaticHelper.GetInvariantOption());
            }
            else
            {
                return Json(result);
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/equipmentrecords")]
        public IActionResult EquipmentRecords(int vehicleId, MethodParameter parameters)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _equipmentRecordDataAccess.GetEquipmentRecordsByVehicleId(vehicleId);
            if (parameters.Id != default)
            {
                vehicleRecords.RemoveAll(x => x.Id != parameters.Id);
            }
            if (!string.IsNullOrWhiteSpace(parameters.Tags))
            {
                var tagsFilter = parameters.Tags.Split(' ').Distinct();
                vehicleRecords.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
            }
            var odometerRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
            var convertedRecords = _equipmentHelper.GetEquipmentRecordViewModels(vehicleRecords, odometerRecords);
            var result = convertedRecords.Select(x => new EquipmentRecordAPIExportModel { VehicleId = x.VehicleId.ToString(), Id = x.Id.ToString(), Description = x.Description, IsEquipped = x.IsEquipped.ToString(), DistanceTraveled = x.DistanceTraveled.ToString(), Notes = x.Notes, ExtraFields = x.ExtraFields, Files = x.Files, Tags = string.Join(' ', x.Tags) });
            if (_config.GetInvariantApi() || Request.Headers.ContainsKey("culture-invariant"))
            {
                return Json(result, StaticHelper.GetInvariantOption());
            }
            else
            {
                return Json(result);
            }
        }
        [TypeFilter(typeof(QueryParamFilter), Arguments = new object[] { new string[] { "vehicleId" } })]
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { false, true, HouseholdPermission.Edit })]
        [HttpPost]
        [Route("/api/vehicle/equipmentrecords/add")]
        [Consumes("application/json")]
        public IActionResult AddEquipmentRecordJson(int vehicleId, [FromBody] EquipmentRecordExportModel input) => AddEquipmentRecord(vehicleId, input);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { false, true, HouseholdPermission.Edit })]
        [HttpPost]
        [Route("/api/vehicle/equipmentrecords/add")]
        public IActionResult AddEquipmentRecord(int vehicleId, EquipmentRecordExportModel input)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            if (string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.IsEquipped))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Description and IsEquipped cannot be empty."));
            }
            if (input.Files == null)
            {
                input.Files = new List<UploadedFiles>();
            }
            if (input.ExtraFields == null)
            {
                input.ExtraFields = new List<ExtraField>();
            }
            try
            {
                var equipmentRecord = new EquipmentRecord()
                {
                    VehicleId = vehicleId,
                    Description = input.Description,
                    IsEquipped = bool.Parse(input.IsEquipped),
                    Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                    ExtraFields = input.ExtraFields,
                    Files = input.Files,
                    Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList()
                };
                _equipmentRecordDataAccess.SaveEquipmentRecordToVehicle(equipmentRecord);
                _eventLogic.PublishEvent(GetUserID(), WebHookPayload.FromEquipmentRecord(equipmentRecord, "equipmentrecord.add.api", User.Identity?.Name ?? string.Empty));
                return Json(OperationResponse.Succeed("Equipment Record Added", new { recordId = equipmentRecord.Id }));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Delete })]
        [HttpDelete]
        [Route("/api/vehicle/equipmentrecords/delete")]
        public IActionResult DeleteEquipmentRecord(int id)
        {
            var existingRecord = _equipmentRecordDataAccess.GetEquipmentRecordById(id);
            if (existingRecord == null || existingRecord.Id == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Invalid Record Id"));
            }
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Delete))
            {
                Response.StatusCode = 401;
                return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
            }
            //delete link to odometer record
            var odometerRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(existingRecord.VehicleId);
            var linkedOdometerRecords = odometerRecords.Where(x => x.EquipmentRecordId.Contains(existingRecord.Id));
            if (linkedOdometerRecords.Any())
            {
                foreach (OdometerRecord linkedOdometerRecord in linkedOdometerRecords)
                {
                    linkedOdometerRecord.EquipmentRecordId.RemoveAll(x => x == existingRecord.Id);
                    _odometerRecordDataAccess.SaveOdometerRecordToVehicle(linkedOdometerRecord);
                }
            }
            var result = _equipmentRecordDataAccess.DeleteEquipmentRecordById(existingRecord.Id);
            if (result)
            {
                _eventLogic.PublishEvent(GetUserID(), WebHookPayload.FromEquipmentRecord(existingRecord, "equipmentrecord.delete.api", User.Identity?.Name ?? string.Empty));
            }
            return Json(OperationResponse.Conditional(result, "Equipment Record Deleted"));
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/equipmentrecords/update")]
        [Consumes("application/json")]
        public IActionResult UpdateEquipmentRecordJson([FromBody] EquipmentRecordExportModel input) => UpdateEquipmentRecord(input);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/equipmentrecords/update")]
        public IActionResult UpdateEquipmentRecord(EquipmentRecordExportModel input)
        {
            if (string.IsNullOrWhiteSpace(input.Id) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.IsEquipped))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Id, Description, and IsEquipped cannot be empty."));
            }
            if (input.Files == null)
            {
                input.Files = new List<UploadedFiles>();
            }
            if (input.ExtraFields == null)
            {
                input.ExtraFields = new List<ExtraField>();
            }
            try
            {
                //retrieve existing record
                var existingRecord = _equipmentRecordDataAccess.GetEquipmentRecordById(int.Parse(input.Id));
                if (existingRecord != null && existingRecord.Id == int.Parse(input.Id))
                {
                    //check if user has access to the vehicleId
                    if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Edit))
                    {
                        Response.StatusCode = 401;
                        return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
                    }
                    existingRecord.Description = input.Description;
                    existingRecord.IsEquipped = bool.Parse(input.IsEquipped);
                    existingRecord.Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes;
                    existingRecord.ExtraFields = input.ExtraFields;
                    existingRecord.Files = input.Files;
                    existingRecord.Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList();
                    _equipmentRecordDataAccess.SaveEquipmentRecordToVehicle(existingRecord);
                    _eventLogic.PublishEvent(GetUserID(), WebHookPayload.FromEquipmentRecord(existingRecord, "equipmentrecord.update.api", User.Identity?.Name ?? string.Empty));
                }
                else
                {
                    Response.StatusCode = 400;
                    return Json(OperationResponse.Failed("Invalid Record Id"));
                }
                return Json(OperationResponse.Succeed("Equipment Record Updated"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
    }
}

using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public partial class APIController
    {
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/odometerrecords/latest")]
        public IActionResult LastOdometer(int vehicleId)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var result = _vehicleLogic.GetMaxMileage(vehicleId);
            return Json(result);
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { false, true, HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/odometerrecords/recalculate")]
        public IActionResult RecalculateDistance(int vehicleId)
        {
            var result = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
            result = _odometerLogic.AutoConvertOdometerRecord(result);
            if (result.Any())
            {
                return Json(OperationResponse.Succeed($"Odometer Records Adjusted({result.Count()})"));
            } else
            {
                return Json(OperationResponse.Failed());
            }
        }
        [HttpGet]
        [Route("/api/vehicle/odometerrecords/all")]
        public IActionResult AllOdometerRecords(MethodParameter parameters)
        {
            List<int> vehicleIds = new List<int>();
            var vehicles = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehicles = _userLogic.FilterUserVehicles(vehicles, GetUserID());
            }
            vehicleIds.AddRange(vehicles.Select(x => x.Id));
            List<OdometerRecord> vehicleRecords = new List<OdometerRecord>();
            foreach (int vehicleId in vehicleIds)
            {
                vehicleRecords.AddRange(_odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId));
            }
            if (parameters.Id != default)
            {
                vehicleRecords.RemoveAll(x => x.Id != parameters.Id);
            }
            if (!string.IsNullOrWhiteSpace(parameters.StartDate) && DateTime.TryParse(parameters.StartDate, out DateTime startDate))
            {
                vehicleRecords.RemoveAll(x => x.Date < startDate);
            }
            if (!string.IsNullOrWhiteSpace(parameters.EndDate) && DateTime.TryParse(parameters.EndDate, out DateTime endDate))
            {
                vehicleRecords.RemoveAll(x => x.Date > endDate);
            }
            if (!string.IsNullOrWhiteSpace(parameters.Tags))
            {
                var tagsFilter = parameters.Tags.Split(' ').Distinct();
                vehicleRecords.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
            }
            var result = vehicleRecords.Select(x => new OdometerRecordExportModel { VehicleId = x.VehicleId.ToString(), Id = x.Id.ToString(), Date = x.Date.ToShortDateString(), InitialOdometer = x.InitialMileage.ToString(), Odometer = x.Mileage.ToString(), Notes = x.Notes, ExtraFields = x.ExtraFields, Files = x.Files, Tags = string.Join(' ', x.Tags), EquipmentRecordId = string.Join(' ', x.EquipmentRecordId) });
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
        [Route("/api/vehicle/odometerrecords")]
        public IActionResult OdometerRecords(int vehicleId, MethodParameter parameters)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
            //determine if conversion is needed.
            if (vehicleRecords.All(x => x.InitialMileage == default))
            {
                vehicleRecords = _odometerLogic.AutoConvertOdometerRecord(vehicleRecords);
            }
            if (parameters.Id != default)
            {
                vehicleRecords.RemoveAll(x => x.Id != parameters.Id);
            }
            if (!string.IsNullOrWhiteSpace(parameters.StartDate) && DateTime.TryParse(parameters.StartDate, out DateTime startDate))
            {
                vehicleRecords.RemoveAll(x => x.Date < startDate);
            }
            if (!string.IsNullOrWhiteSpace(parameters.EndDate) && DateTime.TryParse(parameters.EndDate, out DateTime endDate))
            {
                vehicleRecords.RemoveAll(x => x.Date > endDate);
            }
            if (!string.IsNullOrWhiteSpace(parameters.Tags))
            {
                var tagsFilter = parameters.Tags.Split(' ').Distinct();
                vehicleRecords.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
            }
            var result = vehicleRecords.Select(x => new OdometerRecordExportModel { VehicleId = x.VehicleId.ToString(), Id = x.Id.ToString(), Date = x.Date.ToShortDateString(), InitialOdometer = x.InitialMileage.ToString(), Odometer = x.Mileage.ToString(), Notes = x.Notes, ExtraFields = x.ExtraFields, Files = x.Files, Tags = string.Join(' ', x.Tags), EquipmentRecordId = string.Join(' ', x.EquipmentRecordId) });
            if (_config.GetInvariantApi() || Request.Headers.ContainsKey("culture-invariant"))
            {
                return Json(result, StaticHelper.GetInvariantOption());
            }
            else
            {
                return Json(result);
            }
        }
        [TypeFilter(typeof(QueryParamFilter), Arguments = new object[] { new string[] { "vehicleId", "autoIncludeEquipment" } })]
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { false, true, HouseholdPermission.Edit })]
        [HttpPost]
        [Route("/api/vehicle/odometerrecords/add")]
        [Consumes("application/json")]
        public IActionResult AddOdometerRecordJson(int vehicleId, [FromBody] OdometerRecordExportModel input, bool autoIncludeEquipment = false) => AddOdometerRecord(vehicleId, input, autoIncludeEquipment);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { false, true, HouseholdPermission.Edit })]
        [HttpPost]
        [Route("/api/vehicle/odometerrecords/add")]
        public IActionResult AddOdometerRecord(int vehicleId, OdometerRecordExportModel input, bool autoIncludeEquipment = false)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            if (string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Odometer))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Date, and Odometer cannot be empty."));
            }
            if (input.Files == null)
            {
                input.Files = new List<UploadedFiles>();
            }
            if (input.ExtraFields == null)
            {
                input.ExtraFields = new List<ExtraField>();
            }
            var equipmentRecordId = new List<int>();
            //validate equipment record ids
            if (!string.IsNullOrWhiteSpace(input.EquipmentRecordId))
            {
                var equipmentRecords = _equipmentRecordDataAccess.GetEquipmentRecordsByVehicleId(vehicleId);
                if (!equipmentRecords.Any())
                {
                    Response.StatusCode = 400;
                    return Json(OperationResponse.Failed($"Input object invalid, equipment with ids {input.EquipmentRecordId} does not exist."));
                }
                var equipmentRecordIds = input.EquipmentRecordId.Split(' ').Distinct().ToList();
                foreach (string equipmentRecordIdToAdd in equipmentRecordIds)
                {
                    if (int.TryParse(equipmentRecordIdToAdd, out int cleanEquipmentRecordId))
                    {
                        equipmentRecordId.Add(cleanEquipmentRecordId);
                    }
                    else
                    {
                        Response.StatusCode = 400;
                        return Json(OperationResponse.Failed($"Input object invalid, equipment id {cleanEquipmentRecordId} is not valid."));
                    }
                }
                var equipmentRecordIdsToCompare = equipmentRecords.Select(x => x.Id);
                var invalidEquipmentRecordIds = equipmentRecordId.Where(x => !equipmentRecordIdsToCompare.Contains(x));
                if (invalidEquipmentRecordIds.Any())
                {
                    Response.StatusCode = 400;
                    return Json(OperationResponse.Failed($"Input object invalid, equipment with ids {string.Join(' ', invalidEquipmentRecordIds)} does not exist."));
                }
            }
            // Auto include equipment marked as currently equipped for vehicle (merges with any explicit IDs)
            if (autoIncludeEquipment)
            {
                var equipmentRecords = _equipmentRecordDataAccess.GetEquipmentRecordsByVehicleId(vehicleId);
                var equippedEquipment = equipmentRecords.Where(x => x.IsEquipped);
                if (equippedEquipment.Any())
                {
                    equipmentRecordId.AddRange(equippedEquipment.Select(x => x.Id));
                    equipmentRecordId = equipmentRecordId.Distinct().ToList();
                }
            }
            try
            {
                var odometerRecord = new OdometerRecord()
                {
                    VehicleId = vehicleId,
                    Date = DateTime.Parse(input.Date),
                    Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                    InitialMileage = (string.IsNullOrWhiteSpace(input.InitialOdometer) || int.Parse(input.InitialOdometer) == default) ? _odometerLogic.GetLastOdometerRecordMileage(vehicleId, new List<OdometerRecord>()) : int.Parse(input.InitialOdometer),
                    Mileage = int.Parse(input.Odometer),
                    ExtraFields = input.ExtraFields,
                    Files = input.Files,
                    Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList(),
                    EquipmentRecordId = equipmentRecordId
                };
                _odometerRecordDataAccess.SaveOdometerRecordToVehicle(odometerRecord);
                _eventLogic.PublishEvent(GetUserID(), WebHookPayload.FromOdometerRecord(odometerRecord, "odometerrecord.add.api", User.Identity?.Name ?? string.Empty));
                return Json(OperationResponse.Succeed("Odometer Record Added", new { recordId = odometerRecord.Id }));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Delete })]
        [HttpDelete]
        [Route("/api/vehicle/odometerrecords/delete")]
        public IActionResult DeleteOdometerRecord(int id)
        {
            var existingRecord = _odometerRecordDataAccess.GetOdometerRecordById(id);
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
            var result = _odometerRecordDataAccess.DeleteOdometerRecordById(existingRecord.Id);
            if (result)
            {
                _eventLogic.PublishEvent(GetUserID(), WebHookPayload.FromOdometerRecord(existingRecord, "odometerrecord.delete.api", User.Identity?.Name ?? string.Empty));
            }
            return Json(OperationResponse.Conditional(result, "Odometer Record Deleted"));
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/odometerrecords/update")]
        [Consumes("application/json")]
        public IActionResult UpdateOdometerRecordJson([FromBody] OdometerRecordExportModel input) => UpdateOdometerRecord(input);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/odometerrecords/update")]
        public IActionResult UpdateOdometerRecord(OdometerRecordExportModel input)
        {
            if (string.IsNullOrWhiteSpace(input.Id) ||
                string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.InitialOdometer) ||
                string.IsNullOrWhiteSpace(input.Odometer))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Id, Date, Initial Odometer, and Odometer cannot be empty."));
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
                var existingRecord = _odometerRecordDataAccess.GetOdometerRecordById(int.Parse(input.Id));
                if (existingRecord != null && existingRecord.Id == int.Parse(input.Id))
                {
                    //check if user has access to the vehicleId
                    if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Edit))
                    {
                        Response.StatusCode = 401;
                        return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
                    }
                    var equipmentRecordId = new List<int>();
                    //validate equipment record ids
                    if (!string.IsNullOrWhiteSpace(input.EquipmentRecordId))
                    {
                        var equipmentRecords = _equipmentRecordDataAccess.GetEquipmentRecordsByVehicleId(existingRecord.VehicleId);
                        if (!equipmentRecords.Any())
                        {
                            Response.StatusCode = 400;
                            return Json(OperationResponse.Failed($"Input object invalid, equipment with ids {input.EquipmentRecordId} does not exist."));
                        }
                        var equipmentRecordIds = input.EquipmentRecordId.Split(' ').Distinct().ToList();
                        foreach (string equipmentRecordIdToAdd in equipmentRecordIds)
                        {
                            if (int.TryParse(equipmentRecordIdToAdd, out int cleanEquipmentRecordId))
                            {
                                equipmentRecordId.Add(cleanEquipmentRecordId);
                            }
                            else
                            {
                                Response.StatusCode = 400;
                                return Json(OperationResponse.Failed($"Input object invalid, equipment id {cleanEquipmentRecordId} is not valid."));
                            }
                        }
                        var equipmentRecordIdsToCompare = equipmentRecords.Select(x => x.Id);
                        var invalidEquipmentRecordIds = equipmentRecordId.Where(x => !equipmentRecordIdsToCompare.Contains(x));
                        if (invalidEquipmentRecordIds.Any())
                        {
                            Response.StatusCode = 400;
                            return Json(OperationResponse.Failed($"Input object invalid, equipment with ids {string.Join(' ', invalidEquipmentRecordIds)} does not exist."));
                        }
                    }
                    existingRecord.Date = DateTime.Parse(input.Date);
                    existingRecord.Mileage = int.Parse(input.Odometer);
                    existingRecord.InitialMileage = int.Parse(input.InitialOdometer);
                    existingRecord.Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes;
                    existingRecord.ExtraFields = input.ExtraFields;
                    existingRecord.Files = input.Files;
                    existingRecord.Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList();
                    existingRecord.EquipmentRecordId = equipmentRecordId;
                    _odometerRecordDataAccess.SaveOdometerRecordToVehicle(existingRecord);
                    _eventLogic.PublishEvent(GetUserID(), WebHookPayload.FromOdometerRecord(existingRecord, "odometerrecord.update.api", User.Identity?.Name ?? string.Empty));
                }
                else
                {
                    Response.StatusCode = 400;
                    return Json(OperationResponse.Failed("Invalid Record Id"));
                }
                return Json(OperationResponse.Succeed("Odometer Record Updated"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
    }
}

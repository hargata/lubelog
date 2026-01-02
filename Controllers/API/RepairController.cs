using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public partial class APIController
    {
        [HttpGet]
        [Route("/api/vehicle/repairrecords/all")]
        public IActionResult AllRepairRecords(MethodParameter parameters)
        {
            List<int> vehicleIds = new List<int>();
            var vehicles = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehicles = _userLogic.FilterUserVehicles(vehicles, GetUserID());
            }
            vehicleIds.AddRange(vehicles.Select(x => x.Id));
            List<CollisionRecord> vehicleRecords = new List<CollisionRecord>();
            foreach (int vehicleId in vehicleIds)
            {
                vehicleRecords.AddRange(_collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId));
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
            var result = vehicleRecords.Select(x => new GenericRecordExportModel { Id = x.Id.ToString(), Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString(), Notes = x.Notes, Odometer = x.Mileage.ToString(), ExtraFields = x.ExtraFields, Files = x.Files, Tags = string.Join(' ', x.Tags) });
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
        [Route("/api/vehicle/repairrecords")]
        public IActionResult RepairRecords(int vehicleId, MethodParameter parameters)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
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
            var result = vehicleRecords.Select(x => new GenericRecordExportModel { Id = x.Id.ToString(), Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString(), Notes = x.Notes, Odometer = x.Mileage.ToString(), ExtraFields = x.ExtraFields, Files = x.Files, Tags = string.Join(' ', x.Tags) });
            if (_config.GetInvariantApi() || Request.Headers.ContainsKey("culture-invariant"))
            {
                return Json(result, StaticHelper.GetInvariantOption());
            }
            else
            {
                return Json(result);
            }
        }
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { false, true, HouseholdPermission.Edit })]
        [HttpPost]
        [Route("/api/vehicle/repairrecords/add")]
        [Consumes("application/json")]
        public IActionResult AddRepairRecordJson(int vehicleId, [FromBody] GenericRecordExportModel input) => AddRepairRecord(vehicleId, input);
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { false, true, HouseholdPermission.Edit })]
        [HttpPost]
        [Route("/api/vehicle/repairrecords/add")]
        public IActionResult AddRepairRecord(int vehicleId, GenericRecordExportModel input)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            if (string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.Odometer) ||
                string.IsNullOrWhiteSpace(input.Cost))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Date, Description, Odometer, and Cost cannot be empty."));
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
                var repairRecord = new CollisionRecord()
                {
                    VehicleId = vehicleId,
                    Date = DateTime.Parse(input.Date),
                    Mileage = int.Parse(input.Odometer),
                    Description = input.Description,
                    Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                    Cost = decimal.Parse(input.Cost),
                    ExtraFields = input.ExtraFields,
                    Files = input.Files,
                    Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList()
                };
                _collisionRecordDataAccess.SaveCollisionRecordToVehicle(repairRecord);
                if (_config.GetUserConfig(User).EnableAutoOdometerInsert)
                {
                    var odometerRecord = new OdometerRecord()
                    {
                        VehicleId = vehicleId,
                        Date = DateTime.Parse(input.Date),
                        Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                        Mileage = int.Parse(input.Odometer),
                        Files = StaticHelper.CreateAttachmentFromRecord(ImportMode.RepairRecord, repairRecord.Id, repairRecord.Description)
                    };
                    _odometerLogic.AutoInsertOdometerRecord(odometerRecord);
                }
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromGenericRecord(repairRecord, "repairrecord.add.api", User.Identity.Name));

                return Json(OperationResponse.Succeed("Repair Record Added", new { recordId = repairRecord.Id }));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [HttpDelete]
        [Route("/api/vehicle/repairrecords/delete")]
        public IActionResult DeleteRepairRecord(int id)
        {
            var existingRecord = _collisionRecordDataAccess.GetCollisionRecordById(id);
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
            //restore any requisitioned supplies.
            if (existingRecord.RequisitionHistory.Any())
            {
                _vehicleLogic.RestoreSupplyRecordsByUsage(existingRecord.RequisitionHistory, existingRecord.Description);
            }
            var result = _collisionRecordDataAccess.DeleteCollisionRecordById(existingRecord.Id);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromGenericRecord(existingRecord, "repairrecord.delete.api", User.Identity.Name));
            }
            return Json(OperationResponse.Conditional(result, "Repair Record Deleted"));
        }
        [HttpPut]
        [Route("/api/vehicle/repairrecords/update")]
        [Consumes("application/json")]
        public IActionResult UpdateRepairRecordJson([FromBody] GenericRecordExportModel input) => UpdateRepairRecord(input);
        [HttpPut]
        [Route("/api/vehicle/repairrecords/update")]
        public IActionResult UpdateRepairRecord(GenericRecordExportModel input)
        {
            if (string.IsNullOrWhiteSpace(input.Id) ||
                string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.Odometer) ||
                string.IsNullOrWhiteSpace(input.Cost))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Id, Date, Description, Odometer, and Cost cannot be empty."));
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
                var existingRecord = _collisionRecordDataAccess.GetCollisionRecordById(int.Parse(input.Id));
                if (existingRecord != null && existingRecord.Id == int.Parse(input.Id))
                {
                    //check if user has access to the vehicleId
                    if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Edit))
                    {
                        Response.StatusCode = 401;
                        return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
                    }
                    existingRecord.Date = DateTime.Parse(input.Date);
                    existingRecord.Mileage = int.Parse(input.Odometer);
                    existingRecord.Description = input.Description;
                    existingRecord.Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes;
                    existingRecord.Cost = decimal.Parse(input.Cost);
                    existingRecord.ExtraFields = input.ExtraFields;
                    existingRecord.Files = input.Files;
                    existingRecord.Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList();
                    _collisionRecordDataAccess.SaveCollisionRecordToVehicle(existingRecord);
                    StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromGenericRecord(existingRecord, "repairrecord.update.api", User.Identity.Name));
                }
                else
                {
                    Response.StatusCode = 400;
                    return Json(OperationResponse.Failed("Invalid Record Id"));
                }
                return Json(OperationResponse.Succeed("Repair Record Updated"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
    }
}

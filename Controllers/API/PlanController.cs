using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public partial class APIController
    {
        [HttpGet]
        [Route("/api/vehicle/planrecords/all")]
        public IActionResult AllPlanRecords(MethodParameter parameters)
        {
            List<int> vehicleIds = new List<int>();
            var vehicles = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehicles = _userLogic.FilterUserVehicles(vehicles, GetUserID());
            }
            vehicleIds.AddRange(vehicles.Select(x => x.Id));
            List<PlanRecord> vehicleRecords = new List<PlanRecord>();
            foreach (int vehicleId in vehicleIds)
            {
                vehicleRecords.AddRange(_planRecordDataAccess.GetPlanRecordsByVehicleId(vehicleId));
            }
            if (parameters.Id != default)
            {
                vehicleRecords.RemoveAll(x => x.Id != parameters.Id);
            }
            if (!string.IsNullOrWhiteSpace(parameters.StartDate) && DateTime.TryParse(parameters.StartDate, out DateTime startDate))
            {
                vehicleRecords.RemoveAll(x => x.DateCreated < startDate);
            }
            if (!string.IsNullOrWhiteSpace(parameters.EndDate) && DateTime.TryParse(parameters.EndDate, out DateTime endDate))
            {
                vehicleRecords.RemoveAll(x => x.DateCreated > endDate);
            }
            var result = vehicleRecords.Select(x => new PlanRecordExportModel
            {
                VehicleId = x.VehicleId.ToString(),
                Id = x.Id.ToString(),
                DateCreated = x.DateCreated.ToShortDateString(),
                DateModified = x.DateModified.ToShortDateString(),
                Description = x.Description,
                Cost = x.Cost.ToString(),
                Notes = x.Notes,
                Type = x.ImportMode.ToString(),
                Priority = x.Priority.ToString(),
                Progress = x.Progress.ToString(),
                ExtraFields = x.ExtraFields,
                Files = x.Files
            });
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
        [Route("/api/vehicle/planrecords")]
        public IActionResult PlanRecords(int vehicleId, MethodParameter parameters)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _planRecordDataAccess.GetPlanRecordsByVehicleId(vehicleId);
            if (parameters.Id != default)
            {
                vehicleRecords.RemoveAll(x => x.Id != parameters.Id);
            }
            if (!string.IsNullOrWhiteSpace(parameters.StartDate) && DateTime.TryParse(parameters.StartDate, out DateTime startDate))
            {
                vehicleRecords.RemoveAll(x => x.DateCreated < startDate);
            }
            if (!string.IsNullOrWhiteSpace(parameters.EndDate) && DateTime.TryParse(parameters.EndDate, out DateTime endDate))
            {
                vehicleRecords.RemoveAll(x => x.DateCreated > endDate);
            }
            var result = vehicleRecords.Select(x => new PlanRecordExportModel
            {
                VehicleId = x.VehicleId.ToString(),
                Id = x.Id.ToString(),
                DateCreated = x.DateCreated.ToShortDateString(),
                DateModified = x.DateModified.ToShortDateString(),
                Description = x.Description,
                Cost = x.Cost.ToString(),
                Notes = x.Notes,
                Type = x.ImportMode.ToString(),
                Priority = x.Priority.ToString(),
                Progress = x.Progress.ToString(),
                ExtraFields = x.ExtraFields,
                Files = x.Files
            });
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
        [Route("/api/vehicle/planrecords/add")]
        [Consumes("application/json")]
        public IActionResult AddPlanRecordJson(int vehicleId, [FromBody] PlanRecordExportModel input) => AddPlanRecord(vehicleId, input);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { false, true, HouseholdPermission.Edit })]
        [HttpPost]
        [Route("/api/vehicle/planrecords/add")]
        public IActionResult AddPlanRecord(int vehicleId, PlanRecordExportModel input)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            if (string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.Cost) ||
                string.IsNullOrWhiteSpace(input.Type) ||
                string.IsNullOrWhiteSpace(input.Priority) ||
                string.IsNullOrWhiteSpace(input.Progress))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Description, Cost, Type, Priority, and Progress cannot be empty."));
            }
            bool validType = Enum.TryParse(input.Type, out ImportMode parsedType);
            bool validPriority = Enum.TryParse(input.Priority, out PlanPriority parsedPriority);
            bool validProgress = Enum.TryParse(input.Progress, out PlanProgress parsedProgress);
            if (!validType || !validPriority || !validProgress)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, values for Type(ServiceRecord, RepairRecord, UpgradeRecord), Priority(Critical, Normal, Low), or Progress(Backlog, InProgress, Testing) is invalid."));
            }
            if (parsedType != ImportMode.ServiceRecord && parsedType != ImportMode.RepairRecord && parsedType != ImportMode.UpgradeRecord)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Type can only ServiceRecord, RepairRecord, or UpgradeRecord"));
            }
            if (parsedProgress == PlanProgress.Done)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Progress cannot be set to Done."));
            }
            //hardening - turns null values for List types into empty lists.
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
                var planRecord = new PlanRecord()
                {
                    VehicleId = vehicleId,
                    DateCreated = DateTime.Now,
                    DateModified = DateTime.Now,
                    Description = input.Description,
                    Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                    Cost = decimal.Parse(input.Cost),
                    ImportMode = parsedType,
                    Priority = parsedPriority,
                    Progress = parsedProgress,
                    ExtraFields = input.ExtraFields,
                    Files = input.Files
                };
                _planRecordDataAccess.SavePlanRecordToVehicle(planRecord);
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromPlanRecord(planRecord, "planrecord.add.api", User.Identity.Name));
                return Json(OperationResponse.Succeed("Plan Record Added", new { recordId = planRecord.Id }));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Delete })]
        [HttpDelete]
        [Route("/api/vehicle/planrecords/delete")]
        public IActionResult DeletePlanRecord(int id)
        {
            var existingRecord = _planRecordDataAccess.GetPlanRecordById(id);
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
            var result = _planRecordDataAccess.DeletePlanRecordById(existingRecord.Id);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromPlanRecord(existingRecord, "planrecord.delete.api", User.Identity.Name));
            }
            return Json(OperationResponse.Conditional(result, "Plan Record Deleted"));
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/planrecords/update")]
        [Consumes("application/json")]
        public IActionResult UpdatePlanRecordJson([FromBody] PlanRecordExportModel input) => UpdatePlanRecord(input);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/planrecords/update")]
        public IActionResult UpdatePlanRecord(PlanRecordExportModel input)
        {
            if (string.IsNullOrWhiteSpace(input.Id) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.Cost) ||
                string.IsNullOrWhiteSpace(input.Type) ||
                string.IsNullOrWhiteSpace(input.Priority) ||
                string.IsNullOrWhiteSpace(input.Progress))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Id, Description, Cost, Type, Priority, and Progress cannot be empty."));
            }
            bool validType = Enum.TryParse(input.Type, out ImportMode parsedType);
            bool validPriority = Enum.TryParse(input.Priority, out PlanPriority parsedPriority);
            bool validProgress = Enum.TryParse(input.Progress, out PlanProgress parsedProgress);
            if (!validType || !validPriority || !validProgress)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, values for Type(ServiceRecord, RepairRecord, UpgradeRecord), Priority(Critical, Normal, Low), or Progress(Backlog, InProgress, Testing) is invalid."));
            }
            if (parsedType != ImportMode.ServiceRecord && parsedType != ImportMode.RepairRecord && parsedType != ImportMode.UpgradeRecord)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Type can only ServiceRecord, RepairRecord, or UpgradeRecord"));
            }
            if (parsedProgress == PlanProgress.Done)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Progress cannot be set to Done."));
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
                var existingRecord = _planRecordDataAccess.GetPlanRecordById(int.Parse(input.Id));
                if (existingRecord != null && existingRecord.Id == int.Parse(input.Id))
                {
                    //check if user has access to the vehicleId
                    if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Edit))
                    {
                        Response.StatusCode = 401;
                        return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
                    }
                    existingRecord.DateModified = DateTime.Now;
                    existingRecord.Description = input.Description;
                    existingRecord.Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes;
                    existingRecord.Cost = decimal.Parse(input.Cost);
                    existingRecord.ImportMode = parsedType;
                    existingRecord.Priority = parsedPriority;
                    existingRecord.Progress = parsedProgress;
                    existingRecord.Files = input.Files;
                    existingRecord.ExtraFields = input.ExtraFields;
                    _planRecordDataAccess.SavePlanRecordToVehicle(existingRecord);
                    StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromPlanRecord(existingRecord, "planrecord.update.api", User.Identity.Name));
                }
                else
                {
                    Response.StatusCode = 400;
                    return Json(OperationResponse.Failed("Invalid Record Id"));
                }
                return Json(OperationResponse.Succeed("Plan Record Updated"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
    }
}

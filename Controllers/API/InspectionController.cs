using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public partial class APIController
    {
        private List<InspectionRecordTemplateFieldExportModel> ConvertToInspectionRecordTemplateFieldExportModels(List<InspectionRecordTemplateField> input)
        {
            return input.Select(x => new InspectionRecordTemplateFieldExportModel
            {
                Description = x.Description,
                FieldType = x.FieldType.ToString(),
                Options = x.Options.Select(y => new InspectionRecordTemplateFieldOptionExportModel { Description = y.Description, IsFail = y.IsFail.ToString() }).ToList(),
                HasNotes = x.HasNotes.ToString(),
                Notes = x.Notes,
                HasActionItem = x.HasActionItem.ToString(),
                ActionItemType = x.ActionItemType.ToString(),
                ActionItemDescription = x.ActionItemDescription,
                ActionItemPriority = x.ActionItemPriority.ToString()
            }).ToList();
        }
        private bool TryConvertInspectionRecordTemplateFields(List<InspectionRecordTemplateFieldExportModel> input, out List<InspectionRecordTemplateField> result, out string errorMessage)
        {
            result = new List<InspectionRecordTemplateField>();
            errorMessage = string.Empty;
            foreach (var field in input)
            {
                if (string.IsNullOrWhiteSpace(field.Description))
                {
                    errorMessage = "Input object invalid, Field Description cannot be empty.";
                    return false;
                }
                if (!Enum.TryParse(field.FieldType, out InspectionFieldType parsedFieldType))
                {
                    errorMessage = $"Input object invalid, FieldType({string.Join(", ", Enum.GetNames(typeof(InspectionFieldType)))}) is invalid for field '{field.Description}'.";
                    return false;
                }
                bool hasActionItem = !string.IsNullOrWhiteSpace(field.HasActionItem) && bool.Parse(field.HasActionItem);
                ImportMode parsedActionItemType = ImportMode.ServiceRecord;
                PlanPriority parsedActionItemPriority = PlanPriority.Normal;
                if (hasActionItem)
                {
                    if (!Enum.TryParse(field.ActionItemType, out parsedActionItemType))
                    {
                        errorMessage = $"Input object invalid, ActionItemType is invalid for field '{field.Description}'.";
                        return false;
                    }
                    if (!Enum.TryParse(field.ActionItemPriority, out parsedActionItemPriority))
                    {
                        errorMessage = $"Input object invalid, ActionItemPriority({string.Join(", ", Enum.GetNames(typeof(PlanPriority)))}) is invalid for field '{field.Description}'.";
                        return false;
                    }
                }
                result.Add(new InspectionRecordTemplateField
                {
                    Description = field.Description,
                    FieldType = parsedFieldType,
                    Options = (field.Options ?? new List<InspectionRecordTemplateFieldOptionExportModel>()).Select(x => new InspectionRecordTemplateFieldOption
                    {
                        Description = x.Description,
                        IsFail = !string.IsNullOrWhiteSpace(x.IsFail) && bool.Parse(x.IsFail),
                        IsSelected = false
                    }).ToList(),
                    HasNotes = !string.IsNullOrWhiteSpace(field.HasNotes) && bool.Parse(field.HasNotes),
                    Notes = field.Notes ?? string.Empty,
                    HasActionItem = hasActionItem,
                    ActionItemType = parsedActionItemType,
                    ActionItemDescription = field.ActionItemDescription ?? string.Empty,
                    ActionItemPriority = parsedActionItemPriority
                });
            }
            return true;
        }
        [HttpGet]
        [Route("/api/vehicle/inspection/templates/all")]
        public IActionResult AllInspectionRecordTemplates(MethodParameter parameters)
        {
            List<int> vehicleIds = new List<int>();
            var vehicles = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehicles = _userLogic.FilterUserVehicles(vehicles, GetUserID());
            }
            vehicleIds.AddRange(vehicles.Select(x => x.Id));
            List<InspectionRecordInput> templateResults = new List<InspectionRecordInput>();
            foreach (int vehicleId in vehicleIds)
            {
                templateResults.AddRange(_inspectionRecordTemplateDataAccess.GetInspectionRecordTemplatesByVehicleId(vehicleId));
            }
            if (parameters.Id != default)
            {
                templateResults.RemoveAll(x => x.Id != parameters.Id);
            }
            if (!string.IsNullOrWhiteSpace(parameters.Tags))
            {
                var tagsFilter = parameters.Tags.Split(' ').Distinct();
                templateResults.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
            }
            var results = templateResults.Select(x => new InspectionRecordTemplateExportModel { VehicleId = x.VehicleId.ToString(), Id = x.Id.ToString(), Description = x.Description, Fields = ConvertToInspectionRecordTemplateFieldExportModels(x.Fields), Tags = string.Join(' ', x.Tags) });
            if (_config.GetInvariantApi() || Request.Headers.ContainsKey("culture-invariant"))
            {
                return Json(results, StaticHelper.GetInvariantOption());
            }
            else
            {
                return Json(results);
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/inspection/templates")]
        public IActionResult InspectionRecordTemplates(int vehicleId, MethodParameter parameters)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            var templateResults = _inspectionRecordTemplateDataAccess.GetInspectionRecordTemplatesByVehicleId(vehicleId);
            if (parameters.Id != default)
            {
                templateResults.RemoveAll(x => x.Id != parameters.Id);
            }
            if (!string.IsNullOrWhiteSpace(parameters.Tags))
            {
                var tagsFilter = parameters.Tags.Split(' ').Distinct();
                templateResults.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
            }
            var results = templateResults.Select(x => new InspectionRecordTemplateExportModel { VehicleId = x.VehicleId.ToString(), Id = x.Id.ToString(), Description = x.Description, Fields = ConvertToInspectionRecordTemplateFieldExportModels(x.Fields), Tags = string.Join(' ', x.Tags) });
            if (_config.GetInvariantApi() || Request.Headers.ContainsKey("culture-invariant"))
            {
                return Json(results, StaticHelper.GetInvariantOption());
            }
            else
            {
                return Json(results);
            }
        }
        [TypeFilter(typeof(QueryParamFilter), Arguments = new object[] { new string[] { "vehicleId" } })]
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { false, true, HouseholdPermission.Edit })]
        [HttpPost]
        [Route("/api/vehicle/inspection/templates/add")]
        [Consumes("application/json")]
        public IActionResult AddInspectionRecordTemplateJson(int vehicleId, [FromBody] InspectionRecordTemplateExportModel input) => AddInspectionRecordTemplate(vehicleId, input);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { false, true, HouseholdPermission.Edit })]
        [HttpPost]
        [Route("/api/vehicle/inspection/templates/add")]
        public IActionResult AddInspectionRecordTemplate(int vehicleId, InspectionRecordTemplateExportModel input)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            if (string.IsNullOrWhiteSpace(input.Description))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Description cannot be empty."));
            }
            if (input.Fields == null)
            {
                input.Fields = new List<InspectionRecordTemplateFieldExportModel>();
            }
            if (!TryConvertInspectionRecordTemplateFields(input.Fields, out var convertedFields, out var fieldErrorMessage))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed(fieldErrorMessage));
            }
            try
            {
                var inspectionRecordTemplate = new InspectionRecordInput()
                {
                    VehicleId = vehicleId,
                    Description = input.Description,
                    Fields = convertedFields,
                    Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList()
                };
                _inspectionRecordTemplateDataAccess.SaveInspectionReportTemplateToVehicle(inspectionRecordTemplate);
                return Json(OperationResponse.Succeed("Inspection Record Template Added", new { recordId = inspectionRecordTemplate.Id }));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/inspection/templates/update")]
        [Consumes("application/json")]
        public IActionResult UpdateInspectionRecordTemplateJson([FromBody] InspectionRecordTemplateExportModel input) => UpdateInspectionRecordTemplate(input);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/inspection/templates/update")]
        public IActionResult UpdateInspectionRecordTemplate(InspectionRecordTemplateExportModel input)
        {
            if (string.IsNullOrWhiteSpace(input.Id) ||
                string.IsNullOrWhiteSpace(input.Description))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Id and Description cannot be empty."));
            }
            if (input.Fields == null)
            {
                input.Fields = new List<InspectionRecordTemplateFieldExportModel>();
            }
            if (!TryConvertInspectionRecordTemplateFields(input.Fields, out var convertedFields, out var fieldErrorMessage))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed(fieldErrorMessage));
            }
            try
            {
                var existingRecord = _inspectionRecordTemplateDataAccess.GetInspectionRecordTemplateById(int.Parse(input.Id));
                if (existingRecord != null && existingRecord.Id == int.Parse(input.Id))
                {
                    //check if user has access to the vehicleId
                    if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Edit))
                    {
                        Response.StatusCode = 401;
                        return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
                    }
                    existingRecord.Description = input.Description;
                    existingRecord.Fields = convertedFields;
                    existingRecord.Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList();
                    _inspectionRecordTemplateDataAccess.SaveInspectionReportTemplateToVehicle(existingRecord);
                }
                else
                {
                    Response.StatusCode = 400;
                    return Json(OperationResponse.Failed("Invalid Record Id"));
                }
                return Json(OperationResponse.Succeed("Inspection Record Template Updated"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Delete })]
        [HttpDelete]
        [Route("/api/vehicle/inspection/templates/delete")]
        public IActionResult DeleteInspectionRecordTemplate(int id)
        {
            var existingRecord = _inspectionRecordTemplateDataAccess.GetInspectionRecordTemplateById(id);
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
            var result = _inspectionRecordTemplateDataAccess.DeleteInspectionRecordTemplateById(existingRecord.Id);
            return Json(OperationResponse.Conditional(result, "Inspection Record Template Deleted"));
        }
    }
}

using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public partial class APIController
    {
        [HttpGet]
        [Route("/api/vehicle/insurancerecords/all")]
        public IActionResult AllInsuranceRecords(MethodParameter parameters)
        {
            List<int> vehicleIds = new List<int>();
            var vehicles = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehicles = _userLogic.FilterUserVehicles(vehicles, GetUserID());
            }
            vehicleIds.AddRange(vehicles.Select(x => x.Id));
            List<InsuranceRecord> vehicleRecords = new List<InsuranceRecord>();
            foreach (int vehicleId in vehicleIds)
            {
                vehicleRecords.AddRange(_insuranceRecordDataAccess.GetInsuranceRecordsByVehicleId(vehicleId));
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
            var result = vehicleRecords.Select(x => new InsuranceRecordExportModel { VehicleId = x.VehicleId.ToString(), Id = x.Id.ToString(), Date = x.Date.ToShortDateString(), Description = x.Description, Provider = x.Provider, PolicyNumber = x.PolicyNumber, Cost = x.Cost.ToString(), TotalPremium = x.TotalPremium.ToString(), Payments = x.Payments, Notes = x.Notes, ExtraFields = x.ExtraFields, Files = x.Files, Tags = string.Join(' ', x.Tags) });
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
        [Route("/api/vehicle/insurancerecords")]
        public IActionResult InsuranceRecords(int vehicleId, MethodParameter parameters)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _insuranceRecordDataAccess.GetInsuranceRecordsByVehicleId(vehicleId);
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
            var result = vehicleRecords.Select(x => new InsuranceRecordExportModel { VehicleId = x.VehicleId.ToString(), Id = x.Id.ToString(), Date = x.Date.ToShortDateString(), Description = x.Description, Provider = x.Provider, PolicyNumber = x.PolicyNumber, Cost = x.Cost.ToString(), TotalPremium = x.TotalPremium.ToString(), Payments = x.Payments, Notes = x.Notes, ExtraFields = x.ExtraFields, Files = x.Files, Tags = string.Join(' ', x.Tags) });
            if (_config.GetInvariantApi() || Request.Headers.ContainsKey("culture-invariant"))
            {
                return Json(result, StaticHelper.GetInvariantOption());
            }
            else
            {
                return Json(result);
            }
        }
        [HttpGet]
        [Route("/api/vehicle/insurancerecords/check")]
        public IActionResult CheckRecurringInsuranceRecords()
        {
            List<Vehicle> vehicles = new List<Vehicle>();
            try
            {
                var result = _dataAccess.GetVehicles();
                if (!User.IsInRole(nameof(UserData.IsRootUser)))
                {
                    result = _userLogic.FilterUserVehicles(result, GetUserID());
                }
                vehicles.AddRange(result);
                int vehiclesUpdated = 0;
                foreach (Vehicle vehicle in vehicles)
                {
                    var updateResult = _vehicleLogic.UpdateRecurringInsurance(vehicle.Id);
                    if (updateResult)
                    {
                        vehiclesUpdated++;
                    }
                }
                if (vehiclesUpdated != default)
                {
                    return Json(OperationResponse.Succeed($"Recurring Insurance for {vehiclesUpdated} Vehicles Updated!"));
                }
                else
                {
                    return Json(OperationResponse.Succeed("No Recurring Insurance Updated"));
                }
            }
            catch (Exception ex)
            {
                return Json(OperationResponse.Failed($"No Recurring Insurance Updated Due To Error: {ex.Message}"));
            }
        }
        [TypeFilter(typeof(QueryParamFilter), Arguments = new object[] { new string[] { "vehicleId" } })]
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { false, true, HouseholdPermission.Edit })]
        [HttpPost]
        [Route("/api/vehicle/insurancerecords/add")]
        [Consumes("application/json")]
        public IActionResult AddInsuranceRecordJson(int vehicleId, [FromBody] InsuranceRecordExportModel input) => AddInsuranceRecord(vehicleId, input);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { false, true, HouseholdPermission.Edit })]
        [HttpPost]
        [Route("/api/vehicle/insurancerecords/add")]
        public IActionResult AddInsuranceRecord(int vehicleId, InsuranceRecordExportModel input)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            if (string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.Cost))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Date, Description, and Cost cannot be empty."));
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
                var insuranceRecord = new InsuranceRecord()
                {
                    VehicleId = vehicleId,
                    Date = DateTime.Parse(input.Date),
                    Description = input.Description,
                    Provider = string.IsNullOrWhiteSpace(input.Provider) ? "" : input.Provider,
                    PolicyNumber = string.IsNullOrWhiteSpace(input.PolicyNumber) ? "" : input.PolicyNumber,
                    Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                    Cost = (input.Payments != null && input.Payments.Any()) ? input.Payments.Sum(x => x.Amount) : decimal.Parse(input.Cost),
                    TotalPremium = string.IsNullOrWhiteSpace(input.TotalPremium) ? 0 : decimal.Parse(input.TotalPremium),
                    Payments = input.Payments ?? new List<InsurancePayment>(),
                    ExtraFields = input.ExtraFields,
                    Files = input.Files,
                    Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList()
                };
                _insuranceRecordDataAccess.SaveInsuranceRecordToVehicle(insuranceRecord);
                _vehicleLogic.UpdateRecurringInsurance(vehicleId);
                _eventLogic.PublishEvent(GetUserID(), WebHookPayload.FromInsuranceRecord(insuranceRecord, "insurancerecord.add.api", User.Identity?.Name ?? string.Empty));
                return Json(OperationResponse.Succeed("Insurance Record Added", new { recordId = insuranceRecord.Id }));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Delete })]
        [HttpDelete]
        [Route("/api/vehicle/insurancerecords/delete")]
        public IActionResult DeleteInsuranceRecord(int id)
        {
            var existingRecord = _insuranceRecordDataAccess.GetInsuranceRecordById(id);
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
            var result = _insuranceRecordDataAccess.DeleteInsuranceRecordById(existingRecord.Id);
            if (result)
            {
                _eventLogic.PublishEvent(GetUserID(), WebHookPayload.FromInsuranceRecord(existingRecord, "insurancerecord.delete.api", User.Identity?.Name ?? string.Empty));
            }
            return Json(OperationResponse.Conditional(result, "Insurance Record Deleted"));
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/insurancerecords/update")]
        [Consumes("application/json")]
        public IActionResult UpdateInsuranceRecordJson([FromBody] InsuranceRecordExportModel input) => UpdateInsuranceRecord(input);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/insurancerecords/update")]
        public IActionResult UpdateInsuranceRecord(InsuranceRecordExportModel input)
        {
            if (string.IsNullOrWhiteSpace(input.Id) ||
                string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.Cost))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Id, Date, Description, and Cost cannot be empty."));
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
                var existingRecord = _insuranceRecordDataAccess.GetInsuranceRecordById(int.Parse(input.Id));
                if (existingRecord != null && existingRecord.Id == int.Parse(input.Id))
                {
                    //check if user has access to the vehicleId
                    if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Edit))
                    {
                        Response.StatusCode = 401;
                        return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
                    }
                    existingRecord.Date = DateTime.Parse(input.Date);
                    existingRecord.Description = input.Description;
                    existingRecord.Provider = string.IsNullOrWhiteSpace(input.Provider) ? "" : input.Provider;
                    existingRecord.PolicyNumber = string.IsNullOrWhiteSpace(input.PolicyNumber) ? "" : input.PolicyNumber;
                    existingRecord.Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes;
                    existingRecord.Cost = (input.Payments != null && input.Payments.Any()) ? input.Payments.Sum(x => x.Amount) : decimal.Parse(input.Cost);
                    existingRecord.TotalPremium = string.IsNullOrWhiteSpace(input.TotalPremium) ? 0 : decimal.Parse(input.TotalPremium);
                    existingRecord.Payments = input.Payments ?? new List<InsurancePayment>();
                    existingRecord.ExtraFields = input.ExtraFields;
                    existingRecord.Files = input.Files;
                    existingRecord.Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList();
                    _insuranceRecordDataAccess.SaveInsuranceRecordToVehicle(existingRecord);
                    _eventLogic.PublishEvent(GetUserID(), WebHookPayload.FromInsuranceRecord(existingRecord, "insurancerecord.update.api", User.Identity?.Name ?? string.Empty));
                }
                else
                {
                    Response.StatusCode = 400;
                    return Json(OperationResponse.Failed("Invalid Record Id"));
                }
                return Json(OperationResponse.Succeed("Insurance Record Updated"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
    }
}

using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public partial class APIController
    {
        [HttpGet]
        [Route("/api/vehicle/supplyrecords/all")]
        public IActionResult AllSupplyRecords(MethodParameter parameters)
        {
            List<int> vehicleIds = new List<int>();
            var vehicles = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehicles = _userLogic.FilterUserVehicles(vehicles, GetUserID());
            }
            vehicleIds.AddRange(vehicles.Select(x => x.Id));
            if (_config.GetServerEnableShopSupplies())
            {
                vehicleIds.Add(0);
            }
            List<SupplyRecord> vehicleRecords = new List<SupplyRecord>();
            foreach (int vehicleId in vehicleIds)
            {
                vehicleRecords.AddRange(_supplyRecordDataAccess.GetSupplyRecordsByVehicleId(vehicleId));
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
            var result = vehicleRecords
                .Select(x => new SupplyRecordExportModel
                {
                    VehicleId = x.VehicleId.ToString(),
                    Id = x.Id.ToString(),
                    Date = x.Date.ToString(),
                    PartNumber = x.PartNumber,
                    PartSupplier = x.PartSupplier,
                    PartQuantity = x.Quantity.ToString(),
                    Description = x.Description,
                    Cost = x.Cost.ToString(),
                    Notes = x.Notes,
                    ExtraFields = x.ExtraFields,
                    Files = x.Files,
                    Tags = string.Join(' ', x.Tags)
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
        [Route("/api/vehicle/supplyrecords")]
        public IActionResult SupplyRecords(int vehicleId, MethodParameter parameters)
        {
            if (vehicleId == default && !_config.GetServerEnableShopSupplies())
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _supplyRecordDataAccess.GetSupplyRecordsByVehicleId(vehicleId);
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
            var result = vehicleRecords
                .Select(x => new SupplyRecordExportModel
                {
                    VehicleId = x.VehicleId.ToString(),
                    Id = x.Id.ToString(),
                    Date = x.Date.ToString(),
                    PartNumber = x.PartNumber,
                    PartSupplier = x.PartSupplier,
                    PartQuantity = x.Quantity.ToString(),
                    Description = x.Description,
                    Cost = x.Cost.ToString(),
                    Notes = x.Notes,
                    ExtraFields = x.ExtraFields,
                    Files = x.Files,
                    Tags = string.Join(' ', x.Tags)
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
        [Route("/api/vehicle/supplyrecords/add")]
        [Consumes("application/json")]
        public IActionResult AddSupplyRecordJson(int vehicleId, [FromBody] SupplyRecordExportModel input) => AddSupplyRecord(vehicleId, input);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { false, true, HouseholdPermission.Edit })]
        [HttpPost]
        [Route("/api/vehicle/supplyrecords/add")]
        public IActionResult AddSupplyRecord(int vehicleId, SupplyRecordExportModel input)
        {
            if (vehicleId == default && !_config.GetServerEnableShopSupplies())
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            if (string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.PartQuantity) ||
                string.IsNullOrWhiteSpace(input.Cost))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Date, Description, Quantity and Cost cannot be empty."));
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
                var supplyRecord = new SupplyRecord()
                {
                    VehicleId = vehicleId,
                    Date = DateTime.Parse(input.Date),
                    PartNumber = input.PartNumber,
                    PartSupplier = input.PartSupplier,
                    Quantity = decimal.Parse(input.PartQuantity),
                    Description = input.Description,
                    Cost = decimal.Parse(input.Cost),
                    Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                    ExtraFields = input.ExtraFields,
                    Files = input.Files,
                    Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList()
                };
                _supplyRecordDataAccess.SaveSupplyRecordToVehicle(supplyRecord);
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromSupplyRecord(supplyRecord, "supplyrecord.add.api", User.Identity?.Name ?? string.Empty));
                return Json(OperationResponse.Succeed("Supply Record Added", new { recordId = supplyRecord.Id }));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Delete })]
        [HttpDelete]
        [Route("/api/vehicle/supplyrecords/delete")]
        public IActionResult DeleteSupplyRecord(int id)
        {
            var existingRecord = _supplyRecordDataAccess.GetSupplyRecordById(id);
            if (existingRecord == null || existingRecord.Id == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Invalid Record Id"));
            }
            //security check.
            if (existingRecord.VehicleId != default)
            {
                if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Delete))
                {
                    Response.StatusCode = 401;
                    return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
                }
            }
            else if (!_config.GetServerEnableShopSupplies())
            {
                //shop supplies not enabled
                Response.StatusCode = 401;
                return Json(OperationResponse.Failed("Access Denied, shop supplies is not enabled."));
            }

            var result = _supplyRecordDataAccess.DeleteSupplyRecordById(existingRecord.Id);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromSupplyRecord(existingRecord, "supplyrecord.delete.api", User.Identity?.Name ?? string.Empty));
            }
            return Json(OperationResponse.Conditional(result, "Supply Record Deleted"));
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/supplyrecords/update")]
        [Consumes("application/json")]
        public IActionResult UpdateSupplyRecordJson([FromBody] SupplyRecordExportModel input) => UpdateSupplyRecord(input);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/supplyrecords/update")]
        public IActionResult UpdateSupplyRecord(SupplyRecordExportModel input)
        {
            if (string.IsNullOrWhiteSpace(input.Id) ||
                string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.PartQuantity) ||
                string.IsNullOrWhiteSpace(input.Cost))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Id, Date, Description, Quantity and Cost cannot be empty."));
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
                var existingRecord = _supplyRecordDataAccess.GetSupplyRecordById(int.Parse(input.Id));
                if (existingRecord != null && existingRecord.Id == int.Parse(input.Id))
                {
                    //check if user has access to the vehicleId
                    if (existingRecord.VehicleId != default)
                    {
                        if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Edit))
                        {
                            Response.StatusCode = 401;
                            return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
                        }
                    }
                    else if (!_config.GetServerEnableShopSupplies())
                    {
                        //shop supplies not enabled
                        Response.StatusCode = 401;
                        return Json(OperationResponse.Failed("Access Denied, shop supplies is not enabled."));
                    }
                    existingRecord.Date = DateTime.Parse(input.Date);
                    existingRecord.PartNumber = input.PartNumber;
                    existingRecord.PartSupplier = input.PartSupplier;
                    existingRecord.Quantity = decimal.Parse(input.PartQuantity);
                    existingRecord.Description = input.Description;
                    existingRecord.Cost = decimal.Parse(input.Cost);
                    existingRecord.Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes;
                    existingRecord.ExtraFields = input.ExtraFields;
                    existingRecord.Files = input.Files;
                    existingRecord.Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList();
                    _supplyRecordDataAccess.SaveSupplyRecordToVehicle(existingRecord);
                    StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromSupplyRecord(existingRecord, "supplyrecord.update.api", User.Identity?.Name ?? string.Empty));
                }
                else
                {
                    Response.StatusCode = 400;
                    return Json(OperationResponse.Failed("Invalid Record Id"));
                }
                return Json(OperationResponse.Succeed("Supply Record Updated"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
    }
}

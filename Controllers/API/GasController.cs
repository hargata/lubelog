using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public partial class APIController
    {
        [HttpGet]
        [Route("/api/vehicle/gasrecords/all")]
        public IActionResult AllGasRecords(MethodParameter parameters)
        {
            List<int> vehicleIds = new List<int>();
            var vehicles = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehicles = _userLogic.FilterUserVehicles(vehicles, GetUserID());
            }
            vehicleIds.AddRange(vehicles.Select(x => x.Id));
            List<GasRecordViewModel> vehicleRecords = new List<GasRecordViewModel>();
            foreach (int vehicleId in vehicleIds)
            {
                var rawVehicleRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
                vehicleRecords.AddRange(_gasHelper.GetGasRecordViewModels(rawVehicleRecords, parameters.UseMPG, parameters.UseUKMPG));
            }
            if (parameters.Id != default)
            {
                vehicleRecords.RemoveAll(x => x.Id != parameters.Id);
            }
            if (!string.IsNullOrWhiteSpace(parameters.StartDate) && DateTime.TryParse(parameters.StartDate, out DateTime startDate))
            {
                vehicleRecords.RemoveAll(x => DateTime.Parse(x.Date) < startDate);
            }
            if (!string.IsNullOrWhiteSpace(parameters.EndDate) && DateTime.TryParse(parameters.EndDate, out DateTime endDate))
            {
                vehicleRecords.RemoveAll(x => DateTime.Parse(x.Date) > endDate);
            }
            if (!string.IsNullOrWhiteSpace(parameters.Tags))
            {
                var tagsFilter = parameters.Tags.Split(' ').Distinct();
                vehicleRecords.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
            }
            var result = vehicleRecords
                .Select(x => new GasRecordExportModel
                {
                    VehicleId = x.VehicleId.ToString(),
                    Id = x.Id.ToString(),
                    Date = x.Date,
                    Odometer = x.Mileage.ToString(),
                    Cost = x.Cost.ToString(),
                    FuelConsumed = x.Gallons.ToString(),
                    FuelEconomy = x.MilesPerGallon.ToString(),
                    IsFillToFull = x.IsFillToFull.ToString(),
                    MissedFuelUp = x.MissedFuelUp.ToString(),
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
        [Route("/api/vehicle/gasrecords")]
        public IActionResult GasRecords(int vehicleId, MethodParameter parameters)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var rawVehicleRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            var vehicleRecords = _gasHelper.GetGasRecordViewModels(rawVehicleRecords, parameters.UseMPG, parameters.UseUKMPG);
            if (parameters.Id != default)
            {
                vehicleRecords.RemoveAll(x => x.Id != parameters.Id);
            }
            if (!string.IsNullOrWhiteSpace(parameters.StartDate) && DateTime.TryParse(parameters.StartDate, out DateTime startDate))
            {
                vehicleRecords.RemoveAll(x => DateTime.Parse(x.Date) < startDate);
            }
            if (!string.IsNullOrWhiteSpace(parameters.EndDate) && DateTime.TryParse(parameters.EndDate, out DateTime endDate))
            {
                vehicleRecords.RemoveAll(x => DateTime.Parse(x.Date) > endDate);
            }
            if (!string.IsNullOrWhiteSpace(parameters.Tags))
            {
                var tagsFilter = parameters.Tags.Split(' ').Distinct();
                vehicleRecords.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
            }
            var result = vehicleRecords
                .Select(x => new GasRecordExportModel
                {
                    VehicleId = x.VehicleId.ToString(),
                    Id = x.Id.ToString(),
                    Date = x.Date,
                    Odometer = x.Mileage.ToString(),
                    Cost = x.Cost.ToString(),
                    FuelConsumed = x.Gallons.ToString(),
                    FuelEconomy = x.MilesPerGallon.ToString(),
                    IsFillToFull = x.IsFillToFull.ToString(),
                    MissedFuelUp = x.MissedFuelUp.ToString(),
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
        [Route("/api/vehicle/gasrecords/add")]
        [Consumes("application/json")]
        public IActionResult AddGasRecordJson(int vehicleId, [FromBody] GasRecordExportModel input) => AddGasRecord(vehicleId, input);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { false, true, HouseholdPermission.Edit })]
        [HttpPost]
        [Route("/api/vehicle/gasrecords/add")]
        public IActionResult AddGasRecord(int vehicleId, GasRecordExportModel input)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            if (string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Odometer) ||
                string.IsNullOrWhiteSpace(input.FuelConsumed) ||
                string.IsNullOrWhiteSpace(input.Cost) ||
                string.IsNullOrWhiteSpace(input.IsFillToFull) ||
                string.IsNullOrWhiteSpace(input.MissedFuelUp)
                )
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Date, Odometer, FuelConsumed, IsFillToFull, MissedFuelUp, and Cost cannot be empty."));
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
                var gasRecord = new GasRecord()
                {
                    VehicleId = vehicleId,
                    Date = DateTime.Parse(input.Date),
                    Mileage = int.Parse(input.Odometer),
                    Gallons = decimal.Parse(input.FuelConsumed),
                    IsFillToFull = bool.Parse(input.IsFillToFull),
                    MissedFuelUp = bool.Parse(input.MissedFuelUp),
                    Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                    Cost = decimal.Parse(input.Cost),
                    ExtraFields = input.ExtraFields,
                    Files = input.Files,
                    Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList()
                };
                _gasRecordDataAccess.SaveGasRecordToVehicle(gasRecord);
                if (_config.GetUserConfig(User).EnableAutoOdometerInsert)
                {
                    var odometerRecord = new OdometerRecord()
                    {
                        VehicleId = vehicleId,
                        Date = DateTime.Parse(input.Date),
                        Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                        Mileage = int.Parse(input.Odometer),
                        Files = StaticHelper.CreateAttachmentFromRecord(ImportMode.GasRecord, gasRecord.Id, $"Gas Record - {gasRecord.Mileage.ToString()}")
                    };
                    _odometerLogic.AutoInsertOdometerRecord(odometerRecord);
                }
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromGasRecord(gasRecord, "gasrecord.add.api", User.Identity.Name));
                return Json(OperationResponse.Succeed("Gas Record Added", new { recordId = gasRecord.Id }));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Delete })]
        [HttpDelete]
        [Route("/api/vehicle/gasrecords/delete")]
        public IActionResult DeleteGasRecord(int id)
        {
            var existingRecord = _gasRecordDataAccess.GetGasRecordById(id);
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
            var result = _gasRecordDataAccess.DeleteGasRecordById(existingRecord.Id);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromGasRecord(existingRecord, "gasrecord.delete.api", User.Identity.Name));
            }
            return Json(OperationResponse.Conditional(result, "Gas Record Deleted"));
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/gasrecords/update")]
        [Consumes("application/json")]
        public IActionResult UpdateGasRecordJson([FromBody] GasRecordExportModel input) => UpdateGasRecord(input);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/gasrecords/update")]
        public IActionResult UpdateGasRecord(GasRecordExportModel input)
        {
            if (string.IsNullOrWhiteSpace(input.Id) ||
                string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Odometer) ||
                string.IsNullOrWhiteSpace(input.FuelConsumed) ||
                string.IsNullOrWhiteSpace(input.Cost) ||
                string.IsNullOrWhiteSpace(input.IsFillToFull) ||
                string.IsNullOrWhiteSpace(input.MissedFuelUp))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Id, Date, Odometer, FuelConsumed, IsFillToFull, MissedFuelUp, and Cost cannot be empty."));
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
                var existingRecord = _gasRecordDataAccess.GetGasRecordById(int.Parse(input.Id));
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
                    existingRecord.Gallons = decimal.Parse(input.FuelConsumed);
                    existingRecord.IsFillToFull = bool.Parse(input.IsFillToFull);
                    existingRecord.MissedFuelUp = bool.Parse(input.MissedFuelUp);
                    existingRecord.Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes;
                    existingRecord.Cost = decimal.Parse(input.Cost);
                    existingRecord.ExtraFields = input.ExtraFields;
                    existingRecord.Files = input.Files;
                    existingRecord.Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList();
                    _gasRecordDataAccess.SaveGasRecordToVehicle(existingRecord);
                    StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromGasRecord(existingRecord, "gasrecord.update.api", User.Identity.Name));
                }
                else
                {
                    Response.StatusCode = 400;
                    return Json(OperationResponse.Failed("Invalid Record Id"));
                }
                return Json(OperationResponse.Succeed("Gas Record Updated"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
    }
}

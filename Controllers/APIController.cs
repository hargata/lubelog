using CarCareTracker.External.Interfaces;
using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Logic;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarCareTracker.Controllers
{
    [Authorize]
    public class APIController : Controller
    {
        private readonly IVehicleDataAccess _dataAccess;
        private readonly INoteDataAccess _noteDataAccess;
        private readonly IServiceRecordDataAccess _serviceRecordDataAccess;
        private readonly IGasRecordDataAccess _gasRecordDataAccess;
        private readonly ICollisionRecordDataAccess _collisionRecordDataAccess;
        private readonly ITaxRecordDataAccess _taxRecordDataAccess;
        private readonly IReminderRecordDataAccess _reminderRecordDataAccess;
        private readonly IUpgradeRecordDataAccess _upgradeRecordDataAccess;
        private readonly IOdometerRecordDataAccess _odometerRecordDataAccess;
        private readonly ISupplyRecordDataAccess _supplyRecordDataAccess;
        private readonly IPlanRecordDataAccess _planRecordDataAccess;
        private readonly IPlanRecordTemplateDataAccess _planRecordTemplateDataAccess;
        private readonly IUserAccessDataAccess _userAccessDataAccess;
        private readonly IUserRecordDataAccess _userRecordDataAccess;
        private readonly IReminderHelper _reminderHelper;
        private readonly IGasHelper _gasHelper;
        private readonly IUserLogic _userLogic;
        private readonly IVehicleLogic _vehicleLogic;
        private readonly IOdometerLogic _odometerLogic;
        private readonly IFileHelper _fileHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IConfigHelper _config;
        private readonly IWebHostEnvironment _webEnv;
        public APIController(IVehicleDataAccess dataAccess,
            IGasHelper gasHelper,
            IReminderHelper reminderHelper,
            INoteDataAccess noteDataAccess,
            IServiceRecordDataAccess serviceRecordDataAccess,
            IGasRecordDataAccess gasRecordDataAccess,
            ICollisionRecordDataAccess collisionRecordDataAccess,
            ITaxRecordDataAccess taxRecordDataAccess,
            IReminderRecordDataAccess reminderRecordDataAccess,
            IUpgradeRecordDataAccess upgradeRecordDataAccess,
            IOdometerRecordDataAccess odometerRecordDataAccess,
            ISupplyRecordDataAccess supplyRecordDataAccess,
            IPlanRecordDataAccess planRecordDataAccess,
            IPlanRecordTemplateDataAccess planRecordTemplateDataAccess,
            IUserAccessDataAccess userAccessDataAccess,
            IUserRecordDataAccess userRecordDataAccess,
            IMailHelper mailHelper,
            IFileHelper fileHelper,
            IConfigHelper config,
            IUserLogic userLogic,
            IVehicleLogic vehicleLogic,
            IOdometerLogic odometerLogic,
            IWebHostEnvironment webEnv) 
        {
            _dataAccess = dataAccess;
            _noteDataAccess = noteDataAccess;
            _serviceRecordDataAccess = serviceRecordDataAccess;
            _gasRecordDataAccess = gasRecordDataAccess;
            _collisionRecordDataAccess = collisionRecordDataAccess;
            _taxRecordDataAccess = taxRecordDataAccess;
            _reminderRecordDataAccess = reminderRecordDataAccess;
            _upgradeRecordDataAccess = upgradeRecordDataAccess;
            _odometerRecordDataAccess = odometerRecordDataAccess;
            _supplyRecordDataAccess = supplyRecordDataAccess;
            _planRecordDataAccess = planRecordDataAccess;
            _planRecordTemplateDataAccess = planRecordTemplateDataAccess;
            _userAccessDataAccess = userAccessDataAccess;
            _userRecordDataAccess = userRecordDataAccess;
            _mailHelper = mailHelper;
            _gasHelper = gasHelper;
            _reminderHelper = reminderHelper;
            _userLogic = userLogic;
            _odometerLogic = odometerLogic;
            _vehicleLogic = vehicleLogic;
            _fileHelper = fileHelper;
            _config = config;
            _webEnv = webEnv;
        }
        public IActionResult Index()
        {
            return View();
        }
        private int GetUserID()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
        [HttpGet]
        [Route("/api/vehicles")]
        public IActionResult Vehicles()
        {
            var result = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                result = _userLogic.FilterUserVehicles(result, GetUserID());
            }
            return Json(result);
        }

        [HttpGet]
        [Route("/api/vehicle/info")]
        public IActionResult VehicleInfo(int vehicleId)
        {
            //stats for a specific or all vehicles
            List<Vehicle> vehicles = new List<Vehicle>();
            if (vehicleId != default)
            {
                if (_userLogic.UserCanEditVehicle(GetUserID(), vehicleId))
                {
                    vehicles.Add(_dataAccess.GetVehicleById(vehicleId));
                } else
                {
                    return new RedirectResult("/Error/Unauthorized");
                }
            } else
            {
                var result = _dataAccess.GetVehicles();
                if (!User.IsInRole(nameof(UserData.IsRootUser)))
                {
                    result = _userLogic.FilterUserVehicles(result, GetUserID());
                }
                vehicles.AddRange(result);
            }

            var apiResult = _vehicleLogic.GetVehicleInfo(vehicles);
            if (_config.GetInvariantApi() || Request.Headers.ContainsKey("culture-invariant"))
            {
                return Json(apiResult, StaticHelper.GetInvariantOption());
            }
            else
            {
                return Json(apiResult);
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/adjustedodometer")]
        public IActionResult AdjustedOdometer(int vehicleId, int odometer)
        {
            var vehicle = _dataAccess.GetVehicleById(vehicleId);
            if (vehicle == null || !vehicle.HasOdometerAdjustment)
            {
                return Json(odometer);
            } else
            {
                var convertedOdometer = (odometer + int.Parse(vehicle.OdometerDifference)) * decimal.Parse(vehicle.OdometerMultiplier);
                return Json(convertedOdometer);
            }
        }
        #region ServiceRecord
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/servicerecords")]
        public IActionResult ServiceRecords(int vehicleId)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
            var result = vehicleRecords.Select(x => new GenericRecordExportModel { Id = x.Id.ToString(), Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString(), Notes = x.Notes, Odometer = x.Mileage.ToString(), ExtraFields = x.ExtraFields, Files = x.Files, Tags = string.Join(' ', x.Tags) });
            if (_config.GetInvariantApi() || Request.Headers.ContainsKey("culture-invariant"))
            {
                return Json(result, StaticHelper.GetInvariantOption());
            } else
            {
                return Json(result);
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/vehicle/servicerecords/add")]
        [Consumes("application/json")]
        public IActionResult AddServiceRecordJson(int vehicleId, [FromBody] GenericRecordExportModel input) => AddServiceRecord(vehicleId, input);
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/vehicle/servicerecords/add")]
        public IActionResult AddServiceRecord(int vehicleId, GenericRecordExportModel input)
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
            try
            {
                var serviceRecord = new ServiceRecord()
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
                _serviceRecordDataAccess.SaveServiceRecordToVehicle(serviceRecord);
                if (_config.GetUserConfig(User).EnableAutoOdometerInsert)
                {
                    var odometerRecord = new OdometerRecord()
                    {
                        VehicleId = vehicleId,
                        Date = DateTime.Parse(input.Date),
                        Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                        Mileage = int.Parse(input.Odometer)
                    };
                    _odometerLogic.AutoInsertOdometerRecord(odometerRecord);
                }
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromGenericRecord(serviceRecord, "servicerecord.add.api", User.Identity.Name));
                return Json(OperationResponse.Succeed("Service Record Added"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [HttpDelete]
        [Route("/api/vehicle/servicerecords/delete")]
        public IActionResult DeleteServiceRecord(int id)
        {
            var existingRecord = _serviceRecordDataAccess.GetServiceRecordById(id);
            if (existingRecord == null || existingRecord.Id == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Invalid Record Id"));
            }
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
            {
                Response.StatusCode = 401;
                return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
            }
            //restore any requisitioned supplies.
            if (existingRecord.RequisitionHistory.Any())
            {
                _vehicleLogic.RestoreSupplyRecordsByUsage(existingRecord.RequisitionHistory, existingRecord.Description);
            }
            var result = _serviceRecordDataAccess.DeleteServiceRecordById(existingRecord.Id);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromGenericRecord(existingRecord, "servicerecord.delete.api", User.Identity.Name));
            }
            return Json(OperationResponse.Conditional(result, "Service Record Deleted"));
        }
        [HttpPut]
        [Route("/api/vehicle/servicerecords/update")]
        [Consumes("application/json")]
        public IActionResult UpdateServiceRecordJson([FromBody] GenericRecordExportModel input) => UpdateServiceRecord(input);
        [HttpPut]
        [Route("/api/vehicle/servicerecords/update")]
        public IActionResult UpdateServiceRecord(GenericRecordExportModel input)
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
            try
            {
                //retrieve existing record
                var existingRecord = _serviceRecordDataAccess.GetServiceRecordById(int.Parse(input.Id));
                if (existingRecord != null && existingRecord.Id == int.Parse(input.Id))
                {
                    //check if user has access to the vehicleId
                    if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
                    {
                        Response.StatusCode = 401;
                        return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
                    }
                    existingRecord.Date = DateTime.Parse(input.Date);
                    existingRecord.Mileage = int.Parse(input.Odometer);
                    existingRecord.Description = input.Description;
                    existingRecord.Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes;
                    existingRecord.Cost = decimal.Parse(input.Cost);
                    existingRecord.Files = input.Files;
                    existingRecord.ExtraFields = input.ExtraFields;
                    existingRecord.Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList();
                    _serviceRecordDataAccess.SaveServiceRecordToVehicle(existingRecord);
                    StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromGenericRecord(existingRecord, "servicerecord.update.api", User.Identity.Name));
                } else
                {
                    Response.StatusCode = 400;
                    return Json(OperationResponse.Failed("Invalid Record Id"));
                }
                return Json(OperationResponse.Succeed("Service Record Updated"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        #endregion
        #region RepairRecord
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/repairrecords")]
        public IActionResult RepairRecords(int vehicleId)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
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
        [HttpPost]
        [Route("/api/vehicle/repairrecords/add")]
        [Consumes("application/json")]
        public IActionResult AddRepairRecordJson(int vehicleId, [FromBody] GenericRecordExportModel input) => AddRepairRecord(vehicleId, input);
        [TypeFilter(typeof(CollaboratorFilter))]
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
                        Mileage = int.Parse(input.Odometer)
                    };
                    _odometerLogic.AutoInsertOdometerRecord(odometerRecord);
                }
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromGenericRecord(repairRecord, "repairrecord.add.api", User.Identity.Name));

                return Json(OperationResponse.Succeed("Repair Record Added"));
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
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
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
            try
            {
                //retrieve existing record
                var existingRecord = _collisionRecordDataAccess.GetCollisionRecordById(int.Parse(input.Id));
                if (existingRecord != null && existingRecord.Id == int.Parse(input.Id))
                {
                    //check if user has access to the vehicleId
                    if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
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
        #endregion
        #region UpgradeRecord
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/upgraderecords")]
        public IActionResult UpgradeRecords(int vehicleId)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
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
        [HttpPost]
        [Route("/api/vehicle/upgraderecords/add")]
        [Consumes("application/json")]
        public IActionResult AddUpgradeRecordJson(int vehicleId, [FromBody] GenericRecordExportModel input) => AddUpgradeRecord(vehicleId, input);
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/vehicle/upgraderecords/add")]
        public IActionResult AddUpgradeRecord(int vehicleId, GenericRecordExportModel input)
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
            try
            {
                var upgradeRecord = new UpgradeRecord()
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
                _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(upgradeRecord);
                if (_config.GetUserConfig(User).EnableAutoOdometerInsert)
                {
                    var odometerRecord = new OdometerRecord()
                    {
                        VehicleId = vehicleId,
                        Date = DateTime.Parse(input.Date),
                        Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                        Mileage = int.Parse(input.Odometer)
                    };
                    _odometerLogic.AutoInsertOdometerRecord(odometerRecord);
                }
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromGenericRecord(upgradeRecord, "upgraderecord.add.api", User.Identity.Name));
                return Json(OperationResponse.Succeed("Upgrade Record Added"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [HttpDelete]
        [Route("/api/vehicle/upgraderecords/delete")]
        public IActionResult DeleteUpgradeRecord(int id)
        {
            var existingRecord = _upgradeRecordDataAccess.GetUpgradeRecordById(id);
            if (existingRecord == null || existingRecord.Id == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Invalid Record Id"));
            }
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
            {
                Response.StatusCode = 401;
                return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
            }
            //restore any requisitioned supplies.
            if (existingRecord.RequisitionHistory.Any())
            {
                _vehicleLogic.RestoreSupplyRecordsByUsage(existingRecord.RequisitionHistory, existingRecord.Description);
            }
            var result = _upgradeRecordDataAccess.DeleteUpgradeRecordById(existingRecord.Id);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromGenericRecord(existingRecord, "upgraderecord.delete.api", User.Identity.Name));
            }
            return Json(OperationResponse.Conditional(result,"Upgrade Record Deleted"));
        }
        [HttpPut]
        [Route("/api/vehicle/upgraderecords/update")]
        [Consumes("application/json")]
        public IActionResult UpdateUpgradeRecordJson([FromBody] GenericRecordExportModel input) => UpdateUpgradeRecord(input);
        [HttpPut]
        [Route("/api/vehicle/upgraderecords/update")]
        public IActionResult UpdateUpgradeRecord(GenericRecordExportModel input)
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
            try
            {
                //retrieve existing record
                var existingRecord = _upgradeRecordDataAccess.GetUpgradeRecordById(int.Parse(input.Id));
                if (existingRecord != null && existingRecord.Id == int.Parse(input.Id))
                {
                    //check if user has access to the vehicleId
                    if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
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
                    _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(existingRecord);
                    StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromGenericRecord(existingRecord, "upgraderecord.update.api", User.Identity.Name));
                }
                else
                {
                    Response.StatusCode = 400;
                    return Json(OperationResponse.Failed("Invalid Record Id"));
                }
                return Json(OperationResponse.Succeed("Upgrade Record Updated"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        #endregion
        #region TaxRecord
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/taxrecords")]
        public IActionResult TaxRecords(int vehicleId)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var result = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId).Select(x => new TaxRecordExportModel { Id = x.Id.ToString(), Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString(), Notes = x.Notes, ExtraFields = x.ExtraFields, Files = x.Files, Tags = string.Join(' ', x.Tags) });
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
        [Route("/api/vehicle/taxrecords/check")]
        public IActionResult CheckRecurringTaxRecords()
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
                foreach(Vehicle vehicle in vehicles)
                {
                    var updateResult = _vehicleLogic.UpdateRecurringTaxes(vehicle.Id);
                    if (updateResult)
                    {
                        vehiclesUpdated++;
                    }
                }
                if (vehiclesUpdated != default)
                {
                    return Json(OperationResponse.Succeed($"Recurring Taxes for {vehiclesUpdated} Vehicles Updated!"));
                } else
                {
                    return Json(OperationResponse.Succeed("No Recurring Taxes Updated"));
                }
            }
            catch (Exception ex)
            {
                return Json(OperationResponse.Failed($"No Recurring Taxes Updated Due To Error: {ex.Message}"));
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/vehicle/taxrecords/add")]
        [Consumes("application/json")]
        public IActionResult AddTaxRecordJson(int vehicleId, [FromBody] TaxRecordExportModel input) => AddTaxRecord(vehicleId, input);
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/vehicle/taxrecords/add")]
        public IActionResult AddTaxRecord(int vehicleId, TaxRecordExportModel input)
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
            try
            {
                var taxRecord = new TaxRecord()
                {
                    VehicleId = vehicleId,
                    Date = DateTime.Parse(input.Date),
                    Description = input.Description,
                    Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                    Cost = decimal.Parse(input.Cost),
                    ExtraFields = input.ExtraFields,
                    Files = input.Files,
                    Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList()
                };
                _taxRecordDataAccess.SaveTaxRecordToVehicle(taxRecord);
                _vehicleLogic.UpdateRecurringTaxes(vehicleId);
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromTaxRecord(taxRecord, "taxrecord.add.api", User.Identity.Name));
                return Json(OperationResponse.Succeed("Tax Record Added"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [HttpDelete]
        [Route("/api/vehicle/taxrecords/delete")]
        public IActionResult DeleteTaxRecord(int id)
        {
            var existingRecord = _taxRecordDataAccess.GetTaxRecordById(id);
            if (existingRecord == null || existingRecord.Id == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Invalid Record Id"));
            }
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
            {
                Response.StatusCode = 401;
                return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
            }
            var result = _taxRecordDataAccess.DeleteTaxRecordById(existingRecord.Id);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromTaxRecord(existingRecord, "taxrecord.delete.api", User.Identity.Name));
            }
            return Json(OperationResponse.Conditional(result, "Tax Record Deleted"));
        }
        [HttpPut]
        [Route("/api/vehicle/taxrecords/update")]
        [Consumes("application/json")]
        public IActionResult UpdateTaxRecordJson([FromBody] TaxRecordExportModel input) => UpdateTaxRecord(input);
        [HttpPut]
        [Route("/api/vehicle/taxrecords/update")]
        public IActionResult UpdateTaxRecord(TaxRecordExportModel input)
        {
            if (string.IsNullOrWhiteSpace(input.Id) ||
                string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.Cost))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Id, Date, Description, and Cost cannot be empty."));
            }
            try
            {
                //retrieve existing record
                var existingRecord = _taxRecordDataAccess.GetTaxRecordById(int.Parse(input.Id));
                if (existingRecord != null && existingRecord.Id == int.Parse(input.Id))
                {
                    //check if user has access to the vehicleId
                    if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
                    {
                        Response.StatusCode = 401;
                        return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
                    }
                    existingRecord.Date = DateTime.Parse(input.Date);
                    existingRecord.Description = input.Description;
                    existingRecord.Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes;
                    existingRecord.Cost = decimal.Parse(input.Cost);
                    existingRecord.ExtraFields = input.ExtraFields;
                    existingRecord.Files = input.Files;
                    existingRecord.Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList();
                    _taxRecordDataAccess.SaveTaxRecordToVehicle(existingRecord);
                    StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromTaxRecord(existingRecord, "taxrecord.update.api", User.Identity.Name));
                }
                else
                {
                    Response.StatusCode = 400;
                    return Json(OperationResponse.Failed("Invalid Record Id"));
                }
                return Json(OperationResponse.Succeed("Tax Record Updated"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        #endregion
        #region OdometerRecord
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
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/odometerrecords")]
        public IActionResult OdometerRecords(int vehicleId)
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
            var result = vehicleRecords.Select(x => new OdometerRecordExportModel { Id = x.Id.ToString(), Date = x.Date.ToShortDateString(), InitialOdometer = x.InitialMileage.ToString(), Odometer = x.Mileage.ToString(), Notes = x.Notes, ExtraFields = x.ExtraFields, Files = x.Files, Tags = string.Join(' ', x.Tags) });
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
        [HttpPost]
        [Route("/api/vehicle/odometerrecords/add")]
        [Consumes("application/json")]
        public IActionResult AddOdometerRecordJson(int vehicleId, [FromBody] OdometerRecordExportModel input) => AddOdometerRecord(vehicleId, input);
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/vehicle/odometerrecords/add")]
        public IActionResult AddOdometerRecord(int vehicleId, OdometerRecordExportModel input)
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
                    Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList()
                };
                _odometerRecordDataAccess.SaveOdometerRecordToVehicle(odometerRecord);
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromOdometerRecord(odometerRecord, "odometerrecord.add.api", User.Identity.Name));
                return Json(OperationResponse.Succeed("Odometer Record Added"));
            } catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
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
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
            {
                Response.StatusCode = 401;
                return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
            }
            var result = _odometerRecordDataAccess.DeleteOdometerRecordById(existingRecord.Id);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromOdometerRecord(existingRecord, "odometerrecord.delete.api", User.Identity.Name));
            }
            return Json(OperationResponse.Conditional(result, "Odometer Record Deleted"));
        }
        [HttpPut]
        [Route("/api/vehicle/odometerrecords/update")]
        [Consumes("application/json")]
        public IActionResult UpdateOdometerRecordJson([FromBody] OdometerRecordExportModel input) => UpdateOdometerRecord(input);
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
            try
            {
                //retrieve existing record
                var existingRecord = _odometerRecordDataAccess.GetOdometerRecordById(int.Parse(input.Id));
                if (existingRecord != null && existingRecord.Id == int.Parse(input.Id))
                {
                    //check if user has access to the vehicleId
                    if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
                    {
                        Response.StatusCode = 401;
                        return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
                    }
                    existingRecord.Date = DateTime.Parse(input.Date);
                    existingRecord.Mileage = int.Parse(input.Odometer);
                    existingRecord.InitialMileage = int.Parse(input.InitialOdometer);
                    existingRecord.Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes;
                    existingRecord.ExtraFields = input.ExtraFields;
                    existingRecord.Files = input.Files;
                    existingRecord.Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList();
                    _odometerRecordDataAccess.SaveOdometerRecordToVehicle(existingRecord);
                    StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromOdometerRecord(existingRecord, "odometerrecord.update.api", User.Identity.Name));
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
        #endregion
        #region GasRecord
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/gasrecords")]
        public IActionResult GasRecords(int vehicleId, bool useMPG, bool useUKMPG)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            var result = _gasHelper.GetGasRecordViewModels(vehicleRecords, useMPG, useUKMPG)
                .Select(x => new GasRecordExportModel { 
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
        [HttpPost]
        [Route("/api/vehicle/gasrecords/add")]
        [Consumes("application/json")]
        public IActionResult AddGasRecordJson(int vehicleId, [FromBody] GasRecordExportModel input) => AddGasRecord(vehicleId, input);
        [TypeFilter(typeof(CollaboratorFilter))]
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
                        Mileage = int.Parse(input.Odometer)
                    };
                    _odometerLogic.AutoInsertOdometerRecord(odometerRecord);
                }
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromGasRecord(gasRecord, "gasrecord.add.api", User.Identity.Name));
                return Json(OperationResponse.Succeed("Gas Record Added"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
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
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
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
        [HttpPut]
        [Route("/api/vehicle/gasrecords/update")]
        [Consumes("application/json")]
        public IActionResult UpdateGasRecordJson([FromBody] GasRecordExportModel input) => UpdateGasRecord(input);
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
            try
            {
                //retrieve existing record
                var existingRecord = _gasRecordDataAccess.GetGasRecordById(int.Parse(input.Id));
                if (existingRecord != null && existingRecord.Id == int.Parse(input.Id))
                {
                    //check if user has access to the vehicleId
                    if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
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
        #endregion
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/reminders")]
        public IActionResult Reminders(int vehicleId)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            var currentMileage = _vehicleLogic.GetMaxMileage(vehicleId);
            var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
            var results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, DateTime.Now).Select(x=> new ReminderExportModel {  Description = x.Description, Urgency = x.Urgency.ToString(), Metric = x.Metric.ToString(), Notes = x.Notes, DueDate = x.Date.ToShortDateString(), DueOdometer = x.Mileage.ToString()});
            if (_config.GetInvariantApi() || Request.Headers.ContainsKey("culture-invariant"))
            {
                return Json(results, StaticHelper.GetInvariantOption());
            }
            else
            {
                return Json(results);
            }
        }
        [HttpGet]
        [Route("/api/calendar")]
        public IActionResult Calendar()
        {
            var vehiclesStored = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehiclesStored = _userLogic.FilterUserVehicles(vehiclesStored, GetUserID());
            }
            var reminders = _vehicleLogic.GetReminders(vehiclesStored, true);
            var calendarContent = StaticHelper.RemindersToCalendar(reminders);
            return File(calendarContent, "text/calendar");
        }
        [HttpPost]
        [Route("/api/documents/upload")]
        public IActionResult UploadDocument(List<IFormFile> documents)
        {
            if (documents.Any())
            {
                List<UploadedFiles> uploadedFiles = new List<UploadedFiles>();
                string uploadDirectory = "documents/";
                string uploadPath = Path.Combine(_webEnv.ContentRootPath, "data", uploadDirectory);
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);
                foreach (IFormFile document in documents)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(document.FileName);
                    string filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        document.CopyTo(stream);
                    }
                    uploadedFiles.Add(new UploadedFiles
                    {
                        Location = Path.Combine("/", uploadDirectory, fileName),
                        Name = Path.GetFileName(document.FileName)
                    });
                }
                return Json(uploadedFiles);
            } else
            {
                return Json(OperationResponse.Failed("No files to upload"));
            }
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        [Route("/api/vehicle/reminders/send")]
        public IActionResult SendReminders(List<ReminderUrgency> urgencies)
        {
            var vehicles = _dataAccess.GetVehicles();
            List<OperationResponse> operationResponses = new List<OperationResponse>();
            var defaultEmailAddress = _config.GetUserConfig(User).DefaultReminderEmail;
            foreach(Vehicle vehicle in vehicles)
            {
                var vehicleId = vehicle.Id;
                //get reminders
                var currentMileage = _vehicleLogic.GetMaxMileage(vehicleId);
                var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
                var results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, DateTime.Now).OrderByDescending(x => x.Urgency).ToList();
                results.RemoveAll(x => !urgencies.Contains(x.Urgency));
                if (!results.Any())
                {
                    continue;
                }
                //get list of recipients.
                var userIds = _userAccessDataAccess.GetUserAccessByVehicleId(vehicleId).Select(x => x.Id.UserId);
                List<string> emailRecipients = new List<string>();
                if (!string.IsNullOrWhiteSpace(defaultEmailAddress))
                {
                    emailRecipients.Add(defaultEmailAddress);
                }
                foreach (int userId in userIds)
                {
                    var userData = _userRecordDataAccess.GetUserRecordById(userId);
                    emailRecipients.Add(userData.EmailAddress);
                };
                if (!emailRecipients.Any())
                {
                    continue;
                }
                var result = _mailHelper.NotifyUserForReminders(vehicle, emailRecipients, results);
                operationResponses.Add(result);
            }
            if (!operationResponses.Any())
            {
                return Json(OperationResponse.Failed("No Emails Sent, No Vehicles Available or No Recipients Configured"));
            }
            else if (operationResponses.All(x => x.Success))
            {
                return Json(OperationResponse.Succeed($"Emails Sent({operationResponses.Count()})"));
            } else if (operationResponses.All(x => !x.Success))
            {
                return Json(OperationResponse.Failed($"All Emails Failed({operationResponses.Count()}), Check SMTP Settings"));
            } else
            {
                return Json(OperationResponse.Succeed($"Emails Sent({operationResponses.Count(x => x.Success)}), Emails Failed({operationResponses.Count(x => !x.Success)}), Check Recipient Settings"));
            }
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        [Route("/api/makebackup")]
        public IActionResult MakeBackup()
        {
            var result = _fileHelper.MakeBackup();
            return Json(result);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        [Route("/api/cleanup")]
        public IActionResult CleanUp(bool deepClean = false)
        {
            var jsonResponse = new Dictionary<string, string>();
            //Clear out temp folder
            var tempFilesDeleted = _fileHelper.ClearTempFolder();
            jsonResponse.Add("temp_files_deleted", tempFilesDeleted.ToString());
            if (deepClean)
            {
                //clear out unused vehicle thumbnails
                var vehicles = _dataAccess.GetVehicles();
                var vehicleImages = vehicles.Select(x => x.ImageLocation).Where(x => x.StartsWith("/images/")).Select(x=>Path.GetFileName(x)).ToList();
                if (vehicleImages.Any())
                {
                    var thumbnailsDeleted = _fileHelper.ClearUnlinkedThumbnails(vehicleImages);
                    jsonResponse.Add("unlinked_thumbnails_deleted", thumbnailsDeleted.ToString());
                }
                var vehicleDocuments = new List<string>();
                foreach(Vehicle vehicle in vehicles)
                {
                    vehicleDocuments.AddRange(_serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y=>Path.GetFileName(y.Location)));
                    vehicleDocuments.AddRange(_collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                    vehicleDocuments.AddRange(_upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                    vehicleDocuments.AddRange(_taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                    vehicleDocuments.AddRange(_gasRecordDataAccess.GetGasRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                    vehicleDocuments.AddRange(_noteDataAccess.GetNotesByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                    vehicleDocuments.AddRange(_odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                    vehicleDocuments.AddRange(_supplyRecordDataAccess.GetSupplyRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                    vehicleDocuments.AddRange(_planRecordDataAccess.GetPlanRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                    vehicleDocuments.AddRange(_planRecordTemplateDataAccess.GetPlanRecordTemplatesByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                }
                //shop supplies
                vehicleDocuments.AddRange(_supplyRecordDataAccess.GetSupplyRecordsByVehicleId(0).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                if (vehicleDocuments.Any())
                {
                    var documentsDeleted = _fileHelper.ClearUnlinkedDocuments(vehicleDocuments);
                    jsonResponse.Add("unlinked_documents_deleted", documentsDeleted.ToString());
                }
            }
            return Json(jsonResponse);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        [Route("/api/demo/restore")]
        public IActionResult RestoreDemo()
        {
            var result = _fileHelper.RestoreBackup("/defaults/demo_default.zip", true);
            return Json(result);
        }
    }
}

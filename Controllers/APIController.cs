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
            IOdometerLogic odometerLogic) 
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

            List<VehicleInfo> apiResult = new List<VehicleInfo>();

            foreach(Vehicle vehicle in vehicles)
            {
                var currentMileage = _vehicleLogic.GetMaxMileage(vehicle.Id);
                var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicle.Id);
                var results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, DateTime.Now);

                var serviceRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicle.Id);
                var repairRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicle.Id);
                var upgradeRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicle.Id);
                var gasRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicle.Id);
                var taxRecords = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicle.Id);

                var resultToAdd = new VehicleInfo()
                {
                    VehicleData = vehicle,
                    LastReportedOdometer = currentMileage,
                    ServiceRecordCount = serviceRecords.Count(),
                    ServiceRecordCost = serviceRecords.Sum(x=>x.Cost),
                    RepairRecordCount = repairRecords.Count(),
                    RepairRecordCost = repairRecords.Sum(x=>x.Cost),
                    UpgradeRecordCount = upgradeRecords.Count(),
                    UpgradeRecordCost = upgradeRecords.Sum(x=>x.Cost),
                    GasRecordCount = gasRecords.Count(),
                    GasRecordCost = gasRecords.Sum(x=>x.Cost),
                    TaxRecordCount = taxRecords.Count(),
                    TaxRecordCost = taxRecords.Sum(x=> x.Cost),
                    VeryUrgentReminderCount = results.Count(x => x.Urgency == ReminderUrgency.VeryUrgent),
                    PastDueReminderCount = results.Count(x => x.Urgency == ReminderUrgency.PastDue),
                    UrgentReminderCount = results.Count(x => x.Urgency == ReminderUrgency.Urgent),
                    NotUrgentReminderCount = results.Count(x => x.Urgency == ReminderUrgency.NotUrgent)
                };
                //set next reminder
                if (results.Any(x => (x.Metric == ReminderMetric.Date || x.Metric == ReminderMetric.Both) && x.Date >= DateTime.Now.Date))
                {
                    resultToAdd.NextReminder = results.Where(x => x.Date >= DateTime.Now.Date).OrderBy(x => x.Date).Select(x => new ReminderExportModel { Description = x.Description, Urgency = x.Urgency.ToString(), Metric = x.Metric.ToString(), Notes = x.Notes, DueDate = x.Date.ToShortDateString(), DueOdometer = x.Mileage.ToString() }).First();
                }
                else if (results.Any(x => (x.Metric == ReminderMetric.Odometer || x.Metric == ReminderMetric.Both) && x.Mileage >= currentMileage))
                {
                    resultToAdd.NextReminder = results.Where(x => x.Mileage >= currentMileage).OrderBy(x => x.Mileage).Select(x => new ReminderExportModel { Description = x.Description, Urgency = x.Urgency.ToString(), Metric = x.Metric.ToString(), Notes = x.Notes, DueDate = x.Date.ToShortDateString(), DueOdometer = x.Mileage.ToString() }).First();
                }
                apiResult.Add(resultToAdd);
            }
            return Json(apiResult);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/servicerecords")]
        public IActionResult ServiceRecords(int vehicleId)
        {
            if (vehicleId == default)
            {
                var response = new OperationResponse();
                response.Success = false;
                response.Message = "Must provide a valid vehicle id";
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
            var result = vehicleRecords.Select(x => new GenericRecordExportModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString(), Notes = x.Notes, Odometer = x.Mileage.ToString(), ExtraFields = x.ExtraFields });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/vehicle/servicerecords/add")]
        public IActionResult AddServiceRecord(int vehicleId, GenericRecordExportModel input)
        {
            var response = new OperationResponse();
            if (vehicleId == default)
            {
                response.Success = false;
                response.Message = "Must provide a valid vehicle id";
                Response.StatusCode = 400;
                return Json(response);
            }
            if (string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.Odometer) ||
                string.IsNullOrWhiteSpace(input.Cost))
            {
                response.Success = false;
                response.Message = "Input object invalid, Date, Description, Odometer, and Cost cannot be empty.";
                Response.StatusCode = 400;
                return Json(response);
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
                    ExtraFields = input.ExtraFields
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
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), vehicleId, User.Identity.Name, $"Added Service Record via API - Description: {serviceRecord.Description}");
                response.Success = true;
                response.Message = "Service Record Added";
                return Json(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                Response.StatusCode = 500;
                return Json(response);
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/repairrecords")]
        public IActionResult RepairRecords(int vehicleId)
        {
            if (vehicleId == default)
            {
                var response = new OperationResponse();
                response.Success = false;
                response.Message = "Must provide a valid vehicle id";
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
            var result = vehicleRecords.Select(x => new GenericRecordExportModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString(), Notes = x.Notes, Odometer = x.Mileage.ToString(), ExtraFields = x.ExtraFields });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/vehicle/repairrecords/add")]
        public IActionResult AddRepairRecord(int vehicleId, GenericRecordExportModel input)
        {
            var response = new OperationResponse();
            if (vehicleId == default)
            {
                response.Success = false;
                response.Message = "Must provide a valid vehicle id";
                Response.StatusCode = 400;
                return Json(response);
            }
            if (string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.Odometer) ||
                string.IsNullOrWhiteSpace(input.Cost))
            {
                response.Success = false;
                response.Message = "Input object invalid, Date, Description, Odometer, and Cost cannot be empty.";
                Response.StatusCode = 400;
                return Json(response);
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
                    ExtraFields = input.ExtraFields
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
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), vehicleId, User.Identity.Name, $"Added Repair Record via API - Description: {repairRecord.Description}");
                response.Success = true;
                response.Message = "Repair Record Added";
                return Json(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                Response.StatusCode = 500;
                return Json(response);
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/upgraderecords")]
        public IActionResult UpgradeRecords(int vehicleId)
        {
            if (vehicleId == default)
            {
                var response = new OperationResponse();
                response.Success = false;
                response.Message = "Must provide a valid vehicle id";
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
            var result = vehicleRecords.Select(x => new GenericRecordExportModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString(), Notes = x.Notes, Odometer = x.Mileage.ToString(), ExtraFields = x.ExtraFields });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/vehicle/upgraderecords/add")]
        public IActionResult AddUpgradeRecord(int vehicleId, GenericRecordExportModel input)
        {
            var response = new OperationResponse();
            if (vehicleId == default)
            {
                response.Success = false;
                response.Message = "Must provide a valid vehicle id";
                Response.StatusCode = 400;
                return Json(response);
            }
            if (string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.Odometer) ||
                string.IsNullOrWhiteSpace(input.Cost))
            {
                response.Success = false;
                response.Message = "Input object invalid, Date, Description, Odometer, and Cost cannot be empty.";
                Response.StatusCode = 400;
                return Json(response);
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
                    ExtraFields = input.ExtraFields
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
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), vehicleId, User.Identity.Name, $"Added Upgrade Record via API - Description: {upgradeRecord.Description}");
                response.Success = true;
                response.Message = "Upgrade Record Added";
                return Json(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                Response.StatusCode = 500;
                return Json(response);
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/taxrecords")]
        public IActionResult TaxRecords(int vehicleId)
        {
            if (vehicleId == default)
            {
                var response = new OperationResponse();
                response.Success = false;
                response.Message = "Must provide a valid vehicle id";
                Response.StatusCode = 400;
                return Json(response);
            }
            var result = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId).Select(x => new TaxRecordExportModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString(), Notes = x.Notes, ExtraFields = x.ExtraFields });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/vehicle/taxrecords/add")]
        public IActionResult AddTaxRecord(int vehicleId, TaxRecordExportModel input)
        {
            var response = new OperationResponse();
            if (vehicleId == default)
            {
                response.Success = false;
                response.Message = "Must provide a valid vehicle id";
                Response.StatusCode = 400;
                return Json(response);
            }
            if (string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.Cost))
            {
                response.Success = false;
                response.Message = "Input object invalid, Date, Description, and Cost cannot be empty.";
                Response.StatusCode = 400;
                return Json(response);
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
                    ExtraFields = input.ExtraFields
                };
                _taxRecordDataAccess.SaveTaxRecordToVehicle(taxRecord);
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), vehicleId, User.Identity.Name, $"Added Tax Record via API - Description: {taxRecord.Description}");
                response.Success = true;
                response.Message = "Tax Record Added";
                return Json(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                Response.StatusCode = 500;
                return Json(response);
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/odometerrecords/latest")]
        public IActionResult LastOdometer(int vehicleId)
        {
            if (vehicleId == default)
            {
                var response = new OperationResponse();
                response.Success = false;
                response.Message = "Must provide a valid vehicle id";
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
                var response = new OperationResponse();
                response.Success = false;
                response.Message = "Must provide a valid vehicle id";
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
            //determine if conversion is needed.
            if (vehicleRecords.All(x => x.InitialMileage == default))
            {
                vehicleRecords = _odometerLogic.AutoConvertOdometerRecord(vehicleRecords);
            }
            var result = vehicleRecords.Select(x => new OdometerRecordExportModel { Date = x.Date.ToShortDateString(), InitialOdometer = x.InitialMileage.ToString(), Odometer = x.Mileage.ToString(), Notes = x.Notes, ExtraFields = x.ExtraFields });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/vehicle/odometerrecords/add")]
        public IActionResult AddOdometerRecord(int vehicleId, OdometerRecordExportModel input)
        {
            var response = new OperationResponse();
            if (vehicleId == default)
            {
                response.Success = false;
                response.Message = "Must provide a valid vehicle id";
                Response.StatusCode = 400;
                return Json(response);
            }
            if (string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Odometer))
            {
                response.Success = false;
                response.Message = "Input object invalid, Date and Odometer cannot be empty.";
                Response.StatusCode = 400;
                return Json(response);
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
                    ExtraFields = input.ExtraFields
                };
                _odometerRecordDataAccess.SaveOdometerRecordToVehicle(odometerRecord);
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), vehicleId, User.Identity.Name, $"Added Odometer Record via API - Mileage: {odometerRecord.Mileage.ToString()}");
                response.Success = true;
                response.Message = "Odometer Record Added";
                return Json(response);
            } catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                Response.StatusCode = 500;
                return Json(response);
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/gasrecords")]
        public IActionResult GasRecords(int vehicleId, bool useMPG, bool useUKMPG)
        {
            if (vehicleId == default)
            {
                var response = new OperationResponse();
                response.Success = false;
                response.Message = "Must provide a valid vehicle id";
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            var result = _gasHelper.GetGasRecordViewModels(vehicleRecords, useMPG, useUKMPG)
                .Select(x => new GasRecordExportModel { 
                    Date = x.Date, 
                    Odometer = x.Mileage.ToString(), 
                    Cost = x.Cost.ToString(), 
                    FuelConsumed = x.Gallons.ToString(), 
                    FuelEconomy = x.MilesPerGallon.ToString(),
                    IsFillToFull = x.IsFillToFull.ToString(),
                    MissedFuelUp = x.MissedFuelUp.ToString(),
                    Notes = x.Notes,
                    ExtraFields = x.ExtraFields
                });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/vehicle/gasrecords/add")]
        public IActionResult AddGasRecord(int vehicleId, GasRecordExportModel input)
        {
            var response = new OperationResponse();
            if (vehicleId == default)
            {
                response.Success = false;
                response.Message = "Must provide a valid vehicle id";
                Response.StatusCode = 400;
                return Json(response);
            }
            if (string.IsNullOrWhiteSpace(input.Date) ||
                string.IsNullOrWhiteSpace(input.Odometer) ||
                string.IsNullOrWhiteSpace(input.FuelConsumed) ||
                string.IsNullOrWhiteSpace(input.Cost) ||
                string.IsNullOrWhiteSpace(input.IsFillToFull) ||
                string.IsNullOrWhiteSpace(input.MissedFuelUp)
                )
            {
                response.Success = false;
                response.Message = "Input object invalid, Date, Odometer, FuelConsumed, IsFillToFull, MissedFuelUp, and Cost cannot be empty.";
                Response.StatusCode = 400;
                return Json(response);
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
                    ExtraFields = input.ExtraFields
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
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), vehicleId, User.Identity.Name, $"Added Gas record via API - Mileage: {gasRecord.Mileage.ToString()}");
                response.Success = true;
                response.Message = "Gas Record Added";
                return Json(response);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                Response.StatusCode = 500;
                return Json(response);
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/reminders")]
        public IActionResult Reminders(int vehicleId)
        {
            if (vehicleId == default)
            {
                var response = new OperationResponse();
                response.Success = false;
                response.Message = "Must provide a valid vehicle id";
                Response.StatusCode = 400;
                return Json(response);
            }
            var currentMileage = _vehicleLogic.GetMaxMileage(vehicleId);
            var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
            var results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, DateTime.Now).Select(x=> new ReminderExportModel {  Description = x.Description, Urgency = x.Urgency.ToString(), Metric = x.Metric.ToString(), Notes = x.Notes, DueDate = x.Date.ToShortDateString(), DueOdometer = x.Mileage.ToString()});
            return Json(results);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        [Route("/api/vehicle/reminders/send")]
        public IActionResult SendReminders(List<ReminderUrgency> urgencies)
        {
            var vehicles = _dataAccess.GetVehicles();
            List<OperationResponse> operationResponses = new List<OperationResponse>();
            var defaultEmailAddress = _config.GetDefaultReminderEmail();
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
                return Json(new OperationResponse { Success = false, Message = "No Emails sent either because there are no vehicles or no recipients" });
            }
            else if (operationResponses.All(x => x.Success))
            {
                return Json(new OperationResponse { Success = true, Message = $"{operationResponses.Count()} Emails sent" });
            } else if (operationResponses.All(x => !x.Success))
            {
                return Json(new OperationResponse { Success = false, Message = "All emails failed, check SMTP settings" });
            } else
            {
                return Json(new OperationResponse { Success = true, Message = $"{operationResponses.Count(x=>x.Success)} Emails sent, {operationResponses.Count(x => !x.Success)} failed, check recipient settings" });
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

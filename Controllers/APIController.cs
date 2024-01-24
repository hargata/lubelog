using CarCareTracker.External.Implementations;
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
        private readonly ITorqueRecordDataAccess _torqueRecordDataAccess;
        private readonly IReminderHelper _reminderHelper;
        private readonly IGasHelper _gasHelper;
        private readonly IUserLogic _userLogic;
        private readonly IFileHelper _fileHelper;
        private readonly IConfigHelper _configHelper;
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
            ITorqueRecordDataAccess torqueRecordDataAccess,
            IConfigHelper configHelper,
            IFileHelper fileHelper,
            IUserLogic userLogic)
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
            _torqueRecordDataAccess = torqueRecordDataAccess;
            _gasHelper = gasHelper;
            _configHelper = configHelper;
            _reminderHelper = reminderHelper;
            _userLogic = userLogic;
            _fileHelper = fileHelper;
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
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/servicerecords")]
        public IActionResult ServiceRecords(int vehicleId)
        {
            var vehicleRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
            var result = vehicleRecords.Select(x => new ServiceRecordExportModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString(), Notes = x.Notes, Odometer = x.Mileage.ToString() });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/vehicle/servicerecords/add")]
        public IActionResult AddServiceRecord(int vehicleId, ServiceRecordExportModel input)
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
                    Cost = decimal.Parse(input.Cost)
                };
                _serviceRecordDataAccess.SaveServiceRecordToVehicle(serviceRecord);
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
            var vehicleRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
            var result = vehicleRecords.Select(x => new ServiceRecordExportModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString(), Notes = x.Notes, Odometer = x.Mileage.ToString() });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/vehicle/repairrecords/add")]
        public IActionResult AddRepairRecord(int vehicleId, ServiceRecordExportModel input)
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
                    Cost = decimal.Parse(input.Cost)
                };
                _collisionRecordDataAccess.SaveCollisionRecordToVehicle(repairRecord);
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
            var vehicleRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
            var result = vehicleRecords.Select(x => new ServiceRecordExportModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString(), Notes = x.Notes, Odometer = x.Mileage.ToString() });
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/vehicle/upgraderecords/add")]
        public IActionResult AddUpgradeRecord(int vehicleId, ServiceRecordExportModel input)
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
                    Cost = decimal.Parse(input.Cost)
                };
                _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(upgradeRecord);
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
            var result = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
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
                    Cost = decimal.Parse(input.Cost)
                };
                _taxRecordDataAccess.SaveTaxRecordToVehicle(taxRecord);
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
        [Route("/api/vehicle/odometerrecords")]
        public IActionResult OdometerRecords(int vehicleId)
        {
            var vehicleRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
            var result = vehicleRecords.Select(x => new OdometerRecordExportModel { Date = x.Date.ToShortDateString(), Odometer = x.Mileage.ToString(), Notes = x.Notes });
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
                    Mileage = int.Parse(input.Odometer)
                };
                _odometerRecordDataAccess.SaveOdometerRecordToVehicle(odometerRecord);
                response.Success = true;
                response.Message = "Odometer Record Added";
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
        [Route("/api/vehicle/gasrecords")]
        public IActionResult GasRecords(int vehicleId, bool useMPG, bool useUKMPG)
        {
            var vehicleRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            var result = _gasHelper.GetGasRecordViewModels(vehicleRecords, useMPG, useUKMPG)
                .Select(x => new GasRecordExportModel
                {
                    Date = x.Date,
                    Odometer = x.Mileage.ToString(),
                    Cost = x.Cost.ToString(),
                    FuelConsumed = x.Gallons.ToString(),
                    FuelEconomy = x.MilesPerGallon.ToString(),
                    IsFillToFull = x.IsFillToFull.ToString(),
                    MissedFuelUp = x.MissedFuelUp.ToString(),
                    Notes = x.Notes
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
                    Cost = decimal.Parse(input.Cost)
                };
                _gasRecordDataAccess.SaveGasRecordToVehicle(gasRecord);
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
            var currentMileage = GetMaxMileage(vehicleId);
            var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
            var results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, DateTime.Now).Select(x => new ReminderExportModel { Description = x.Description, Urgency = x.Urgency.ToString(), Metric = x.Metric.ToString(), Notes = x.Notes });
            return Json(results);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        [Route("/api/makebackup")]
        public IActionResult MakeBackup()
        {
            var result = _fileHelper.MakeBackup();
            return Json(result);
        }
        [Route("/api/obdii/vehicle/{vehicleId}")]
        [AllowAnonymous]
        public IActionResult OBDII(int vehicleId, TorqueRecord record)
        {
            if (record.kff1005 != default && record.kff1006 != default && vehicleId != default)
            {
                //check if there is an existing session.
                try
                {
                    var existingRecord = _torqueRecordDataAccess.GetTorqueRecordById(record.Session);
                    if (existingRecord != null)
                    {
                        //calculate difference between last coordinates.
                        var distance = GetDistance(existingRecord.LastLongitude, existingRecord.LastLatitude, record.kff1005, record.kff1006);
                        var useMPG = _configHelper.GetUserConfig(User).UseMPG;
                        if (useMPG)
                        {
                            distance /= 1609; //get miles.
                        }
                        else
                        {
                            distance /= 1000;
                        }
                        existingRecord.DistanceTraveled += distance;
                        existingRecord.LastLongitude = record.kff1005;
                        existingRecord.LastLatitude = record.kff1006;
                        _torqueRecordDataAccess.SaveTorqueRecord(existingRecord);
                    }
                    else
                    {
                        //new record.
                        record.InitialLongitude = record.kff1005;
                        record.InitialLatitude = record.kff1006;
                        record.LastLongitude = record.kff1005;
                        record.LastLatitude = record.kff1006;
                        _torqueRecordDataAccess.SaveTorqueRecord(record);
                    }
                    return Json(true);
                }
                catch (Exception ex)
                {
                    return Json(false);
                }
            }
            return Json(false);
        }
        private double GetDistance(double longitude, double latitude, double otherLongitude, double otherLatitude)
        {
            var d1 = latitude * (Math.PI / 180.0);
            var num1 = longitude * (Math.PI / 180.0);
            var d2 = otherLatitude * (Math.PI / 180.0);
            var num2 = otherLongitude * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }
        private int GetMaxMileage(int vehicleId)
        {
            var numbersArray = new List<int>();
            var serviceRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
            if (serviceRecords.Any())
            {
                numbersArray.Add(serviceRecords.Max(x => x.Mileage));
            }
            var repairRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
            if (repairRecords.Any())
            {
                numbersArray.Add(repairRecords.Max(x => x.Mileage));
            }
            var gasRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            if (gasRecords.Any())
            {
                numbersArray.Add(gasRecords.Max(x => x.Mileage));
            }
            var upgradeRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
            if (upgradeRecords.Any())
            {
                numbersArray.Add(upgradeRecords.Max(x => x.Mileage));
            }
            var odometerRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
            if (odometerRecords.Any())
            {
                numbersArray.Add(odometerRecords.Max(x => x.Mileage));
            }
            return numbersArray.Any() ? numbersArray.Max() : 0;
        }
    }
}

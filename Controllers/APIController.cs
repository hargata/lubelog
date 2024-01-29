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
        private readonly IUserAccessDataAccess _userAccessDataAccess;
        private readonly IUserRecordDataAccess _userRecordDataAccess;
        private readonly IReminderHelper _reminderHelper;
        private readonly IGasHelper _gasHelper;
        private readonly IUserLogic _userLogic;
        private readonly IFileHelper _fileHelper;
        private readonly IMailHelper _mailHelper;
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
            IUserAccessDataAccess userAccessDataAccess,
            IUserRecordDataAccess userRecordDataAccess,
            IMailHelper mailHelper,
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
            _userAccessDataAccess = userAccessDataAccess;
            _userRecordDataAccess = userRecordDataAccess;
            _mailHelper = mailHelper;
            _gasHelper = gasHelper;
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
        [Route("/api/vehicle/feerecords")]
        public IActionResult FeeRecords(int vehicleId)
        {
            var result = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        [Route("/api/vehicle/feerecords/add")]
        public IActionResult AddFeeRecord(int vehicleId, TaxRecordExportModel input)
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
                response.Message = "Fee Record Added";
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
            var results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, DateTime.Now).Select(x=> new ReminderExportModel {  Description = x.Description, Urgency = x.Urgency.ToString(), Metric = x.Metric.ToString(), Notes = x.Notes});
            return Json(results);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        [Route("/api/vehicle/reminders/send")]
        public IActionResult SendReminders(List<ReminderUrgency> urgencies)
        {
            var vehicles = _dataAccess.GetVehicles();
            List<OperationResponse> operationResponses = new List<OperationResponse>();
            foreach(Vehicle vehicle in vehicles)
            {
                var vehicleId = vehicle.Id;
                //get reminders
                var currentMileage = GetMaxMileage(vehicleId);
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
            if (operationResponses.All(x => x.Success))
            {
                return Json(new OperationResponse { Success = true, Message = "Emails sent" });
            } else if (operationResponses.All(x => !x.Success))
            {
                return Json(new OperationResponse { Success = false, Message = "All emails failed, check SMTP settings" });
            } else
            {
                return Json(new OperationResponse { Success = true, Message = "Some emails sent, some failed, check recipient settings" });
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
        [Route("/api/demo/restore")]
        public IActionResult RestoreDemo()
        {
            var result = _fileHelper.RestoreBackup("/defaults/demo_default.zip", true);
            return Json(result);
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

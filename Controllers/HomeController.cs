using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CarCareTracker.Helper;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CarCareTracker.Logic;

namespace CarCareTracker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IVehicleDataAccess _dataAccess;
        private readonly IUserLogic _userLogic;
        private readonly ILoginLogic _loginLogic;
        private readonly IVehicleLogic _vehicleLogic;
        private readonly IFileHelper _fileHelper;
        private readonly IConfigHelper _config;
        private readonly IExtraFieldDataAccess _extraFieldDataAccess;
        private readonly IReminderRecordDataAccess _reminderRecordDataAccess;
        private readonly IReminderHelper _reminderHelper;
        public HomeController(ILogger<HomeController> logger,
            IVehicleDataAccess dataAccess,
            IUserLogic userLogic,
            ILoginLogic loginLogic,
            IVehicleLogic vehicleLogic,
            IConfigHelper configuration,
            IFileHelper fileHelper,
            IExtraFieldDataAccess extraFieldDataAccess,
            IReminderRecordDataAccess reminderRecordDataAccess,
            IReminderHelper reminderHelper)
        {
            _logger = logger;
            _dataAccess = dataAccess;
            _config = configuration;
            _userLogic = userLogic;
            _fileHelper = fileHelper;
            _extraFieldDataAccess = extraFieldDataAccess;
            _reminderRecordDataAccess = reminderRecordDataAccess;
            _reminderHelper = reminderHelper;
            _loginLogic = loginLogic;
            _vehicleLogic = vehicleLogic;
        }
        private int GetUserID()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
        public IActionResult Index(string tab = "garage")
        {
            return View(model: tab);
        }
        public IActionResult Garage()
        {
            var vehiclesStored = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehiclesStored = _userLogic.FilterUserVehicles(vehiclesStored, GetUserID());
            }
            var vehicleViewModels = vehiclesStored.Select(x => {
                var vehicleVM = new VehicleViewModel
                {
                    Id = x.Id,
                    ImageLocation = x.ImageLocation,
                    Year = x.Year,
                    Make = x.Make,
                    Model = x.Model,
                    LicensePlate = x.LicensePlate,
                    SoldDate = x.SoldDate,
                    IsElectric = x.IsElectric,
                    IsDiesel = x.IsDiesel,
                    UseHours = x.UseHours,
                    OdometerOptional = x.OdometerOptional,
                    ExtraFields = x.ExtraFields,
                    Tags = x.Tags,
                    DashboardMetrics = x.DashboardMetrics
                };
                //dashboard metrics
                if (x.DashboardMetrics.Any())
                {
                    var vehicleRecords = _vehicleLogic.GetVehicleRecords(x.Id);
                    var userConfig = _config.GetUserConfig(User);
                    var distanceUnit = x.UseHours ? "h" : userConfig.UseMPG ? "mi." : "km";
                    if (vehicleVM.DashboardMetrics.Contains(DashboardMetric.Default))
                    {
                        vehicleVM.LastReportedMileage = _vehicleLogic.GetMaxMileage(vehicleRecords);
                        vehicleVM.HasReminders = _vehicleLogic.GetVehicleHasUrgentOrPastDueReminders(x.Id, vehicleVM.LastReportedMileage);
                    }
                    if (vehicleVM.DashboardMetrics.Contains(DashboardMetric.CostPerMile))
                    {
                        var vehicleTotalCost = _vehicleLogic.GetVehicleTotalCost(vehicleRecords);
                        var maxMileage = _vehicleLogic.GetMaxMileage(vehicleRecords);
                        var minMileage = _vehicleLogic.GetMinMileage(vehicleRecords);
                        var totalDistance = maxMileage - minMileage;
                        vehicleVM.CostPerMile = totalDistance != default ? vehicleTotalCost / totalDistance : 0.00M;
                        vehicleVM.DistanceUnit = distanceUnit;
                    }
                    if (vehicleVM.DashboardMetrics.Contains(DashboardMetric.TotalCost))
                    {
                        vehicleVM.TotalCost = _vehicleLogic.GetVehicleTotalCost(vehicleRecords);
                    }
                }
                return vehicleVM;
            }).ToList();
            return PartialView("_GarageDisplay", vehicleViewModels);
        }
        public IActionResult Calendar()
        {
            var vehiclesStored = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehiclesStored = _userLogic.FilterUserVehicles(vehiclesStored, GetUserID());
            }
            List<ReminderRecordViewModel> reminders = new List<ReminderRecordViewModel>();
            foreach (Vehicle vehicle in vehiclesStored)
            {
                var vehicleReminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicle.Id);
                vehicleReminders.RemoveAll(x => x.Metric == ReminderMetric.Odometer);
                //we don't care about mileages so we can basically fake the current vehicle mileage.
                if (vehicleReminders.Any())
                {
                    var reminderUrgency = _reminderHelper.GetReminderRecordViewModels(vehicleReminders, 0, DateTime.Now);
                    reminderUrgency = reminderUrgency.Select(x => new ReminderRecordViewModel { Id = x.Id, Date = x.Date, Urgency = x.Urgency, Description = $"{vehicle.Year} {vehicle.Make} {vehicle.Model} #{vehicle.LicensePlate} - {x.Description}" }).ToList();
                    reminders.AddRange(reminderUrgency);
                }
            }
            return PartialView("_Calendar", reminders);
        }
        public IActionResult ViewCalendarReminder(int reminderId)
        {
            var reminder = _reminderRecordDataAccess.GetReminderRecordById(reminderId);
            var reminderUrgency = _reminderHelper.GetReminderRecordViewModels(new List<ReminderRecord> { reminder }, 0, DateTime.Now).FirstOrDefault();
            return PartialView("_ReminderRecordCalendarModal", reminderUrgency);
        }
        public async Task<IActionResult> Settings()
        {
            var userConfig = _config.GetUserConfig(User);
            var languages = _fileHelper.GetLanguages();
            var viewModel = new SettingsViewModel
            {
                UserConfig = userConfig,
                UILanguages = languages
            };
            return PartialView("_Settings", viewModel);
        }
        public async Task<IActionResult> Sponsors()
        {
            try
            {
                var httpClient = new HttpClient();
                var sponsorsData = await httpClient.GetFromJsonAsync<Sponsors>(StaticHelper.SponsorsPath) ?? new Sponsors();
                return PartialView("_Sponsors", sponsorsData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to retrieve sponsors: {ex.Message}");
                return PartialView("_Sponsors", new Sponsors());
            }
        }
        [HttpPost]
        public IActionResult WriteToSettings(UserConfig userConfig)
        {
            //retrieve existing userConfig.
            var existingConfig = _config.GetUserConfig(User);
            //copy over stuff that persists
            userConfig.UserColumnPreferences = existingConfig.UserColumnPreferences;
            userConfig.ReminderUrgencyConfig = existingConfig.ReminderUrgencyConfig;
            var result = _config.SaveUserConfig(User, userConfig);
            return Json(result);
        }
        [HttpPost]
        public IActionResult SaveReminderUrgencyThreshold(ReminderUrgencyConfig reminderUrgencyConfig)
        {
            //retrieve existing userConfig.
            var existingConfig = _config.GetUserConfig(User);
            existingConfig.ReminderUrgencyConfig = reminderUrgencyConfig;
            var result = _config.SaveUserConfig(User, existingConfig);
            return Json(result);
        }
        public IActionResult Privacy()
        {
            return View();
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        public IActionResult GetExtraFieldsModal(int importMode = 0)
        {
            var recordExtraFields = _extraFieldDataAccess.GetExtraFieldsById(importMode);
            if (recordExtraFields.Id != importMode)
            {
                recordExtraFields.Id = importMode;
            }
            return PartialView("_ExtraFields", recordExtraFields);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        public IActionResult UpdateExtraFields(RecordExtraField record)
        {
            try
            {
                var result = _extraFieldDataAccess.SaveExtraFields(record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            var recordExtraFields = _extraFieldDataAccess.GetExtraFieldsById(record.Id);
            return PartialView("_ExtraFields", recordExtraFields);
        }
        [HttpPost]
        public IActionResult GenerateTokenForUser()
        {
            try
            {
                //get current user email address.
                var emailAddress = User.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrWhiteSpace(emailAddress))
                {
                    var result = _loginLogic.GenerateTokenForEmailAddress(emailAddress, false);
                    return Json(result);
                }
                return Json(false);
            } catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(false);
            }
        }
        [HttpPost]
        public IActionResult UpdateUserAccount(LoginModel userAccount)
        {
            try
            {
                var userId = GetUserID();
                if (userId > 0)
                {
                    var result = _loginLogic.UpdateUserDetails(userId, userAccount);
                    return Json(result);
                }
                return Json(new OperationResponse { Success = false, Message = StaticHelper.GenericErrorMessage});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new OperationResponse { Success = false, Message = StaticHelper.GenericErrorMessage });
            }
        }
        [HttpGet]
        public IActionResult GetUserAccountInformationModal()
        {
            var emailAddress = User.FindFirstValue(ClaimTypes.Email);
            var userName = User.Identity.Name;
            return PartialView("_AccountModal", new UserData() { EmailAddress = emailAddress, UserName = userName });
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        public IActionResult GetRootAccountInformationModal()
        {
            var userName = User.Identity.Name;
            return PartialView("_RootAccountModal", new UserData() { UserName = userName });
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

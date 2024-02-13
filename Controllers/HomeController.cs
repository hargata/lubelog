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
        private readonly IFileHelper _fileHelper;
        private readonly IConfigHelper _config;
        private readonly IExtraFieldDataAccess _extraFieldDataAccess;
        private readonly IReminderRecordDataAccess _reminderRecordDataAccess;
        private readonly IReminderHelper _reminderHelper;
        public HomeController(ILogger<HomeController> logger,
            IVehicleDataAccess dataAccess,
            IUserLogic userLogic,
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
            return PartialView("_GarageDisplay", vehiclesStored);
        }
        public IActionResult Calendar()
        {
            var vehiclesStored = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehiclesStored = _userLogic.FilterUserVehicles(vehiclesStored, GetUserID());
            }
            List<ReminderRecordViewModel> reminders = new List<ReminderRecordViewModel>();
            foreach(Vehicle vehicle in vehiclesStored)
            {
                var vehicleReminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicle.Id);
                vehicleReminders.RemoveAll(x => x.Metric == ReminderMetric.Odometer);
                //we don't care about mileages so we can basically fake the current vehicle mileage.
                if (vehicleReminders.Any())
                {
                    var maxMileage = vehicleReminders.Max(x => x.Mileage) + 1000;
                    var reminderUrgency = _reminderHelper.GetReminderRecordViewModels(vehicleReminders, maxMileage, DateTime.Now);
                    reminderUrgency = reminderUrgency.Select(x => new ReminderRecordViewModel { Date = x.Date, Urgency = x.Urgency, Description = $"{vehicle.Year} {vehicle.Make} {vehicle.Model} #{vehicle.LicensePlate} - {x.Description}" }).ToList();
                    reminders.AddRange(reminderUrgency);
                }
            }
            return PartialView("_Calendar", reminders);
        }
        public IActionResult Settings()
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
        [HttpPost]
        public IActionResult WriteToSettings(UserConfig userConfig)
        {
            var result = _config.SaveUserConfig(User, userConfig);
            return Json(result);
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult GetExtraFieldsModal(int importMode = 0)
        {
            var recordExtraFields = _extraFieldDataAccess.GetExtraFieldsById(importMode);
            if (recordExtraFields.Id != importMode)
            {
                recordExtraFields.Id = importMode;
            }
            return PartialView("_ExtraFields", recordExtraFields);
        }
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
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

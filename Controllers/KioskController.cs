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
    public class KioskController : Controller
    {
        private readonly ILogger<KioskController> _logger;
        private readonly IVehicleDataAccess _dataAccess;
        private readonly IVehicleLogic _vehicleLogic;
        private readonly IUserLogic _userLogic;
        private readonly IConfigHelper _config;
        public KioskController(ILogger<KioskController> logger, 
            IVehicleDataAccess dataAccess, 
            IVehicleLogic vehicleLogic, 
            IUserLogic userLogic, 
            IConfigHelper config)
        {
            _logger = logger;
            _dataAccess = dataAccess;
            _vehicleLogic = vehicleLogic;
            _userLogic = userLogic;
            _config = config;
        }
        private int GetUserID()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
        public IActionResult Index(string exclusions, KioskMode kioskMode = KioskMode.Vehicle)
        {
            try
            {
                var viewModel = new KioskViewModel
                {
                    Exclusions = string.IsNullOrWhiteSpace(exclusions) ? new List<int>() : exclusions.Split(',').Select(x => int.Parse(x)).ToList(),
                    KioskMode = kioskMode
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View(new KioskViewModel());
            }
        }
        [HttpPost]
        public IActionResult KioskContent(KioskViewModel kioskParameters)
        {
            var vehiclesStored = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehiclesStored = _userLogic.FilterUserVehicles(vehiclesStored, GetUserID());
            }
            vehiclesStored.RemoveAll(x => kioskParameters.Exclusions.Contains(x.Id));
            var userConfig = _config.GetUserConfig(User);
            if (userConfig.HideSoldVehicles)
            {
                vehiclesStored.RemoveAll(x => !string.IsNullOrWhiteSpace(x.SoldDate));
            }
            switch (kioskParameters.KioskMode)
            {
                case KioskMode.Vehicle:
                    {
                        var kioskResult = _vehicleLogic.GetVehicleInfo(vehiclesStored);
                        return PartialView("_Kiosk", kioskResult);
                    }
                case KioskMode.Plan:
                    {
                        var kioskResult = _vehicleLogic.GetPlansForKiosk(vehiclesStored, false);
                        return PartialView("_KioskPlan", kioskResult);
                    }
                case KioskMode.Reminder:
                    {
                        var kioskResult = _vehicleLogic.GetRemindersForKiosk(vehiclesStored);
                        return PartialView("_KioskReminder", kioskResult);
                    }
            }
            var result = _vehicleLogic.GetVehicleInfo(vehiclesStored);
            return PartialView("_Kiosk", result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        public IActionResult GetKioskVehicleInfo(int vehicleId)
        {
            var result = _vehicleLogic.GetKioskVehicleInfo(vehicleId);
            return PartialView("_KioskVehicleInfo", result);
        }
    }
}

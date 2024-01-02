using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;

namespace CarCareTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IVehicleDataAccess _dataAccess;
        private readonly IWebHostEnvironment _webEnv;

        public HomeController(ILogger<HomeController> logger, IVehicleDataAccess dataAccess, IWebHostEnvironment webEnv)
        {
            _logger = logger;
            _dataAccess = dataAccess;
            _webEnv = webEnv;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Garage()
        {
            var vehiclesStored = _dataAccess.GetVehicles();
            return PartialView("_GarageDisplay", vehiclesStored);
        }
        public IActionResult Privacy()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddVehicle(Vehicle vehicleInput)
        {
            try
            {
                //save vehicle.
                var result = _dataAccess.SaveVehicle(vehicleInput);
                return Json(result);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error Saving Vehicle");
                return Json(false);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

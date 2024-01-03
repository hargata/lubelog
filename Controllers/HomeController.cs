using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using CarCareTracker.Helper;

namespace CarCareTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IVehicleDataAccess _dataAccess;
        private readonly IFileHelper _fileHelper;

        public HomeController(ILogger<HomeController> logger, IVehicleDataAccess dataAccess, IFileHelper fileHelper)
        {
            _logger = logger;
            _dataAccess = dataAccess;
            _fileHelper = fileHelper;
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
        [HttpGet]
        public IActionResult AddVehiclePartialView()
        {
            return PartialView("_VehicleModal", new Vehicle());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

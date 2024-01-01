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

        public IActionResult Index(VehicleInputModel? initialModel)
        {
            if (initialModel is not null && initialModel.Errors is not null)
            {
                return View(initialModel);
            }
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
        public IActionResult AddVehicle(VehicleInputModel vehicleInput)
        {
            var errors = new List<string>();
            //validation
            if (vehicleInput.Year < 1900)
                errors.Add("Year is invalid");
            if (string.IsNullOrWhiteSpace(vehicleInput.Make))
                errors.Add("Make is required");
            if (string.IsNullOrWhiteSpace(vehicleInput.Model))
                errors.Add("Model is required");
            if (string.IsNullOrWhiteSpace(vehicleInput.LicensePlate))
                errors.Add("License Plate is required");
            if (errors.Any())
            {
                vehicleInput.Errors = errors;
                return RedirectToAction("Index", "Home", vehicleInput);
            }

            try
            {
                //map vehicleInput to vehicle object.
                var newVehicle = new Vehicle
                {
                    Year = vehicleInput.Year,
                    Make = vehicleInput.Make,
                    Model = vehicleInput.Model,
                    LicensePlate = vehicleInput.LicensePlate
                };
                if (vehicleInput.Image is not null)
                {
                    string imagePath = UploadImage(vehicleInput.Image);
                    if (!string.IsNullOrWhiteSpace(imagePath))
                    {
                        newVehicle.ImageLocation = imagePath;
                    }
                }
                //save vehicle.
                var result = _dataAccess.AddVehicle(newVehicle);
                RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error Saving Vehicle");
                vehicleInput.Errors = new List<string> { "Error Saving Vehicle, Please Try Again Later" };
                return RedirectToAction("Index", "Home", vehicleInput);
            }
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private string UploadImage(IFormFile fileToUpload)
        {
            string uploadDirectory = "images/";
            string uploadPath = Path.Combine(_webEnv.WebRootPath, uploadDirectory);
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);
            string fileName = Guid.NewGuid() + Path.GetExtension(fileToUpload.FileName);
            string filePath = Path.Combine(uploadPath, fileName);
            using (var stream = System.IO.File.Create(filePath))
            {
                fileToUpload.CopyTo(stream);
            }
            return Path.Combine("/", uploadDirectory, fileName);
        }
    }
}

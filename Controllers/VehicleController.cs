using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using CarCareTracker.External.Implementations;
using CarCareTracker.Helper;

namespace CarCareTracker.Controllers
{
    public class VehicleController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IVehicleDataAccess _dataAccess;
        private readonly INoteDataAccess _noteDataAccess;
        private readonly IServiceRecordDataAccess _serviceRecordDataAccess;
        private readonly IGasRecordDataAccess _gasRecordDataAccess;
        private readonly IWebHostEnvironment _webEnv;
        private readonly IFileHelper _fileHelper;

        public VehicleController(ILogger<HomeController> logger, 
            IFileHelper fileHelper, 
            IVehicleDataAccess dataAccess, 
            INoteDataAccess noteDataAccess, 
            IServiceRecordDataAccess serviceRecordDataAccess, 
            IGasRecordDataAccess gasRecordDataAccess,
            IWebHostEnvironment webEnv)
        {
            _logger = logger;
            _dataAccess = dataAccess;
            _noteDataAccess = noteDataAccess;
            _fileHelper = fileHelper;
            _serviceRecordDataAccess = serviceRecordDataAccess;
            _gasRecordDataAccess = gasRecordDataAccess;
            _webEnv = webEnv;
        }
        [HttpGet]
        public IActionResult Index(int vehicleId)
        {
            var data = _dataAccess.GetVehicleById(vehicleId);
            return View(data);
        }
        [HttpPost]
        public IActionResult DeleteVehicle(int vehicleId)
        {
            var result = _dataAccess.DeleteVehicle(vehicleId);
            return Json(result);
        }
        [HttpPost]
        public IActionResult SaveNoteToVehicle(Note newNote)
        {
            //check if there is already an existing note for this vehicle.
            var existingNote = _noteDataAccess.GetNoteByVehicleId(newNote.VehicleId);
            if (existingNote.Id != default)
            {
                newNote.Id = existingNote.Id;
            }
            var result = _noteDataAccess.SaveNoteToVehicleId(newNote);
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetNoteByVehicleId(int vehicleId)
        {
            var existingNote = _noteDataAccess.GetNoteByVehicleId(vehicleId);
            if (existingNote.Id != default)
            {
                return Json(existingNote.NoteText);
            }
            return Json("");
        }
        #region "Gas Records"
        [HttpGet]
        public IActionResult GetGasRecordsByVehicleId(int vehicleId)
        {
            var result = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            var computedResults = new List<GasRecordViewModel>();
            int previousMileage = 0;
            //perform computation.
            for(int i = 0; i < result.Count; i++)
            {
                if (i > 0)
                {
                    var currentObject = result[i];
                    var deltaMileage = currentObject.Mileage - previousMileage;
                    computedResults.Add(new GasRecordViewModel()
                    {
                        Id = currentObject.Id,
                        VehicleId = currentObject.VehicleId,
                        Date = currentObject.Date.ToShortDateString(),
                        Mileage = currentObject.Mileage,
                        Gallons = currentObject.Gallons,
                        Cost = currentObject.Cost,
                        DeltaMileage = deltaMileage,
                        MilesPerGallon = deltaMileage / currentObject.Gallons,
                        CostPerGallon = (currentObject.Cost / currentObject.Gallons)
                    });
                } else
                {
                    computedResults.Add(new GasRecordViewModel()
                    {
                        Id = result[i].Id,
                        VehicleId = result[i].VehicleId,
                        Date = result[i].Date.ToShortDateString(),
                        Mileage = result[i].Mileage,
                        Gallons = result[i].Gallons,
                        Cost = result[i].Cost,
                        DeltaMileage = 0,
                        MilesPerGallon = 0,
                        CostPerGallon = (result[i].Cost / result[i].Gallons)
                    });
                }
                previousMileage = result[i].Mileage;
            }
            return PartialView("_Gas", computedResults);
        }
        [HttpPost]
        public IActionResult SaveGasRecordToVehicleId(GasRecordInput gasRecord)
        {
            gasRecord.Files = gasRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var result = _gasRecordDataAccess.SaveGasRecordToVehicle(gasRecord.ToGasRecord());
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddGasRecordPartialView()
        {
            return PartialView("_GasModal", new GasRecordInput());
        }
        #endregion
        #region "Service Records"
        [HttpGet]
        public IActionResult GetServiceRecordsByVehicleId(int vehicleId)
        {
            var result = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
            return PartialView("_ServiceRecords", result);
        }
        [HttpPost]
        public IActionResult SaveServiceRecordToVehicleId(ServiceRecordInput serviceRecord)
        {
            //move files from temp.
            serviceRecord.Files = serviceRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var result = _serviceRecordDataAccess.SaveServiceRecordToVehicle(serviceRecord.ToServiceRecord());
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddServiceRecordPartialView()
        {
            return PartialView("_ServiceRecordModal", new ServiceRecordInput());
        }
        [HttpGet]
        public IActionResult GetServiceRecordForEditById(int serviceRecordId)
        {
            var result = _serviceRecordDataAccess.GetServiceRecordById(serviceRecordId);
            //convert to Input object.
            var convertedResult = new ServiceRecordInput { Id = result.Id, 
                Cost = result.Cost, 
                Date = result.Date.ToShortDateString(), 
                Description = result.Description,
                Mileage = result.Mileage,
                Notes = result.Notes,
                VehicleId = result.VehicleId,
                Files = result.Files
            };
            return PartialView("_ServiceRecordModal", convertedResult);
        }
        [HttpPost] 
        public IActionResult DeleteServiceRecordById(int serviceRecordId)
        {
            var result = _serviceRecordDataAccess.DeleteServiceRecordById(serviceRecordId);
            return Json(result);
        }
        #endregion

    }
}

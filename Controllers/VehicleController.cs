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

namespace CarCareTracker.Controllers
{
    public class VehicleController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IVehicleDataAccess _dataAccess;
        private readonly INoteDataAccess _noteDataAccess;
        private readonly IServiceRecordDataAccess _serviceRecordDataAccess;
        private readonly IWebHostEnvironment _webEnv;

        public VehicleController(ILogger<HomeController> logger, IVehicleDataAccess dataAccess, INoteDataAccess noteDataAccess, IServiceRecordDataAccess serviceRecordDataAccess, IWebHostEnvironment webEnv)
        {
            _logger = logger;
            _dataAccess = dataAccess;
            _noteDataAccess = noteDataAccess;
            _serviceRecordDataAccess = serviceRecordDataAccess;
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
        [HttpGet]
        public IActionResult GetServiceRecordsByVehicleId(int vehicleId)
        {
            var result = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
            return PartialView("_ServiceRecords", result);
        }
        [HttpPost]
        public IActionResult SaveServiceRecordToVehicleId(ServiceRecordInput serviceRecord)
        {
            var result = _serviceRecordDataAccess.SaveServiceRecordToVehicle(serviceRecord.ToServiceRecord());
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetServiceRecordById(int serviceRecordId)
        {
            var result = _serviceRecordDataAccess.GetServiceRecordById(serviceRecordId);
            //convert to Input object.
            var convertedResult = new ServiceRecordInput { Id = result.Id, 
                Cost = result.Cost, 
                Date = result.Date.ToShortDateString(), 
                Description = result.Description,
                Mileage = result.Mileage,
                Notes = result.Notes,
                VehicleId = result.VehicleId
            };
            return Json(convertedResult);
        }
        [HttpPost] 
        public IActionResult DeleteServiceRecordById(int serviceRecordId)
        {
            var result = _serviceRecordDataAccess.DeleteServiceRecordById(serviceRecordId);
            return Json(result);
        }
    }
}

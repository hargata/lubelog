using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using CarCareTracker.Helper;
using CsvHelper;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using CarCareTracker.External.Implementations;

namespace CarCareTracker.Controllers
{
    [Authorize]
    public class VehicleController : Controller
    {
        private readonly ILogger<VehicleController> _logger;
        private readonly IVehicleDataAccess _dataAccess;
        private readonly INoteDataAccess _noteDataAccess;
        private readonly IServiceRecordDataAccess _serviceRecordDataAccess;
        private readonly IGasRecordDataAccess _gasRecordDataAccess;
        private readonly ICollisionRecordDataAccess _collisionRecordDataAccess;
        private readonly ITaxRecordDataAccess _taxRecordDataAccess;
        private readonly IReminderRecordDataAccess _reminderRecordDataAccess;
        private readonly IWebHostEnvironment _webEnv;
        private readonly bool _useDescending;
        private readonly IConfiguration _config;
        private readonly IFileHelper _fileHelper;

        public VehicleController(ILogger<VehicleController> logger,
            IFileHelper fileHelper,
            IVehicleDataAccess dataAccess,
            INoteDataAccess noteDataAccess,
            IServiceRecordDataAccess serviceRecordDataAccess,
            IGasRecordDataAccess gasRecordDataAccess,
            ICollisionRecordDataAccess collisionRecordDataAccess,
            ITaxRecordDataAccess taxRecordDataAccess,
            IReminderRecordDataAccess reminderRecordDataAccess,
            IWebHostEnvironment webEnv,
            IConfiguration config)
        {
            _logger = logger;
            _dataAccess = dataAccess;
            _noteDataAccess = noteDataAccess;
            _fileHelper = fileHelper;
            _serviceRecordDataAccess = serviceRecordDataAccess;
            _gasRecordDataAccess = gasRecordDataAccess;
            _collisionRecordDataAccess = collisionRecordDataAccess;
            _taxRecordDataAccess = taxRecordDataAccess;
            _reminderRecordDataAccess = reminderRecordDataAccess;
            _webEnv = webEnv;
            _config = config;
            _useDescending = bool.Parse(config[nameof(UserConfig.UseDescending)]);
        }
        [HttpGet]
        public IActionResult Index(int vehicleId)
        {
            var data = _dataAccess.GetVehicleById(vehicleId);
            return View(data);
        }
        [HttpGet]
        public IActionResult AddVehiclePartialView()
        {
            return PartialView("_VehicleModal", new Vehicle());
        }
        [HttpGet]
        public IActionResult GetEditVehiclePartialViewById(int vehicleId)
        {
            var data = _dataAccess.GetVehicleById(vehicleId);
            return PartialView("_VehicleModal", data);
        }
        [HttpPost]
        public IActionResult SaveVehicle(Vehicle vehicleInput)
        {
            try
            {
                //move image from temp folder to images folder.
                vehicleInput.ImageLocation = _fileHelper.MoveFileFromTemp(vehicleInput.ImageLocation, "images/");
                //save vehicle.
                var result = _dataAccess.SaveVehicle(vehicleInput);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Saving Vehicle");
                return Json(false);
            }
        }
        [HttpPost]
        public IActionResult DeleteVehicle(int vehicleId)
        {
            //Delete all service records, gas records, notes, etc.
            var result = _gasRecordDataAccess.DeleteAllGasRecordsByVehicleId(vehicleId) &&
                _serviceRecordDataAccess.DeleteAllServiceRecordsByVehicleId(vehicleId) &&
                _collisionRecordDataAccess.DeleteAllCollisionRecordsByVehicleId(vehicleId) &&
                _taxRecordDataAccess.DeleteAllTaxRecordsByVehicleId(vehicleId) &&
                _noteDataAccess.DeleteNoteByVehicleId(vehicleId) &&
                _reminderRecordDataAccess.DeleteAllReminderRecordsByVehicleId(vehicleId) &&
                _dataAccess.DeleteVehicle(vehicleId);
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
        #region "Bulk Imports"
        [HttpGet]
        public IActionResult GetBulkImportModalPartialView(string mode)
        {
            return PartialView("_BulkDataImporter", mode);
        }
        [HttpPost]
        public IActionResult ImportToVehicleIdFromCsv(int vehicleId, string mode, string fileName)
        {
            if (vehicleId == default || string.IsNullOrWhiteSpace(fileName))
            {
                return Json(false);
            }
            var fullFileName = _fileHelper.GetFullFilePath(fileName);
            if (string.IsNullOrWhiteSpace(fullFileName))
            {
                return Json(false);
            }
            try
            {
                using (var reader = new StreamReader(fullFileName))
                {
                    var config = new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture);
                    config.MissingFieldFound = null;
                    config.HeaderValidated = null;
                    using (var csv = new CsvReader(reader, config))
                    {
                        if (mode == "gasrecord")
                        {
                            var records = csv.GetRecords<GasRecordImport>().ToList();
                            if (records.Any())
                            {
                                foreach (GasRecordImport recordToInsert in records)
                                {
                                    var convertedRecord = new GasRecord()
                                    {
                                        VehicleId = vehicleId,
                                        Date = recordToInsert.Date,
                                        Mileage = recordToInsert.Odometer,
                                        Gallons = recordToInsert.FuelConsumed,
                                        Cost = recordToInsert.Cost
                                    };
                                    _gasRecordDataAccess.SaveGasRecordToVehicle(convertedRecord);
                                }
                            }
                        }
                        else if (mode == "servicerecord")
                        {
                            var records = csv.GetRecords<ServiceRecordImport>().ToList();
                            if (records.Any())
                            {
                                foreach (ServiceRecordImport recordToInsert in records)
                                {
                                    var convertedRecord = new ServiceRecord()
                                    {
                                        VehicleId = vehicleId,
                                        Date = recordToInsert.Date,
                                        Mileage = recordToInsert.Odometer,
                                        Description = recordToInsert.Description,
                                        Notes = recordToInsert.Notes,
                                        Cost = recordToInsert.Cost
                                    };
                                    _serviceRecordDataAccess.SaveServiceRecordToVehicle(convertedRecord);
                                }
                            }
                        }
                        else if (mode == "repairrecord")
                        {
                            var records = csv.GetRecords<ServiceRecordImport>().ToList();
                            if (records.Any())
                            {
                                foreach (ServiceRecordImport recordToInsert in records)
                                {
                                    var convertedRecord = new CollisionRecord()
                                    {
                                        VehicleId = vehicleId,
                                        Date = recordToInsert.Date,
                                        Mileage = recordToInsert.Odometer,
                                        Description = recordToInsert.Description,
                                        Notes = recordToInsert.Notes,
                                        Cost = recordToInsert.Cost
                                    };
                                    _collisionRecordDataAccess.SaveCollisionRecordToVehicle(convertedRecord);
                                }
                            }
                        }
                        else if (mode == "taxrecord")
                        {
                            var records = csv.GetRecords<TaxRecordImport>().ToList();
                            if (records.Any())
                            {
                                foreach (TaxRecordImport recordToInsert in records)
                                {
                                    var convertedRecord = new TaxRecord()
                                    {
                                        VehicleId = vehicleId,
                                        Date = recordToInsert.Date,
                                        Description = recordToInsert.Description,
                                        Notes = recordToInsert.Notes,
                                        Cost = recordToInsert.Cost
                                    };
                                    _taxRecordDataAccess.SaveTaxRecordToVehicle(convertedRecord);
                                }
                            }
                        }
                    }
                }
                return Json(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Occurred While Bulk Inserting");
                return Json(false);
            }
        }
        #endregion
        #region "Gas Records"
        [HttpGet]
        public IActionResult GetGasRecordsByVehicleId(int vehicleId)
        {
            var result = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            //need it in ascending order to perform computation.
            result = result.OrderBy(x => x.Date).ThenBy(x => x.Mileage).ToList();
            //check if the user uses MPG or Liters per 100km.
            bool useMPG = bool.Parse(_config[nameof(UserConfig.UseMPG)]);
            var computedResults = new List<GasRecordViewModel>();
            int previousMileage = 0;
            decimal unFactoredConsumption = 0.00M;
            int unFactoredMileage = 0;
            //perform computation.
            for (int i = 0; i < result.Count; i++)
            {
                if (i > 0)
                {
                    var currentObject = result[i];
                    var deltaMileage = currentObject.Mileage - previousMileage;
                    var gasRecordViewModel = new GasRecordViewModel()
                    {
                        Id = currentObject.Id,
                        VehicleId = currentObject.VehicleId,
                        Date = currentObject.Date.ToShortDateString(),
                        Mileage = currentObject.Mileage,
                        Gallons = currentObject.Gallons,
                        Cost = currentObject.Cost,
                        DeltaMileage = deltaMileage,
                        CostPerGallon = (currentObject.Cost / currentObject.Gallons)
                    };
                    if (currentObject.IsFillToFull)
                    {
                        //if user filled to full.
                        gasRecordViewModel.MilesPerGallon = useMPG ? ((unFactoredMileage + deltaMileage) / (unFactoredConsumption + currentObject.Gallons)) : 100 / ((unFactoredMileage + deltaMileage) / (unFactoredConsumption + currentObject.Gallons));
                        //reset unFactored vars
                        unFactoredConsumption = 0;
                        unFactoredMileage = 0;
                    } else
                    {
                        unFactoredConsumption += currentObject.Gallons;
                        unFactoredMileage += deltaMileage;
                        gasRecordViewModel.MilesPerGallon = 0;
                    }
                    computedResults.Add(gasRecordViewModel);
                }
                else
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
            if (_useDescending)
            {
                computedResults = computedResults.OrderByDescending(x => DateTime.Parse(x.Date)).ThenByDescending(x => x.Mileage).ToList();
            }
            var vehicleIsElectric = _dataAccess.GetVehicleById(vehicleId).IsElectric;
            var viewModel = new GasRecordViewModelContainer()
            {
                UseKwh = vehicleIsElectric,
                GasRecords = computedResults
            };
            return PartialView("_Gas", viewModel);
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
            return PartialView("_GasModal", new GasRecordInputContainer() { GasRecord = new GasRecordInput() });
        }
        [HttpGet]
        public IActionResult GetGasRecordForEditById(int gasRecordId)
        {
            var result = _gasRecordDataAccess.GetGasRecordById(gasRecordId);
            var convertedResult = new GasRecordInput
            {
                Id = result.Id,
                Mileage = result.Mileage,
                VehicleId = result.VehicleId,
                Cost = result.Cost,
                Date = result.Date.ToShortDateString(),
                Files = result.Files,
                Gallons = result.Gallons,
                IsFillToFull = result.IsFillToFull
            };
            var vehicleIsElectric = _dataAccess.GetVehicleById(convertedResult.VehicleId).IsElectric;
            var viewModel = new GasRecordInputContainer()
            {
                UseKwh = vehicleIsElectric,
                GasRecord = convertedResult
            };
            return PartialView("_GasModal", viewModel);
        }
        [HttpPost]
        public IActionResult DeleteGasRecordById(int gasRecordId)
        {
            var result = _gasRecordDataAccess.DeleteGasRecordById(gasRecordId);
            return Json(result);
        }
        #endregion
        #region "Service Records"
        [HttpGet]
        public IActionResult GetServiceRecordsByVehicleId(int vehicleId)
        {
            var result = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
            if (_useDescending)
            {
                result = result.OrderByDescending(x => x.Date).ThenByDescending(x => x.Mileage).ToList();
            }
            else
            {
                result = result.OrderBy(x => x.Date).ThenBy(x => x.Mileage).ToList();
            }
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
            var convertedResult = new ServiceRecordInput
            {
                Id = result.Id,
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
        #region "Collision Records"
        [HttpGet]
        public IActionResult GetCollisionRecordsByVehicleId(int vehicleId)
        {
            var result = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
            if (_useDescending)
            {
                result = result.OrderByDescending(x => x.Date).ThenByDescending(x => x.Mileage).ToList();
            }
            else
            {
                result = result.OrderBy(x => x.Date).ThenBy(x => x.Mileage).ToList();
            }
            return PartialView("_CollisionRecords", result);
        }
        [HttpPost]
        public IActionResult SaveCollisionRecordToVehicleId(CollisionRecordInput collisionRecord)
        {
            //move files from temp.
            collisionRecord.Files = collisionRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var result = _collisionRecordDataAccess.SaveCollisionRecordToVehicle(collisionRecord.ToCollisionRecord());
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddCollisionRecordPartialView()
        {
            return PartialView("_CollisionRecordModal", new CollisionRecordInput());
        }
        [HttpGet]
        public IActionResult GetCollisionRecordForEditById(int collisionRecordId)
        {
            var result = _collisionRecordDataAccess.GetCollisionRecordById(collisionRecordId);
            //convert to Input object.
            var convertedResult = new CollisionRecordInput
            {
                Id = result.Id,
                Cost = result.Cost,
                Date = result.Date.ToShortDateString(),
                Description = result.Description,
                Mileage = result.Mileage,
                Notes = result.Notes,
                VehicleId = result.VehicleId,
                Files = result.Files
            };
            return PartialView("_CollisionRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeleteCollisionRecordById(int collisionRecordId)
        {
            var result = _collisionRecordDataAccess.DeleteCollisionRecordById(collisionRecordId);
            return Json(result);
        }
        #endregion
        #region "Tax Records"
        [HttpGet]
        public IActionResult GetTaxRecordsByVehicleId(int vehicleId)
        {
            var result = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
            if (_useDescending)
            {
                result = result.OrderByDescending(x => x.Date).ToList();
            }
            else
            {
                result = result.OrderBy(x => x.Date).ToList();
            }
            return PartialView("_TaxRecords", result);
        }
        [HttpPost]
        public IActionResult SaveTaxRecordToVehicleId(TaxRecordInput taxRecord)
        {
            //move files from temp.
            taxRecord.Files = taxRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var result = _taxRecordDataAccess.SaveTaxRecordToVehicle(taxRecord.ToTaxRecord());
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddTaxRecordPartialView()
        {
            return PartialView("_TaxRecordModal", new TaxRecordInput());
        }
        [HttpGet]
        public IActionResult GetTaxRecordForEditById(int taxRecordId)
        {
            var result = _taxRecordDataAccess.GetTaxRecordById(taxRecordId);
            //convert to Input object.
            var convertedResult = new TaxRecordInput
            {
                Id = result.Id,
                Cost = result.Cost,
                Date = result.Date.ToShortDateString(),
                Description = result.Description,
                Notes = result.Notes,
                VehicleId = result.VehicleId,
                Files = result.Files
            };
            return PartialView("_TaxRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeleteTaxRecordById(int taxRecordId)
        {
            var result = _taxRecordDataAccess.DeleteTaxRecordById(taxRecordId);
            return Json(result);
        }
        #endregion
        #region "Reports"
        [HttpGet]
        public IActionResult GetReportPartialView()
        {
            return PartialView("_Report");
        }
        [HttpGet]
        public IActionResult GetCostMakeUpForVehicle(int vehicleId, int year = 0)
        {
            var serviceRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
            var gasRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            var collisionRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
            var taxRecords = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
            if (year != default)
            {
                serviceRecords.RemoveAll(x => x.Date.Year != year);
                gasRecords.RemoveAll(x => x.Date.Year != year);
                collisionRecords.RemoveAll(x => x.Date.Year != year);
                taxRecords.RemoveAll(x => x.Date.Year != year);
            }
            var viewModel = new CostMakeUpForVehicle
            {
                ServiceRecordSum = serviceRecords.Sum(x => x.Cost),
                GasRecordSum = gasRecords.Sum(x => x.Cost),
                CollisionRecordSum = collisionRecords.Sum(x => x.Cost),
                TaxRecordSum = taxRecords.Sum(x => x.Cost)
            };
            return PartialView("_CostMakeUpReport", viewModel);
        }
        public IActionResult GetFuelCostByMonthByVehicle(int vehicleId, int year = 0)
        {
            var gasRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            if (year != default)
            {
                gasRecords.RemoveAll(x => x.Date.Year != year);
            }
            var groupedGasRecord = gasRecords.GroupBy(x => x.Date.Month).OrderBy(x => x.Key).Select(x => new GasCostForVehicleByMonth
            {
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key),
                Cost = x.Sum(y => y.Cost)
            }).ToList();
            return PartialView("_GasCostByMonthReport", groupedGasRecord);
        }
        #endregion
        #region "Reminders"
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
            return numbersArray.Any() ? numbersArray.Max() : 0;
        }
        private List<ReminderRecordViewModel> GetRemindersAndUrgency(int vehicleId)
        {
            var currentMileage = GetMaxMileage(vehicleId);
            var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
            List<ReminderRecordViewModel> reminderViewModels = new List<ReminderRecordViewModel>();
            foreach(var reminder in reminders)
            {
                var reminderViewModel = new ReminderRecordViewModel()
                {
                    Id = reminder.Id,
                    VehicleId = reminder.VehicleId,
                    Date = reminder.Date,
                    Mileage = reminder.Mileage,
                    Description = reminder.Description,
                    Notes = reminder.Notes,
                    Metric = reminder.Metric
                };
                if (reminder.Metric == ReminderMetric.Both)
                {
                    if (reminder.Date < DateTime.Now)
                    {
                        reminderViewModel.Urgency = ReminderUrgency.PastDue;
                        reminderViewModel.Metric = ReminderMetric.Date;
                    }
                    else if (reminder.Mileage < currentMileage)
                    {
                        reminderViewModel.Urgency = ReminderUrgency.PastDue;
                        reminderViewModel.Metric = ReminderMetric.Odometer;
                    }
                    else if (reminder.Date < DateTime.Now.AddDays(7))
                    {
                        //if less than a week from today or less than 50 miles from current mileage then very urgent.
                        reminderViewModel.Urgency = ReminderUrgency.VeryUrgent;
                        //have to specify by which metric this reminder is urgent.
                        reminderViewModel.Metric = ReminderMetric.Date;
                    }
                    else if (reminder.Mileage < currentMileage + 50)
                    {
                        reminderViewModel.Urgency = ReminderUrgency.VeryUrgent;
                        reminderViewModel.Metric = ReminderMetric.Odometer;
                    }
                    else if (reminder.Date < DateTime.Now.AddDays(30))
                    {
                        reminderViewModel.Urgency = ReminderUrgency.Urgent;
                        reminderViewModel.Metric = ReminderMetric.Date;
                    }
                     else if (reminder.Mileage < currentMileage + 100)
                    {
                        reminderViewModel.Urgency = ReminderUrgency.Urgent;
                        reminderViewModel.Metric = ReminderMetric.Odometer;
                    }
                } else if (reminder.Metric == ReminderMetric.Date)
                {
                    if (reminder.Date < DateTime.Now)
                    {
                        reminderViewModel.Urgency = ReminderUrgency.PastDue;
                    }
                    else if (reminder.Date < DateTime.Now.AddDays(7))
                    {
                        reminderViewModel.Urgency = ReminderUrgency.VeryUrgent;
                    }
                    else if (reminder.Date < DateTime.Now.AddDays(30))
                    {
                        reminderViewModel.Urgency = ReminderUrgency.Urgent;
                    }
                } else if (reminder.Metric == ReminderMetric.Odometer)
                {
                    if (reminder.Mileage < currentMileage)
                    {
                        reminderViewModel.Urgency = ReminderUrgency.PastDue;
                        reminderViewModel.Metric = ReminderMetric.Odometer;
                    }
                    else if (reminder.Mileage < currentMileage + 50)
                    {
                        reminderViewModel.Urgency = ReminderUrgency.VeryUrgent;
                    }
                    else if (reminder.Mileage < currentMileage + 100)
                    {
                        reminderViewModel.Urgency = ReminderUrgency.Urgent;
                    }
                }
                reminderViewModels.Add(reminderViewModel);
            }
            return reminderViewModels;
        }
        [HttpGet]
        public IActionResult GetVehicleHaveUrgentOrPastDueReminders(int vehicleId)
        {
            var result = GetRemindersAndUrgency(vehicleId);
            if (result.Where(x=>x.Urgency == ReminderUrgency.VeryUrgent || x.Urgency == ReminderUrgency.PastDue).Any())
            {
                return Json(true);
            }
            return Json(false);
        }
        [HttpGet]
        public IActionResult GetReminderRecordsByVehicleId(int vehicleId)
        {
            var result = GetRemindersAndUrgency(vehicleId);
            result = result.OrderByDescending(x=>x.Urgency).ToList();
            return PartialView("_ReminderRecords", result);
        }
        [HttpPost]
        public IActionResult SaveReminderRecordToVehicleId(ReminderRecordInput reminderRecord)
        {
            var result = _reminderRecordDataAccess.SaveReminderRecordToVehicle(reminderRecord.ToReminderRecord());
            return Json(result);
        }
        [HttpPost]
        public IActionResult GetAddReminderRecordPartialView(ReminderRecordInput? reminderModel)
        {
            if (reminderModel is not null)
            {
                return PartialView("_ReminderRecordModal", reminderModel);
            }
            else
            {
                return PartialView("_ReminderRecordModal", new ReminderRecordInput());
            }
        }
        [HttpGet]
        public IActionResult GetReminderRecordForEditById(int reminderRecordId)
        {
            var result = _reminderRecordDataAccess.GetReminderRecordById(reminderRecordId);
            //convert to Input object.
            var convertedResult = new ReminderRecordInput
            {
                Id = result.Id,
                Date = result.Date.ToShortDateString(),
                Description = result.Description,
                Notes = result.Notes,
                VehicleId = result.VehicleId,
                Mileage = result.Mileage,
                Metric = result.Metric
            };
            return PartialView("_ReminderRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeleteReminderRecordById(int reminderRecordId)
        {
            var result = _reminderRecordDataAccess.DeleteReminderRecordById(reminderRecordId);
            return Json(result);
        }
        #endregion
    }
}

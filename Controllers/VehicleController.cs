using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using CarCareTracker.Helper;
using CsvHelper;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using CarCareTracker.External.Implementations;
using CarCareTracker.MapProfile;

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
        private readonly IUpgradeRecordDataAccess _upgradeRecordDataAccess;
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
            IUpgradeRecordDataAccess upgradeRecordDataAccess,
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
            _upgradeRecordDataAccess = upgradeRecordDataAccess;
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
                _upgradeRecordDataAccess.DeleteAllUpgradeRecordsByVehicleId(vehicleId) &&
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
        public IActionResult GetBulkImportModalPartialView(ImportMode mode)
        {
            return PartialView("_BulkDataImporter", mode);
        }
        [HttpPost]
        public IActionResult ImportToVehicleIdFromCsv(int vehicleId, ImportMode mode, string fileName)
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
                    config.PrepareHeaderForMatch = args => { return args.Header.Trim().ToLower(); };
                    using (var csv = new CsvReader(reader, config))
                    {
                        csv.Context.RegisterClassMap<FuellyMapper>();
                        var records = csv.GetRecords<ImportModel>().ToList();
                        if (records.Any())
                        {
                            foreach (ImportModel importModel in records)
                            {
                                if (mode == ImportMode.GasRecord)
                                {
                                    //convert to gas model.
                                    var convertedRecord = new GasRecord()
                                    {
                                        VehicleId = vehicleId,
                                        Date = DateTime.Parse(importModel.Date),
                                        Mileage = int.Parse(importModel.Odometer, NumberStyles.Any),
                                        Gallons = decimal.Parse(importModel.FuelConsumed, NumberStyles.Any)
                                    };
                                    if (string.IsNullOrWhiteSpace(importModel.Cost) && !string.IsNullOrWhiteSpace(importModel.Price))
                                    {
                                        //cost was not given but price is.
                                        //fuelly sometimes exports CSVs without total cost.
                                        var parsedPrice = decimal.Parse(importModel.Price, NumberStyles.Any);
                                        convertedRecord.Cost = convertedRecord.Gallons * parsedPrice;
                                    } else
                                    {
                                        convertedRecord.Cost = decimal.Parse(importModel.Cost, NumberStyles.Any);
                                    }
                                    if (string.IsNullOrWhiteSpace(importModel.IsFillToFull) && !string.IsNullOrWhiteSpace(importModel.PartialFuelUp))
                                    {
                                        var parsedBool = importModel.PartialFuelUp.Trim() == "1";
                                        convertedRecord.IsFillToFull = !parsedBool;
                                    } else if (!string.IsNullOrWhiteSpace(importModel.IsFillToFull))
                                    {
                                        var parsedBool = importModel.IsFillToFull.Trim() == "1" || importModel.IsFillToFull.Trim() == "Full";
                                        convertedRecord.IsFillToFull = parsedBool;
                                    }
                                    //insert record into db, check to make sure fuelconsumed is not zero so we don't get a divide by zero error.
                                    if (convertedRecord.Gallons > 0)
                                    {
                                        _gasRecordDataAccess.SaveGasRecordToVehicle(convertedRecord);
                                    }
                                }
                                else if (mode == ImportMode.ServiceRecord)
                                {
                                    var convertedRecord = new ServiceRecord()
                                    {
                                        VehicleId = vehicleId,
                                        Date = DateTime.Parse(importModel.Date),
                                        Mileage = int.Parse(importModel.Odometer, NumberStyles.Any),
                                        Description = string.IsNullOrWhiteSpace(importModel.Description) ? $"Service Record on {importModel.Date}" : importModel.Description,
                                        Notes = string.IsNullOrWhiteSpace(importModel.Notes) ? "" : importModel.Notes,
                                        Cost = decimal.Parse(importModel.Cost, NumberStyles.Any)
                                    };
                                    _serviceRecordDataAccess.SaveServiceRecordToVehicle(convertedRecord);
                                }
                                else if (mode == ImportMode.RepairRecord)
                                {
                                    var convertedRecord = new CollisionRecord()
                                    {
                                        VehicleId = vehicleId,
                                        Date = DateTime.Parse(importModel.Date),
                                        Mileage = int.Parse(importModel.Odometer, NumberStyles.Any),
                                        Description = string.IsNullOrWhiteSpace(importModel.Description) ? $"Repair Record on {importModel.Date}" : importModel.Description,
                                        Notes = string.IsNullOrWhiteSpace(importModel.Notes) ? "" : importModel.Notes,
                                        Cost = decimal.Parse(importModel.Cost, NumberStyles.Any)
                                    };
                                    _collisionRecordDataAccess.SaveCollisionRecordToVehicle(convertedRecord);
                                }
                                else if (mode == ImportMode.TaxRecord)
                                {
                                    var convertedRecord = new TaxRecord()
                                    {
                                        VehicleId = vehicleId,
                                        Date = DateTime.Parse(importModel.Date),
                                        Description = string.IsNullOrWhiteSpace(importModel.Description) ? $"Tax Record on {importModel.Date}" : importModel.Description,
                                        Notes = string.IsNullOrWhiteSpace(importModel.Notes) ? "" : importModel.Notes,
                                        Cost = decimal.Parse(importModel.Cost, NumberStyles.Any)
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
            bool useUKMPG = bool.Parse(_config[nameof(UserConfig.UseUKMPG)]);
            var computedResults = new List<GasRecordViewModel>();
            int previousMileage = 0;
            decimal unFactoredConsumption = 0.00M;
            int unFactoredMileage = 0;
            //perform computation.
            for (int i = 0; i < result.Count; i++)
            {
                var currentObject = result[i];
                decimal convertedConsumption;
                if (useUKMPG && useMPG)
                {
                    //if we're using UK MPG and the user wants imperial calculation insteace of l/100km
                    //if UK MPG is selected then the gas consumption are stored in liters but need to convert into UK gallons for computation.
                    convertedConsumption = currentObject.Gallons / 4.546M;
                } else
                {
                    convertedConsumption = currentObject.Gallons;
                }
                if (i > 0)
                {
                    var deltaMileage = currentObject.Mileage - previousMileage;
                    var gasRecordViewModel = new GasRecordViewModel()
                    {
                        Id = currentObject.Id,
                        VehicleId = currentObject.VehicleId,
                        Date = currentObject.Date.ToShortDateString(),
                        Mileage = currentObject.Mileage,
                        Gallons = convertedConsumption,
                        Cost = currentObject.Cost,
                        DeltaMileage = deltaMileage,
                        CostPerGallon = (currentObject.Cost / convertedConsumption)
                    };
                    if (currentObject.IsFillToFull)
                    {
                        //if user filled to full.
                        gasRecordViewModel.MilesPerGallon = useMPG ? ((unFactoredMileage + deltaMileage) / (unFactoredConsumption + convertedConsumption)) : 100 / ((unFactoredMileage + deltaMileage) / (unFactoredConsumption + convertedConsumption));
                        //reset unFactored vars
                        unFactoredConsumption = 0;
                        unFactoredMileage = 0;
                    }
                    else
                    {
                        unFactoredConsumption += convertedConsumption;
                        unFactoredMileage += deltaMileage;
                        gasRecordViewModel.MilesPerGallon = 0;
                    }
                    computedResults.Add(gasRecordViewModel);
                }
                else
                {
                    computedResults.Add(new GasRecordViewModel()
                    {
                        Id = currentObject.Id,
                        VehicleId = currentObject.VehicleId,
                        Date = currentObject.Date.ToShortDateString(),
                        Mileage = currentObject.Mileage,
                        Gallons = convertedConsumption,
                        Cost = currentObject.Cost,
                        DeltaMileage = 0,
                        MilesPerGallon = 0,
                        CostPerGallon = (currentObject.Cost / convertedConsumption)
                    });
                }
                previousMileage = currentObject.Mileage;
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
            var upgradeRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
            if (year != default)
            {
                serviceRecords.RemoveAll(x => x.Date.Year != year);
                gasRecords.RemoveAll(x => x.Date.Year != year);
                collisionRecords.RemoveAll(x => x.Date.Year != year);
                taxRecords.RemoveAll(x => x.Date.Year != year);
                upgradeRecords.RemoveAll(x => x.Date.Year != year);
            }
            var viewModel = new CostMakeUpForVehicle
            {
                ServiceRecordSum = serviceRecords.Sum(x => x.Cost),
                GasRecordSum = gasRecords.Sum(x => x.Cost),
                CollisionRecordSum = collisionRecords.Sum(x => x.Cost),
                TaxRecordSum = taxRecords.Sum(x => x.Cost),
                UpgradeRecordSum = upgradeRecords.Sum(x=>x.Cost)
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
            var upgradeRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
            if (upgradeRecords.Any())
            {
                numbersArray.Add(upgradeRecords.Max(x => x.Mileage));
            }
            return numbersArray.Any() ? numbersArray.Max() : 0;
        }
        private List<ReminderRecordViewModel> GetRemindersAndUrgency(int vehicleId)
        {
            var currentMileage = GetMaxMileage(vehicleId);
            var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
            List<ReminderRecordViewModel> reminderViewModels = new List<ReminderRecordViewModel>();
            foreach (var reminder in reminders)
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
                }
                else if (reminder.Metric == ReminderMetric.Date)
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
                }
                else if (reminder.Metric == ReminderMetric.Odometer)
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
            if (result.Where(x => x.Urgency == ReminderUrgency.VeryUrgent || x.Urgency == ReminderUrgency.PastDue).Any())
            {
                return Json(true);
            }
            return Json(false);
        }
        [HttpGet]
        public IActionResult GetReminderRecordsByVehicleId(int vehicleId)
        {
            var result = GetRemindersAndUrgency(vehicleId);
            result = result.OrderByDescending(x => x.Urgency).ToList();
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
        #region "Upgrade Records"
        [HttpGet]
        public IActionResult GetUpgradeRecordsByVehicleId(int vehicleId)
        {
            var result = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
            if (_useDescending)
            {
                result = result.OrderByDescending(x => x.Date).ThenByDescending(x => x.Mileage).ToList();
            }
            else
            {
                result = result.OrderBy(x => x.Date).ThenBy(x => x.Mileage).ToList();
            }
            return PartialView("_UpgradeRecords", result);
        }
        [HttpPost]
        public IActionResult SaveUpgradeRecordToVehicleId(UpgradeRecordInput upgradeRecord)
        {
            //move files from temp.
            upgradeRecord.Files = upgradeRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var result = _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(upgradeRecord.ToUpgradeRecord());
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddUpgradeRecordPartialView()
        {
            return PartialView("_UpgradeRecordModal", new UpgradeRecordInput());
        }
        [HttpGet]
        public IActionResult GetUpgradeRecordForEditById(int upgradeRecordId)
        {
            var result = _upgradeRecordDataAccess.GetUpgradeRecordById(upgradeRecordId);
            //convert to Input object.
            var convertedResult = new UpgradeRecordInput
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
            return PartialView("_UpgradeRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeleteUpgradeRecordById(int upgradeRecordId)
        {
            var result = _upgradeRecordDataAccess.DeleteUpgradeRecordById(upgradeRecordId);
            return Json(result);
        }
        #endregion
    }
}

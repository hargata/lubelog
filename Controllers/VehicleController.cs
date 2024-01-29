using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using CarCareTracker.Helper;
using CsvHelper;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using CarCareTracker.MapProfile;
using System.Security.Claims;
using CarCareTracker.Logic;
using CarCareTracker.Filter;

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
        private readonly ISupplyRecordDataAccess _supplyRecordDataAccess;
        private readonly IPlanRecordDataAccess _planRecordDataAccess;
        private readonly IOdometerRecordDataAccess _odometerRecordDataAccess;
        private readonly IWebHostEnvironment _webEnv;
        private readonly IConfigHelper _config;
        private readonly IFileHelper _fileHelper;
        private readonly IGasHelper _gasHelper;
        private readonly IReminderHelper _reminderHelper;
        private readonly IReportHelper _reportHelper;
        private readonly IUserLogic _userLogic;

        public VehicleController(ILogger<VehicleController> logger,
            IFileHelper fileHelper,
            IGasHelper gasHelper,
            IReminderHelper reminderHelper,
            IReportHelper reportHelper,
            IVehicleDataAccess dataAccess,
            INoteDataAccess noteDataAccess,
            IServiceRecordDataAccess serviceRecordDataAccess,
            IGasRecordDataAccess gasRecordDataAccess,
            ICollisionRecordDataAccess collisionRecordDataAccess,
            ITaxRecordDataAccess taxRecordDataAccess,
            IReminderRecordDataAccess reminderRecordDataAccess,
            IUpgradeRecordDataAccess upgradeRecordDataAccess,
            ISupplyRecordDataAccess supplyRecordDataAccess,
            IPlanRecordDataAccess planRecordDataAccess,
            IOdometerRecordDataAccess odometerRecordDataAccess,
            IUserLogic userLogic,
            IWebHostEnvironment webEnv,
            IConfigHelper config)
        {
            _logger = logger;
            _dataAccess = dataAccess;
            _noteDataAccess = noteDataAccess;
            _fileHelper = fileHelper;
            _gasHelper = gasHelper;
            _reminderHelper = reminderHelper;
            _reportHelper = reportHelper;
            _serviceRecordDataAccess = serviceRecordDataAccess;
            _gasRecordDataAccess = gasRecordDataAccess;
            _collisionRecordDataAccess = collisionRecordDataAccess;
            _taxRecordDataAccess = taxRecordDataAccess;
            _reminderRecordDataAccess = reminderRecordDataAccess;
            _upgradeRecordDataAccess = upgradeRecordDataAccess;
            _supplyRecordDataAccess = supplyRecordDataAccess;
            _planRecordDataAccess = planRecordDataAccess;
            _odometerRecordDataAccess = odometerRecordDataAccess;
            _userLogic = userLogic;
            _webEnv = webEnv;
            _config = config;
        }
        private int GetUserID()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult Index(int vehicleId)
        {
            var data = _dataAccess.GetVehicleById(vehicleId);
            UpdateRecurringTaxes(vehicleId);
            return View(data);
        }
        [HttpGet]
        public IActionResult AddVehiclePartialView()
        {
            return PartialView("_VehicleModal", new Vehicle());
        }
        [TypeFilter(typeof(CollaboratorFilter))]
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
                bool isNewAddition = vehicleInput.Id == default;
                if (!isNewAddition)
                {
                    if (!_userLogic.UserCanEditVehicle(GetUserID(), vehicleInput.Id))
                    {
                        return View("401");
                    }
                }
                //move image from temp folder to images folder.
                vehicleInput.ImageLocation = _fileHelper.MoveFileFromTemp(vehicleInput.ImageLocation, "images/");
                //save vehicle.
                var result = _dataAccess.SaveVehicle(vehicleInput);
                if (isNewAddition)
                {
                    _userLogic.AddUserAccessToVehicle(GetUserID(), vehicleInput.Id);
                }
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Saving Vehicle");
                return Json(false);
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        public IActionResult DeleteVehicle(int vehicleId)
        {
            //Delete all service records, gas records, notes, etc.
            var result = _gasRecordDataAccess.DeleteAllGasRecordsByVehicleId(vehicleId) &&
                _serviceRecordDataAccess.DeleteAllServiceRecordsByVehicleId(vehicleId) &&
                _collisionRecordDataAccess.DeleteAllCollisionRecordsByVehicleId(vehicleId) &&
                _taxRecordDataAccess.DeleteAllTaxRecordsByVehicleId(vehicleId) &&
                _noteDataAccess.DeleteAllNotesByVehicleId(vehicleId) &&
                _reminderRecordDataAccess.DeleteAllReminderRecordsByVehicleId(vehicleId) &&
                _upgradeRecordDataAccess.DeleteAllUpgradeRecordsByVehicleId(vehicleId) &&
                _planRecordDataAccess.DeleteAllPlanRecordsByVehicleId(vehicleId) &&
                _supplyRecordDataAccess.DeleteAllSupplyRecordsByVehicleId(vehicleId) &&
                _userLogic.DeleteAllAccessToVehicle(vehicleId) &&
                _dataAccess.DeleteVehicle(vehicleId);
            return Json(result);
        }
        #region "Bulk Imports and Exports"
        [HttpGet]
        public IActionResult GetBulkImportModalPartialView(ImportMode mode)
        {
            return PartialView("_BulkDataImporter", mode);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult ExportFromVehicleToCsv(int vehicleId, ImportMode mode)
        {
            if (vehicleId == default)
            {
                return Json(false);
            }
            string uploadDirectory = "temp/";
            string uploadPath = Path.Combine(_webEnv.WebRootPath, uploadDirectory);
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);
            if (mode == ImportMode.ServiceRecord)
            {
                var fileNameToExport = $"temp/{Guid.NewGuid()}.csv";
                var fullExportFilePath = _fileHelper.GetFullFilePath(fileNameToExport, false);
                var vehicleRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
                if (vehicleRecords.Any())
                {
                    var exportData = vehicleRecords.Select(x => new ServiceRecordExportModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString("C"), Notes = x.Notes, Odometer = x.Mileage.ToString() });
                    using (var writer = new StreamWriter(fullExportFilePath))
                    {
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            csv.WriteRecords(exportData);
                        }
                    }
                    return Json($"/{fileNameToExport}");
                }
            }
            else if (mode == ImportMode.RepairRecord)
            {
                var fileNameToExport = $"temp/{Guid.NewGuid()}.csv";
                var fullExportFilePath = _fileHelper.GetFullFilePath(fileNameToExport, false);
                var vehicleRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
                if (vehicleRecords.Any())
                {
                    var exportData = vehicleRecords.Select(x => new ServiceRecordExportModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString("C"), Notes = x.Notes, Odometer = x.Mileage.ToString() });
                    using (var writer = new StreamWriter(fullExportFilePath))
                    {
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            csv.WriteRecords(exportData);
                        }
                    }
                    return Json($"/{fileNameToExport}");
                }
            }
            else if (mode == ImportMode.UpgradeRecord)
            {
                var fileNameToExport = $"temp/{Guid.NewGuid()}.csv";
                var fullExportFilePath = _fileHelper.GetFullFilePath(fileNameToExport, false);
                var vehicleRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
                if (vehicleRecords.Any())
                {
                    var exportData = vehicleRecords.Select(x => new ServiceRecordExportModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString("C"), Notes = x.Notes, Odometer = x.Mileage.ToString() });
                    using (var writer = new StreamWriter(fullExportFilePath))
                    {
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            csv.WriteRecords(exportData);
                        }
                    }
                    return Json($"/{fileNameToExport}");
                }
            }
            else if (mode == ImportMode.OdometerRecord)
            {
                var fileNameToExport = $"temp/{Guid.NewGuid()}.csv";
                var fullExportFilePath = _fileHelper.GetFullFilePath(fileNameToExport, false);
                var vehicleRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
                if (vehicleRecords.Any())
                {
                    var exportData = vehicleRecords.Select(x => new OdometerRecordExportModel { Date = x.Date.ToShortDateString(), Notes = x.Notes, Odometer = x.Mileage.ToString() });
                    using (var writer = new StreamWriter(fullExportFilePath))
                    {
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            csv.WriteRecords(exportData);
                        }
                    }
                    return Json($"/{fileNameToExport}");
                }
            }
            else if (mode == ImportMode.SupplyRecord)
            {
                var fileNameToExport = $"temp/{Guid.NewGuid()}.csv";
                var fullExportFilePath = _fileHelper.GetFullFilePath(fileNameToExport, false);
                var vehicleRecords = _supplyRecordDataAccess.GetSupplyRecordsByVehicleId(vehicleId);
                if (vehicleRecords.Any())
                {
                    var exportData = vehicleRecords.Select(x => new SupplyRecordExportModel
                    {
                        Date = x.Date.ToShortDateString(),
                        Description = x.Description,
                        Cost = x.Cost.ToString("C"),
                        PartNumber = x.PartNumber,
                        PartQuantity = x.Quantity.ToString(),
                        PartSupplier = x.PartSupplier,
                        Notes = x.Notes
                    });
                    using (var writer = new StreamWriter(fullExportFilePath))
                    {
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            csv.WriteRecords(exportData);
                        }
                    }
                    return Json($"/{fileNameToExport}");
                }
            }
            else if (mode == ImportMode.TaxRecord)
            {
                var fileNameToExport = $"temp/{Guid.NewGuid()}.csv";
                var fullExportFilePath = _fileHelper.GetFullFilePath(fileNameToExport, false);
                var vehicleRecords = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
                if (vehicleRecords.Any())
                {
                    var exportData = vehicleRecords.Select(x => new TaxRecordExportModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString("C"), Notes = x.Notes });
                    using (var writer = new StreamWriter(fullExportFilePath))
                    {
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            csv.WriteRecords(exportData);
                        }
                    }
                    return Json($"/{fileNameToExport}");
                }
            }
            else if (mode == ImportMode.PlanRecord)
            {
                var fileNameToExport = $"temp/{Guid.NewGuid()}.csv";
                var fullExportFilePath = _fileHelper.GetFullFilePath(fileNameToExport, false);
                var vehicleRecords = _planRecordDataAccess.GetPlanRecordsByVehicleId(vehicleId);
                if (vehicleRecords.Any())
                {
                    var exportData = vehicleRecords.Select(x => new PlanRecordExportModel
                    {
                        DateCreated = x.DateCreated.ToString("G"),
                        DateModified = x.DateModified.ToString("G"),
                        Description = x.Description,
                        Cost = x.Cost.ToString("C"),
                        Type = x.ImportMode.ToString(),
                        Priority = x.Priority.ToString(),
                        Progress = x.Progress.ToString(),
                        Notes = x.Notes
                    });
                    using (var writer = new StreamWriter(fullExportFilePath))
                    {
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            csv.WriteRecords(exportData);
                        }
                    }
                    return Json($"/{fileNameToExport}");
                }
            }
            else if (mode == ImportMode.GasRecord)
            {
                var fileNameToExport = $"temp/{Guid.NewGuid()}.csv";
                var fullExportFilePath = _fileHelper.GetFullFilePath(fileNameToExport, false);
                var vehicleRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
                bool useMPG = _config.GetUserConfig(User).UseMPG;
                bool useUKMPG = _config.GetUserConfig(User).UseUKMPG;
                var convertedRecords = _gasHelper.GetGasRecordViewModels(vehicleRecords, useMPG, useUKMPG);
                var exportData = convertedRecords.Select(x => new GasRecordExportModel
                {
                    Date = x.Date.ToString(),
                    Cost = x.Cost.ToString(),
                    FuelConsumed = x.Gallons.ToString(),
                    FuelEconomy = x.MilesPerGallon.ToString(),
                    Odometer = x.Mileage.ToString(),
                    IsFillToFull = x.IsFillToFull.ToString(),
                    MissedFuelUp = x.MissedFuelUp.ToString(),
                    Notes = x.Notes
                });
                using (var writer = new StreamWriter(fullExportFilePath))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(exportData);
                    }
                }
                return Json($"/{fileNameToExport}");
            }
            return Json(false);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
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
                        csv.Context.RegisterClassMap<ImportMapper>();
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
                                        Mileage = decimal.ToInt32(decimal.Parse(importModel.Odometer, NumberStyles.Any)),
                                        Gallons = decimal.Parse(importModel.FuelConsumed, NumberStyles.Any),
                                        Notes = string.IsNullOrWhiteSpace(importModel.Notes) ? "" : importModel.Notes
                                    };
                                    if (string.IsNullOrWhiteSpace(importModel.Cost) && !string.IsNullOrWhiteSpace(importModel.Price))
                                    {
                                        //cost was not given but price is.
                                        //fuelly sometimes exports CSVs without total cost.
                                        var parsedPrice = decimal.Parse(importModel.Price, NumberStyles.Any);
                                        convertedRecord.Cost = convertedRecord.Gallons * parsedPrice;
                                    }
                                    else
                                    {
                                        convertedRecord.Cost = decimal.Parse(importModel.Cost, NumberStyles.Any);
                                    }
                                    if (string.IsNullOrWhiteSpace(importModel.IsFillToFull) && !string.IsNullOrWhiteSpace(importModel.PartialFuelUp))
                                    {
                                        var parsedBool = importModel.PartialFuelUp.Trim() == "1";
                                        convertedRecord.IsFillToFull = !parsedBool;
                                    }
                                    else if (!string.IsNullOrWhiteSpace(importModel.IsFillToFull))
                                    {
                                        var possibleFillToFullValues = new List<string> { "1", "true", "full" };
                                        var parsedBool = possibleFillToFullValues.Contains(importModel.IsFillToFull.Trim().ToLower());
                                        convertedRecord.IsFillToFull = parsedBool;
                                    }
                                    if (!string.IsNullOrWhiteSpace(importModel.MissedFuelUp))
                                    {
                                        var possibleMissedFuelUpValues = new List<string> { "1", "true" };
                                        var parsedBool = possibleMissedFuelUpValues.Contains(importModel.MissedFuelUp.Trim().ToLower());
                                        convertedRecord.MissedFuelUp = parsedBool;
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
                                        Mileage = decimal.ToInt32(decimal.Parse(importModel.Odometer, NumberStyles.Any)),
                                        Description = string.IsNullOrWhiteSpace(importModel.Description) ? $"Service Record on {importModel.Date}" : importModel.Description,
                                        Notes = string.IsNullOrWhiteSpace(importModel.Notes) ? "" : importModel.Notes,
                                        Cost = decimal.Parse(importModel.Cost, NumberStyles.Any)
                                    };
                                    _serviceRecordDataAccess.SaveServiceRecordToVehicle(convertedRecord);
                                }
                                else if (mode == ImportMode.OdometerRecord)
                                {
                                    var convertedRecord = new OdometerRecord()
                                    {
                                        VehicleId = vehicleId,
                                        Date = DateTime.Parse(importModel.Date),
                                        Mileage = decimal.ToInt32(decimal.Parse(importModel.Odometer, NumberStyles.Any)),
                                        Notes = string.IsNullOrWhiteSpace(importModel.Notes) ? "" : importModel.Notes
                                    };
                                    _odometerRecordDataAccess.SaveOdometerRecordToVehicle(convertedRecord);
                                }
                                else if (mode == ImportMode.PlanRecord)
                                {
                                    var progressIsEnum = Enum.TryParse(importModel.Progress, out PlanProgress parsedProgress);
                                    var typeIsEnum = Enum.TryParse(importModel.Type, out ImportMode parsedType);
                                    var priorityIsEnum = Enum.TryParse(importModel.Priority, out PlanPriority parsedPriority);
                                    var convertedRecord = new PlanRecord()
                                    {
                                        VehicleId = vehicleId,
                                        DateCreated = DateTime.Parse(importModel.DateCreated),
                                        DateModified = DateTime.Parse(importModel.DateModified),
                                        Progress = parsedProgress,
                                        ImportMode = parsedType,
                                        Priority = parsedPriority,
                                        Description = string.IsNullOrWhiteSpace(importModel.Description) ? $"Plan Record on {importModel.DateCreated}" : importModel.Description,
                                        Notes = string.IsNullOrWhiteSpace(importModel.Notes) ? "" : importModel.Notes,
                                        Cost = decimal.Parse(importModel.Cost, NumberStyles.Any)
                                    };
                                    _planRecordDataAccess.SavePlanRecordToVehicle(convertedRecord);
                                }
                                else if (mode == ImportMode.RepairRecord)
                                {
                                    var convertedRecord = new CollisionRecord()
                                    {
                                        VehicleId = vehicleId,
                                        Date = DateTime.Parse(importModel.Date),
                                        Mileage = decimal.ToInt32(decimal.Parse(importModel.Odometer, NumberStyles.Any)),
                                        Description = string.IsNullOrWhiteSpace(importModel.Description) ? $"Repair Record on {importModel.Date}" : importModel.Description,
                                        Notes = string.IsNullOrWhiteSpace(importModel.Notes) ? "" : importModel.Notes,
                                        Cost = decimal.Parse(importModel.Cost, NumberStyles.Any)
                                    };
                                    _collisionRecordDataAccess.SaveCollisionRecordToVehicle(convertedRecord);
                                }
                                else if (mode == ImportMode.UpgradeRecord)
                                {
                                    var convertedRecord = new UpgradeRecord()
                                    {
                                        VehicleId = vehicleId,
                                        Date = DateTime.Parse(importModel.Date),
                                        Mileage = decimal.ToInt32(decimal.Parse(importModel.Odometer, NumberStyles.Any)),
                                        Description = string.IsNullOrWhiteSpace(importModel.Description) ? $"Upgrade Record on {importModel.Date}" : importModel.Description,
                                        Notes = string.IsNullOrWhiteSpace(importModel.Notes) ? "" : importModel.Notes,
                                        Cost = decimal.Parse(importModel.Cost, NumberStyles.Any)
                                    };
                                    _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(convertedRecord);
                                }
                                else if (mode == ImportMode.SupplyRecord)
                                {
                                    var convertedRecord = new SupplyRecord()
                                    {
                                        VehicleId = vehicleId,
                                        Date = DateTime.Parse(importModel.Date),
                                        PartNumber = importModel.PartNumber,
                                        PartSupplier = importModel.PartSupplier,
                                        Quantity = decimal.Parse(importModel.PartQuantity, NumberStyles.Any),
                                        Description = importModel.Description,
                                        Cost = decimal.Parse(importModel.Cost, NumberStyles.Any),
                                        Notes = importModel.Notes
                                    };
                                    _supplyRecordDataAccess.SaveSupplyRecordToVehicle(convertedRecord);
                                }
                                else if (mode == ImportMode.TaxRecord)
                                {
                                    var convertedRecord = new TaxRecord()
                                    {
                                        VehicleId = vehicleId,
                                        Date = DateTime.Parse(importModel.Date),
                                        Description = string.IsNullOrWhiteSpace(importModel.Description) ? $"Fee Record on {importModel.Date}" : importModel.Description,
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
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetGasRecordsByVehicleId(int vehicleId)
        {
            var result = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            //check if the user uses MPG or Liters per 100km.
            var userConfig = _config.GetUserConfig(User);
            bool useMPG = userConfig.UseMPG;
            bool useUKMPG = userConfig.UseUKMPG;
            var computedResults = _gasHelper.GetGasRecordViewModels(result, useMPG, useUKMPG);
            if (userConfig.UseDescending)
            {
                computedResults = computedResults.OrderByDescending(x => DateTime.Parse(x.Date)).ThenByDescending(x => x.Mileage).ToList();
            }
            var vehicleData = _dataAccess.GetVehicleById(vehicleId);
            var vehicleIsElectric = vehicleData.IsElectric;
            var vehicleUseHours = vehicleData.UseHours;
            var viewModel = new GasRecordViewModelContainer()
            {
                UseKwh = vehicleIsElectric,
                UseHours = vehicleUseHours,
                GasRecords = computedResults
            };
            return PartialView("_Gas", viewModel);
        }
        [HttpPost]
        public IActionResult SaveGasRecordToVehicleId(GasRecordInput gasRecord)
        {
            if (gasRecord.Id == default && _config.GetUserConfig(User).EnableAutoOdometerInsert)
            {
                _odometerRecordDataAccess.SaveOdometerRecordToVehicle(new OdometerRecord
                {
                    Date = DateTime.Parse(gasRecord.Date),
                    VehicleId = gasRecord.VehicleId,
                    Mileage = gasRecord.Mileage,
                    Notes = $"Auto Insert From Gas Record. {gasRecord.Notes}"
                });
            }
            gasRecord.Files = gasRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var result = _gasRecordDataAccess.SaveGasRecordToVehicle(gasRecord.ToGasRecord());
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddGasRecordPartialView(int vehicleId)
        {
            var vehicleData = _dataAccess.GetVehicleById(vehicleId);
            var vehicleIsElectric = vehicleData.IsElectric;
            var vehicleUseHours = vehicleData.UseHours;
            return PartialView("_GasModal", new GasRecordInputContainer() { UseKwh = vehicleIsElectric, UseHours = vehicleUseHours, GasRecord = new GasRecordInput() });
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
                IsFillToFull = result.IsFillToFull,
                MissedFuelUp = result.MissedFuelUp,
                Notes = result.Notes
            };
            var vehicleData = _dataAccess.GetVehicleById(convertedResult.VehicleId);
            var vehicleIsElectric = vehicleData.IsElectric;
            var vehicleUseHours = vehicleData.UseHours;
            var viewModel = new GasRecordInputContainer()
            {
                UseKwh = vehicleIsElectric,
                UseHours = vehicleUseHours,
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
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetServiceRecordsByVehicleId(int vehicleId)
        {
            var result = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
            bool _useDescending = _config.GetUserConfig(User).UseDescending;
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
            if (serviceRecord.Id == default && _config.GetUserConfig(User).EnableAutoOdometerInsert)
            {
                _odometerRecordDataAccess.SaveOdometerRecordToVehicle(new OdometerRecord
                {
                    Date = DateTime.Parse(serviceRecord.Date),
                    VehicleId = serviceRecord.VehicleId,
                    Mileage = serviceRecord.Mileage,
                    Notes = $"Auto Insert From Service Record: {serviceRecord.Description}"
                });
            }
            //move files from temp.
            serviceRecord.Files = serviceRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var result = _serviceRecordDataAccess.SaveServiceRecordToVehicle(serviceRecord.ToServiceRecord());
            if (result && serviceRecord.Supplies.Any())
            {
                RequisitionSupplyRecordsByUsage(serviceRecord.Supplies);
            }
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
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetCollisionRecordsByVehicleId(int vehicleId)
        {
            var result = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
            bool _useDescending = _config.GetUserConfig(User).UseDescending;
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
            if (collisionRecord.Id == default && _config.GetUserConfig(User).EnableAutoOdometerInsert)
            {
                _odometerRecordDataAccess.SaveOdometerRecordToVehicle(new OdometerRecord
                {
                    Date = DateTime.Parse(collisionRecord.Date),
                    VehicleId = collisionRecord.VehicleId,
                    Mileage = collisionRecord.Mileage,
                    Notes = $"Auto Insert From Repair Record: {collisionRecord.Description}"
                });
            }
            //move files from temp.
            collisionRecord.Files = collisionRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var result = _collisionRecordDataAccess.SaveCollisionRecordToVehicle(collisionRecord.ToCollisionRecord());
            if (result && collisionRecord.Supplies.Any())
            {
                RequisitionSupplyRecordsByUsage(collisionRecord.Supplies);
            }
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
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetTaxRecordsByVehicleId(int vehicleId)
        {
            var result = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
            bool _useDescending = _config.GetUserConfig(User).UseDescending;
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
        private void UpdateRecurringTaxes(int vehicleId)
        {
            var result = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
            var recurringFees = result.Where(x => x.IsRecurring);
            if (recurringFees.Any())
            {
                foreach(TaxRecord recurringFee in recurringFees)
                {
                    var newDate = recurringFee.Date.AddMonths((int)recurringFee.RecurringInterval);
                    if (DateTime.Now > newDate){
                        recurringFee.IsRecurring = false;
                        var newRecurringFee = new TaxRecord()
                        {
                            VehicleId = recurringFee.VehicleId,
                            Date = newDate,
                            Description = recurringFee.Description,
                            Cost = recurringFee.Cost,
                            IsRecurring = true,
                            Notes = recurringFee.Notes,
                            RecurringInterval = recurringFee.RecurringInterval,
                            Files = recurringFee.Files
                        };
                        _taxRecordDataAccess.SaveTaxRecordToVehicle(recurringFee);
                        _taxRecordDataAccess.SaveTaxRecordToVehicle(newRecurringFee);
                    }
                }
            }
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
                IsRecurring = result.IsRecurring,
                RecurringInterval = result.RecurringInterval,
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
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetReportPartialView(int vehicleId)
        {
            //get records
            var serviceRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
            var gasRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            var collisionRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
            var taxRecords = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
            var upgradeRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
            var viewModel = new ReportViewModel();
            //get totalCostMakeUp
            viewModel.CostMakeUpForVehicle = new CostMakeUpForVehicle
            {
                ServiceRecordSum = serviceRecords.Sum(x => x.Cost),
                GasRecordSum = gasRecords.Sum(x => x.Cost),
                CollisionRecordSum = collisionRecords.Sum(x => x.Cost),
                TaxRecordSum = taxRecords.Sum(x => x.Cost),
                UpgradeRecordSum = upgradeRecords.Sum(x => x.Cost)
            };
            //get costbymonth
            List<CostForVehicleByMonth> allCosts = StaticHelper.GetBaseLineCosts();
            allCosts.AddRange(_reportHelper.GetServiceRecordSum(serviceRecords, 0));
            allCosts.AddRange(_reportHelper.GetRepairRecordSum(collisionRecords, 0));
            allCosts.AddRange(_reportHelper.GetUpgradeRecordSum(upgradeRecords, 0));
            allCosts.AddRange(_reportHelper.GetGasRecordSum(gasRecords, 0));
            allCosts.AddRange(_reportHelper.GetTaxRecordSum(taxRecords, 0));
            viewModel.CostForVehicleByMonth = allCosts.GroupBy(x => new { x.MonthName, x.MonthId }).OrderBy(x => x.Key.MonthId).Select(x => new CostForVehicleByMonth
            {
                MonthName = x.Key.MonthName,
                Cost = x.Sum(y => y.Cost)
            }).ToList();
            //get reminders
            var reminders = GetRemindersAndUrgency(vehicleId, DateTime.Now);
            viewModel.ReminderMakeUpForVehicle = new ReminderMakeUpForVehicle
            {
                NotUrgentCount = reminders.Where(x => x.Urgency == ReminderUrgency.NotUrgent).Count(),
                UrgentCount = reminders.Where(x => x.Urgency == ReminderUrgency.Urgent).Count(),
                VeryUrgentCount = reminders.Where(x => x.Urgency == ReminderUrgency.VeryUrgent).Count(),
                PastDueCount = reminders.Where(x => x.Urgency == ReminderUrgency.PastDue).Count()
            };
            //populate year dropdown.
            var numbersArray = new List<int>();
            if (serviceRecords.Any())
            {
                numbersArray.Add(serviceRecords.Min(x => x.Date.Year));
            }
            if (collisionRecords.Any())
            {
                numbersArray.Add(collisionRecords.Min(x => x.Date.Year));
            }
            if (gasRecords.Any())
            {
                numbersArray.Add(gasRecords.Min(x => x.Date.Year));
            }
            if (upgradeRecords.Any())
            {
                numbersArray.Add(upgradeRecords.Min(x => x.Date.Year));
            }
            var minYear = numbersArray.Any() ? numbersArray.Min() : DateTime.Now.AddYears(-5).Year;
            var yearDifference = DateTime.Now.Year - minYear + 1;
            for (int i = 0; i < yearDifference; i++)
            {
                viewModel.Years.Add(DateTime.Now.AddYears(i * -1).Year);
            }
            //get collaborators
            var collaborators = _userLogic.GetCollaboratorsForVehicle(vehicleId);
            viewModel.Collaborators = collaborators;
            //get MPG per month.
            var userConfig = _config.GetUserConfig(User);
            var mileageData = _gasHelper.GetGasRecordViewModels(gasRecords, userConfig.UseMPG, userConfig.UseUKMPG);
            mileageData.RemoveAll(x => x.MilesPerGallon == default);
            var monthlyMileageData = StaticHelper.GetBaseLineCostsNoMonthName();
            monthlyMileageData.AddRange(mileageData.GroupBy(x => x.MonthId).Select(x => new CostForVehicleByMonth
            {
                MonthId = x.Key,
                Cost = x.Average(y => y.MilesPerGallon)
            }));
            monthlyMileageData = monthlyMileageData.GroupBy(x => x.MonthId).OrderBy(x => x.Key).Select(x => new CostForVehicleByMonth
            {
                MonthId = x.Key,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key),
                Cost = x.Sum(y=>y.Cost)
            }).ToList();
            viewModel.FuelMileageForVehicleByMonth = monthlyMileageData;
            return PartialView("_Report", viewModel);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetCollaboratorsForVehicle(int vehicleId)
        {
            var result = _userLogic.GetCollaboratorsForVehicle(vehicleId);
            return PartialView("_Collaborators", result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        public IActionResult AddCollaboratorsToVehicle(int vehicleId, string username)
        {
            var result = _userLogic.AddCollaboratorToVehicle(vehicleId, username);
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        public IActionResult DeleteCollaboratorFromVehicle(int userId, int vehicleId)
        {
            var result = _userLogic.DeleteCollaboratorFromVehicle(userId, vehicleId);
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
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
                UpgradeRecordSum = upgradeRecords.Sum(x => x.Cost)
            };
            return PartialView("_CostMakeUpReport", viewModel);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        public IActionResult GetReminderMakeUpByVehicle(int vehicleId, int daysToAdd)
        {
            var reminders = GetRemindersAndUrgency(vehicleId, DateTime.Now.AddDays(daysToAdd));
            var viewModel = new ReminderMakeUpForVehicle
            {
                NotUrgentCount = reminders.Where(x => x.Urgency == ReminderUrgency.NotUrgent).Count(),
                UrgentCount = reminders.Where(x => x.Urgency == ReminderUrgency.Urgent).Count(),
                VeryUrgentCount = reminders.Where(x => x.Urgency == ReminderUrgency.VeryUrgent).Count(),
                PastDueCount = reminders.Where(x => x.Urgency == ReminderUrgency.PastDue).Count()
            };
            return PartialView("_ReminderMakeUpReport", viewModel);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        public IActionResult GetVehicleHistory(int vehicleId)
        {
            var vehicleHistory = new VehicleHistoryViewModel();
            vehicleHistory.VehicleData = _dataAccess.GetVehicleById(vehicleId);
            vehicleHistory.Odometer = GetMaxMileage(vehicleId).ToString("N0");
            List<GenericReportModel> reportData = new List<GenericReportModel>();
            var serviceRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
            var repairRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
            var upgradeRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
            var taxRecords = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
            var gasRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            bool useMPG = _config.GetUserConfig(User).UseMPG;
            bool useUKMPG = _config.GetUserConfig(User).UseUKMPG;
            vehicleHistory.TotalGasCost = gasRecords.Sum(x => x.Cost);
            vehicleHistory.TotalCost = serviceRecords.Sum(x => x.Cost) + repairRecords.Sum(x => x.Cost) + upgradeRecords.Sum(x => x.Cost) + taxRecords.Sum(x => x.Cost);
            var averageMPG = "0";
            var gasViewModels = _gasHelper.GetGasRecordViewModels(gasRecords, useMPG, useUKMPG);
            if (gasViewModels.Any())
            {
                averageMPG = _gasHelper.GetAverageGasMileage(gasViewModels, useMPG);
            }
            vehicleHistory.MPG = averageMPG;
            //insert servicerecords
            reportData.AddRange(serviceRecords.Select(x => new GenericReportModel
            {
                Date = x.Date,
                Odometer = x.Mileage,
                Description = x.Description,
                Notes = x.Notes,
                Cost = x.Cost,
                DataType = ImportMode.ServiceRecord
            }));
            //repair records
            reportData.AddRange(repairRecords.Select(x => new GenericReportModel
            {
                Date = x.Date,
                Odometer = x.Mileage,
                Description = x.Description,
                Notes = x.Notes,
                Cost = x.Cost,
                DataType = ImportMode.RepairRecord
            }));
            reportData.AddRange(upgradeRecords.Select(x => new GenericReportModel
            {
                Date = x.Date,
                Odometer = x.Mileage,
                Description = x.Description,
                Notes = x.Notes,
                Cost = x.Cost,
                DataType = ImportMode.UpgradeRecord
            }));
            reportData.AddRange(taxRecords.Select(x => new GenericReportModel
            {
                Date = x.Date,
                Odometer = 0,
                Description = x.Description,
                Notes = x.Notes,
                Cost = x.Cost,
                DataType = ImportMode.TaxRecord
            }));
            vehicleHistory.VehicleHistory = reportData.OrderBy(x => x.Date).ThenBy(x => x.Odometer).ToList();
            return PartialView("_VehicleHistory", vehicleHistory);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        public IActionResult GetMonthMPGByVehicle(int vehicleId, int year = 0)
        {
            var gasRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            var userConfig = _config.GetUserConfig(User);
            var mileageData = _gasHelper.GetGasRecordViewModels(gasRecords, userConfig.UseMPG, userConfig.UseUKMPG);
            if (year != 0)
            {
                mileageData.RemoveAll(x => DateTime.Parse(x.Date).Year != year);
            }
            mileageData.RemoveAll(x => x.MilesPerGallon == default);
            var monthlyMileageData = StaticHelper.GetBaseLineCostsNoMonthName();
            monthlyMileageData.AddRange(mileageData.GroupBy(x => x.MonthId).Select(x => new CostForVehicleByMonth
            {
                MonthId = x.Key,
                Cost = x.Average(y => y.MilesPerGallon)
            }));
            monthlyMileageData = monthlyMileageData.GroupBy(x => x.MonthId).OrderBy(x => x.Key).Select(x => new CostForVehicleByMonth
            {
                MonthId = x.Key,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key),
                Cost = x.Sum(y => y.Cost)
            }).ToList();
            return PartialView("_MPGByMonthReport", monthlyMileageData);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        public IActionResult GetCostByMonthByVehicle(int vehicleId, List<ImportMode> selectedMetrics, int year = 0)
        {
            List<CostForVehicleByMonth> allCosts = StaticHelper.GetBaseLineCosts();
            if (selectedMetrics.Contains(ImportMode.ServiceRecord))
            {
                var serviceRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
                allCosts.AddRange(_reportHelper.GetServiceRecordSum(serviceRecords, year));
            }
            if (selectedMetrics.Contains(ImportMode.RepairRecord))
            {
                var repairRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
                allCosts.AddRange(_reportHelper.GetRepairRecordSum(repairRecords, year));
            }
            if (selectedMetrics.Contains(ImportMode.UpgradeRecord))
            {
                var upgradeRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
                allCosts.AddRange(_reportHelper.GetUpgradeRecordSum(upgradeRecords, year));
            }
            if (selectedMetrics.Contains(ImportMode.GasRecord))
            {
                var gasRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
                allCosts.AddRange(_reportHelper.GetGasRecordSum(gasRecords, year));
            }
            if (selectedMetrics.Contains(ImportMode.TaxRecord))
            {
                var taxRecords = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
                allCosts.AddRange(_reportHelper.GetTaxRecordSum(taxRecords, year));
            }
            var groupedRecord = allCosts.GroupBy(x => new { x.MonthName, x.MonthId }).OrderBy(x => x.Key.MonthId).Select(x => new CostForVehicleByMonth
            {
                MonthName = x.Key.MonthName,
                Cost = x.Sum(y => y.Cost)
            }).ToList();
            return PartialView("_GasCostByMonthReport", groupedRecord);
        }
        #endregion
        #region "Reminders"
        [TypeFilter(typeof(CollaboratorFilter))]
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
            var odometerRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
            if (odometerRecords.Any())
            {
                numbersArray.Add(odometerRecords.Max(x => x.Mileage));
            }
            return numbersArray.Any() ? numbersArray.Max() : 0;
        }
        private List<ReminderRecordViewModel> GetRemindersAndUrgency(int vehicleId, DateTime dateCompare)
        {
            var currentMileage = GetMaxMileage(vehicleId);
            var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
            List<ReminderRecordViewModel> results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, dateCompare);
            return results;
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetVehicleHaveUrgentOrPastDueReminders(int vehicleId)
        {
            var result = GetRemindersAndUrgency(vehicleId, DateTime.Now);
            //check if user wants auto-refresh past-due reminders
            if (_config.GetUserConfig(User).EnableAutoReminderRefresh)
            {
                //check for past due reminders that are eligible for recurring.
                var pastDueAndRecurring = result.Where(x => x.Urgency == ReminderUrgency.PastDue && x.IsRecurring);
                if (pastDueAndRecurring.Any())
                {
                    foreach (ReminderRecordViewModel reminderRecord in pastDueAndRecurring)
                    {
                        //update based on recurring intervals.
                        //pull reminderRecord based on ID
                        var existingReminder = _reminderRecordDataAccess.GetReminderRecordById(reminderRecord.Id);
                        existingReminder = _reminderHelper.GetUpdatedRecurringReminderRecord(existingReminder);
                        //save to db.
                        _reminderRecordDataAccess.SaveReminderRecordToVehicle(existingReminder);
                        //set urgency to not urgent so it gets excluded in count.
                        reminderRecord.Urgency = ReminderUrgency.NotUrgent;
                    }
                }
            }
            //check for very urgent or past due reminders that were not eligible for recurring.
            var pastDueAndUrgentReminders = result.Where(x => x.Urgency == ReminderUrgency.VeryUrgent || x.Urgency == ReminderUrgency.PastDue);
            if (pastDueAndUrgentReminders.Any())
            {
                return Json(true);
            }
            return Json(false);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetReminderRecordsByVehicleId(int vehicleId)
        {
            var result = GetRemindersAndUrgency(vehicleId, DateTime.Now);
            result = result.OrderByDescending(x => x.Urgency).ToList();
            return PartialView("_ReminderRecords", result);
        }
        [HttpPost]
        public IActionResult PushbackRecurringReminderRecord(int reminderRecordId)
        {
            var existingReminder = _reminderRecordDataAccess.GetReminderRecordById(reminderRecordId);
            existingReminder = _reminderHelper.GetUpdatedRecurringReminderRecord(existingReminder);
            //save to db.
            var result = _reminderRecordDataAccess.SaveReminderRecordToVehicle(existingReminder);
            return Json(result);
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
                Metric = result.Metric,
                IsRecurring = result.IsRecurring,
                ReminderMileageInterval = result.ReminderMileageInterval,
                ReminderMonthInterval = result.ReminderMonthInterval
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
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetUpgradeRecordsByVehicleId(int vehicleId)
        {
            var result = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
            bool _useDescending = _config.GetUserConfig(User).UseDescending;
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
            if (upgradeRecord.Id == default && _config.GetUserConfig(User).EnableAutoOdometerInsert)
            {
                _odometerRecordDataAccess.SaveOdometerRecordToVehicle(new OdometerRecord
                {
                    Date = DateTime.Parse(upgradeRecord.Date),
                    VehicleId = upgradeRecord.VehicleId,
                    Mileage = upgradeRecord.Mileage,
                    Notes = $"Auto Insert From Upgrade Record: {upgradeRecord.Description}"
                });
            }
            //move files from temp.
            upgradeRecord.Files = upgradeRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var result = _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(upgradeRecord.ToUpgradeRecord());
            if (result && upgradeRecord.Supplies.Any())
            {
                RequisitionSupplyRecordsByUsage(upgradeRecord.Supplies);
            }
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
        #region "Notes"
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetNotesByVehicleId(int vehicleId)
        {
            var result = _noteDataAccess.GetNotesByVehicleId(vehicleId);
            result = result.OrderByDescending(x => x.Pinned).ToList();
            return PartialView("_Notes", result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetPinnedNotesByVehicleId(int vehicleId)
        {
            var result = _noteDataAccess.GetNotesByVehicleId(vehicleId);
            result = result.Where(x=>x.Pinned).ToList();
            return Json(result);
        }
        [HttpPost]
        public IActionResult SaveNoteToVehicleId(Note note)
        {
            var result = _noteDataAccess.SaveNoteToVehicle(note);
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddNotePartialView()
        {
            return PartialView("_NoteModal", new Note());
        }
        [HttpGet]
        public IActionResult GetNoteForEditById(int noteId)
        {
            var result = _noteDataAccess.GetNoteById(noteId);
            return PartialView("_NoteModal", result);
        }
        [HttpPost]
        public IActionResult DeleteNoteById(int noteId)
        {
            var result = _noteDataAccess.DeleteNoteById(noteId);
            return Json(result);
        }
        #endregion
        #region "Supply Records"
        private void RequisitionSupplyRecordsByUsage(List<SupplyUsage> supplyUsage)
        {
            foreach(SupplyUsage supply in supplyUsage)
            {
                //get supply record.
                var result = _supplyRecordDataAccess.GetSupplyRecordById(supply.SupplyId);
                var unitCost = (result.Quantity != 0 ) ? result.Cost / result.Quantity : 0;
                //deduct quantity used.
                result.Quantity -= supply.Quantity;
                //deduct cost.
                result.Cost -= (supply.Quantity * unitCost);
                //save
                _supplyRecordDataAccess.SaveSupplyRecordToVehicle(result);
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetSupplyRecordsByVehicleId(int vehicleId)
        {
            var result = _supplyRecordDataAccess.GetSupplyRecordsByVehicleId(vehicleId);
            bool _useDescending = _config.GetUserConfig(User).UseDescending;
            if (_useDescending)
            {
                result = result.OrderByDescending(x => x.Date).ToList();
            }
            else
            {
                result = result.OrderBy(x => x.Date).ToList();
            }
            return PartialView("_SupplyRecords", result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetSupplyRecordsForRecordsByVehicleId(int vehicleId)
        {
            var result = _supplyRecordDataAccess.GetSupplyRecordsByVehicleId(vehicleId);
            result.RemoveAll(x => x.Quantity <= 0);
            bool _useDescending = _config.GetUserConfig(User).UseDescending;
            if (_useDescending)
            {
                result = result.OrderByDescending(x => x.Date).ToList();
            }
            else
            {
                result = result.OrderBy(x => x.Date).ToList();
            }
            return PartialView("_SupplyUsage", result);
        }
        [HttpPost]
        public IActionResult SaveSupplyRecordToVehicleId(SupplyRecordInput supplyRecord)
        {
            //move files from temp.
            supplyRecord.Files = supplyRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var result = _supplyRecordDataAccess.SaveSupplyRecordToVehicle(supplyRecord.ToSupplyRecord());
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddSupplyRecordPartialView()
        {
            return PartialView("_SupplyRecordModal", new SupplyRecordInput());
        }
        [HttpGet]
        public IActionResult GetSupplyRecordForEditById(int supplyRecordId)
        {
            var result = _supplyRecordDataAccess.GetSupplyRecordById(supplyRecordId);
            //convert to Input object.
            var convertedResult = new SupplyRecordInput
            {
                Id = result.Id,
                Cost = result.Cost,
                Date = result.Date.ToShortDateString(),
                Description = result.Description,
                PartNumber = result.PartNumber,
                Quantity = result.Quantity,
                PartSupplier = result.PartSupplier,
                Notes = result.Notes,
                VehicleId = result.VehicleId,
                Files = result.Files
            };
            return PartialView("_SupplyRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeleteSupplyRecordById(int supplyRecordId)
        {
            var result = _supplyRecordDataAccess.DeleteSupplyRecordById(supplyRecordId);
            return Json(result);
        }
        #endregion
        #region "Plan Records"
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetPlanRecordsByVehicleId(int vehicleId)
        {
            var result = _planRecordDataAccess.GetPlanRecordsByVehicleId(vehicleId);
            return PartialView("_PlanRecords", result);
        }
        [HttpPost]
        public IActionResult SavePlanRecordToVehicleId(PlanRecordInput planRecord)
        {
            //populate createdDate
            if (planRecord.Id == default)
            {
                planRecord.DateCreated = DateTime.Now.ToString("G");
            }
            planRecord.DateModified = DateTime.Now.ToString("G");
            //move files from temp.
            planRecord.Files = planRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var result = _planRecordDataAccess.SavePlanRecordToVehicle(planRecord.ToPlanRecord());
            if (result && planRecord.Supplies.Any())
            {
                RequisitionSupplyRecordsByUsage(planRecord.Supplies);
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddPlanRecordPartialView()
        {
            return PartialView("_PlanRecordModal", new PlanRecordInput());
        }
        [HttpPost]
        public IActionResult UpdatePlanRecordProgress(int planRecordId, PlanProgress planProgress, int odometer = 0)
        {
            var existingRecord = _planRecordDataAccess.GetPlanRecordById(planRecordId);
            existingRecord.Progress = planProgress;
            existingRecord.DateModified = DateTime.Now;
            var result = _planRecordDataAccess.SavePlanRecordToVehicle(existingRecord);
            if (planProgress == PlanProgress.Done)
            {
                if (_config.GetUserConfig(User).EnableAutoOdometerInsert)
                {
                    _odometerRecordDataAccess.SaveOdometerRecordToVehicle(new OdometerRecord
                    {
                        Date = DateTime.Now,
                        VehicleId = existingRecord.VehicleId,
                        Mileage = odometer,
                        Notes = $"Auto Insert From Plan Record: {existingRecord.Description}"
                    });
                }
                //convert plan record to service/upgrade/repair record.
                if (existingRecord.ImportMode == ImportMode.ServiceRecord)
                {
                    var newRecord = new ServiceRecord()
                    {
                        VehicleId = existingRecord.VehicleId,
                        Date = DateTime.Now,
                        Mileage = odometer,
                        Description = existingRecord.Description,
                        Cost = existingRecord.Cost,
                        Notes = existingRecord.Notes,
                        Files = existingRecord.Files
                    };
                    _serviceRecordDataAccess.SaveServiceRecordToVehicle(newRecord);
                }
                else if (existingRecord.ImportMode == ImportMode.RepairRecord)
                {
                    var newRecord = new CollisionRecord()
                    {
                        VehicleId = existingRecord.VehicleId,
                        Date = DateTime.Now,
                        Mileage = odometer,
                        Description = existingRecord.Description,
                        Cost = existingRecord.Cost,
                        Notes = existingRecord.Notes,
                        Files = existingRecord.Files
                    };
                    _collisionRecordDataAccess.SaveCollisionRecordToVehicle(newRecord);
                }
                else if (existingRecord.ImportMode == ImportMode.UpgradeRecord)
                {
                    var newRecord = new UpgradeRecord()
                    {
                        VehicleId = existingRecord.VehicleId,
                        Date = DateTime.Now,
                        Mileage = odometer,
                        Description = existingRecord.Description,
                        Cost = existingRecord.Cost,
                        Notes = existingRecord.Notes,
                        Files = existingRecord.Files
                    };
                    _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(newRecord);
                }
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetPlanRecordForEditById(int planRecordId)
        {
            var result = _planRecordDataAccess.GetPlanRecordById(planRecordId);
            //convert to Input object.
            var convertedResult = new PlanRecordInput
            {
                Id = result.Id,
                Description = result.Description,
                DateCreated = result.DateCreated.ToString("G"),
                DateModified = result.DateModified.ToString("G"),
                ImportMode = result.ImportMode,
                Priority = result.Priority,
                Progress = result.Progress,
                Cost = result.Cost,
                Notes = result.Notes,
                VehicleId = result.VehicleId,
                Files = result.Files
            };
            return PartialView("_PlanRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeletePlanRecordById(int planRecordId)
        {
            var result = _planRecordDataAccess.DeletePlanRecordById(planRecordId);
            return Json(result);
        }
        #endregion
        #region "Odometer Records"
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetOdometerRecordsByVehicleId(int vehicleId)
        {
            var result = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
            bool _useDescending = _config.GetUserConfig(User).UseDescending;
            if (_useDescending)
            {
                result = result.OrderByDescending(x => x.Date).ThenByDescending(x => x.Mileage).ToList();
            }
            else
            {
                result = result.OrderBy(x => x.Date).ThenBy(x => x.Mileage).ToList();
            }
            return PartialView("_OdometerRecords", result);
        }
        [HttpPost]
        public IActionResult SaveOdometerRecordToVehicleId(OdometerRecordInput odometerRecord)
        {
            //move files from temp.
            odometerRecord.Files = odometerRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var result = _odometerRecordDataAccess.SaveOdometerRecordToVehicle(odometerRecord.ToOdometerRecord());
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddOdometerRecordPartialView()
        {
            return PartialView("_OdometerRecordModal", new OdometerRecordInput());
        }
        [HttpGet]
        public IActionResult GetOdometerRecordForEditById(int odometerRecordId)
        {
            var result = _odometerRecordDataAccess.GetOdometerRecordById(odometerRecordId);
            //convert to Input object.
            var convertedResult = new OdometerRecordInput
            {
                Id = result.Id,
                Date = result.Date.ToShortDateString(),
                Mileage = result.Mileage,
                Notes = result.Notes,
                VehicleId = result.VehicleId,
                Files = result.Files
            };
            return PartialView("_OdometerRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeleteOdometerRecordById(int odometerRecordId)
        {
            var result = _odometerRecordDataAccess.DeleteOdometerRecordById(odometerRecordId);
            return Json(result);
        }
        #endregion
        #region "Shared Methods"
        public IActionResult MoveRecord(int recordId, ImportMode source, ImportMode destination)
        {
            var genericRecord = new GenericRecord();
            bool result = false;
            //get
            switch (source)
            {
                case ImportMode.ServiceRecord:
                    genericRecord = _serviceRecordDataAccess.GetServiceRecordById(recordId);
                    break;
                case ImportMode.RepairRecord:
                    genericRecord = _collisionRecordDataAccess.GetCollisionRecordById(recordId);
                    break;
                case ImportMode.UpgradeRecord:
                    genericRecord = _upgradeRecordDataAccess.GetUpgradeRecordById(recordId);
                    break;
            }
            //save
            switch (destination)
            {
                case ImportMode.ServiceRecord:
                    result = _serviceRecordDataAccess.SaveServiceRecordToVehicle(StaticHelper.GenericToServiceRecord(genericRecord));
                    break;
                case ImportMode.RepairRecord:
                    result = _collisionRecordDataAccess.SaveCollisionRecordToVehicle(StaticHelper.GenericToRepairRecord(genericRecord));
                    break;
                case ImportMode.UpgradeRecord:
                    result = _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(StaticHelper.GenericToUpgradeRecord(genericRecord));
                    break;
            }
            //delete
            if (result)
            {
                switch (source)
                {
                    case ImportMode.ServiceRecord:
                        _serviceRecordDataAccess.DeleteServiceRecordById(recordId);
                        break;
                    case ImportMode.RepairRecord:
                        _collisionRecordDataAccess.DeleteCollisionRecordById(recordId);
                        break;
                    case ImportMode.UpgradeRecord:
                        _upgradeRecordDataAccess.DeleteUpgradeRecordById(recordId);
                        break;
                }
            }
            return Json(result);
        }
        #endregion
    }
}

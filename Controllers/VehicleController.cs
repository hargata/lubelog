using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
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
        private readonly IPlanRecordTemplateDataAccess _planRecordTemplateDataAccess;
        private readonly IOdometerRecordDataAccess _odometerRecordDataAccess;
        private readonly IWebHostEnvironment _webEnv;
        private readonly IConfigHelper _config;
        private readonly IFileHelper _fileHelper;
        private readonly IGasHelper _gasHelper;
        private readonly IReminderHelper _reminderHelper;
        private readonly IReportHelper _reportHelper;
        private readonly IUserLogic _userLogic;
        private readonly IOdometerLogic _odometerLogic;
        private readonly IVehicleLogic _vehicleLogic;
        private readonly IExtraFieldDataAccess _extraFieldDataAccess;

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
            IPlanRecordTemplateDataAccess planRecordTemplateDataAccess,
            IOdometerRecordDataAccess odometerRecordDataAccess,
            IExtraFieldDataAccess extraFieldDataAccess,
            IUserLogic userLogic,
            IOdometerLogic odometerLogic,
            IVehicleLogic vehicleLogic,
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
            _planRecordTemplateDataAccess = planRecordTemplateDataAccess;
            _odometerRecordDataAccess = odometerRecordDataAccess;
            _extraFieldDataAccess = extraFieldDataAccess;
            _userLogic = userLogic;
            _odometerLogic = odometerLogic;
            _vehicleLogic = vehicleLogic;
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
            return PartialView("_VehicleModal", new Vehicle() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.VehicleRecord).ExtraFields });
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetEditVehiclePartialViewById(int vehicleId)
        {
            var data = _dataAccess.GetVehicleById(vehicleId);
            data.ExtraFields = StaticHelper.AddExtraFields(data.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.VehicleRecord).ExtraFields);
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
                    StaticHelper.NotifyAsync(_config.GetWebHookUrl(), vehicleInput.Id, User.Identity.Name, $"Added Vehicle - Description: {vehicleInput.Year} {vehicleInput.Make} {vehicleInput.Model}");
                } else
                {
                    StaticHelper.NotifyAsync(_config.GetWebHookUrl(), vehicleInput.Id, User.Identity.Name, $"Edited Vehicle - Description: {vehicleInput.Year} {vehicleInput.Make} {vehicleInput.Model}");
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
                _planRecordTemplateDataAccess.DeleteAllPlanRecordTemplatesByVehicleId(vehicleId) &&
                _supplyRecordDataAccess.DeleteAllSupplyRecordsByVehicleId(vehicleId) &&
                _odometerRecordDataAccess.DeleteAllOdometerRecordsByVehicleId(vehicleId) &&
                _userLogic.DeleteAllAccessToVehicle(vehicleId) &&
                _dataAccess.DeleteVehicle(vehicleId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), vehicleId, User.Identity.Name, "Deleted Vehicle");
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult DuplicateVehicleCollaborators(int sourceVehicleId, int destVehicleId)
        {
            try
            {
                //retrieve collaborators for both source and destination vehicle id.
                if (_userLogic.UserCanEditVehicle(GetUserID(), sourceVehicleId) && _userLogic.UserCanEditVehicle(GetUserID(), destVehicleId))
                {
                    var sourceCollaborators = _userLogic.GetCollaboratorsForVehicle(sourceVehicleId).Select(x => x.UserVehicle.UserId).ToList();
                    var destCollaborators = _userLogic.GetCollaboratorsForVehicle(destVehicleId).Select(x => x.UserVehicle.UserId).ToList();
                    sourceCollaborators.RemoveAll(x => destCollaborators.Contains(x));
                    if (sourceCollaborators.Any())
                    {
                        foreach (int collaboratorId in sourceCollaborators)
                        {
                            _userLogic.AddUserAccessToVehicle(collaboratorId, destVehicleId);
                        }
                    }
                    else
                    {
                        return Json(new OperationResponse { Success = false, Message = "Both vehicles already have identical collaborators" });
                    }
                }
                return Json(new OperationResponse { Success = true, Message = "Collaborators Copied" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new OperationResponse { Success = false, Message = StaticHelper.GenericErrorMessage });
            }
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
            if (vehicleId == default && mode != ImportMode.SupplyRecord)
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
                    var exportData = vehicleRecords.Select(x => new ServiceRecordExportModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString("C"), Notes = x.Notes, Odometer = x.Mileage.ToString(), Tags = string.Join(" ", x.Tags) });
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
                    var exportData = vehicleRecords.Select(x => new ServiceRecordExportModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString("C"), Notes = x.Notes, Odometer = x.Mileage.ToString(), Tags = string.Join(" ", x.Tags) });
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
                    var exportData = vehicleRecords.Select(x => new ServiceRecordExportModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString("C"), Notes = x.Notes, Odometer = x.Mileage.ToString(), Tags = string.Join(" ", x.Tags) });
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
                    var exportData = vehicleRecords.Select(x => new OdometerRecordExportModel { Date = x.Date.ToShortDateString(), Notes = x.Notes, InitialOdometer = x.InitialMileage.ToString(), Odometer = x.Mileage.ToString(), Tags = string.Join(" ", x.Tags) });
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
                        Notes = x.Notes,
                        Tags = string.Join(" ", x.Tags)
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
                    var exportData = vehicleRecords.Select(x => new TaxRecordExportModel { Date = x.Date.ToShortDateString(), Description = x.Description, Cost = x.Cost.ToString("C"), Notes = x.Notes, Tags = string.Join(" ", x.Tags) });
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
                    Notes = x.Notes,
                    Tags = string.Join(" ", x.Tags)
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
            if (vehicleId == default && mode != ImportMode.SupplyRecord)
            {
                return Json(false);
            }
            if (string.IsNullOrWhiteSpace(fileName))
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
                                        Notes = string.IsNullOrWhiteSpace(importModel.Notes) ? "" : importModel.Notes,
                                        Tags = string.IsNullOrWhiteSpace(importModel.Tags) ? [] : importModel.Tags.Split(" ").ToList()
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
                                        if (_config.GetUserConfig(User).EnableAutoOdometerInsert)
                                        {
                                            _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                                            {
                                                Date = convertedRecord.Date,
                                                VehicleId = convertedRecord.VehicleId,
                                                Mileage = convertedRecord.Mileage,
                                                Notes = $"Auto Insert From Gas Record via CSV Import. {convertedRecord.Notes}"
                                            });
                                        }
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
                                        Cost = decimal.Parse(importModel.Cost, NumberStyles.Any),
                                        Tags = string.IsNullOrWhiteSpace(importModel.Tags) ? [] : importModel.Tags.Split(" ").ToList()
                                    };
                                    _serviceRecordDataAccess.SaveServiceRecordToVehicle(convertedRecord);
                                    if (_config.GetUserConfig(User).EnableAutoOdometerInsert)
                                    {
                                        _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                                        {
                                            Date = convertedRecord.Date,
                                            VehicleId = convertedRecord.VehicleId,
                                            Mileage = convertedRecord.Mileage,
                                            Notes = $"Auto Insert From Service Record via CSV Import. {convertedRecord.Notes}"
                                        });
                                    }
                                }
                                else if (mode == ImportMode.OdometerRecord)
                                {
                                    var convertedRecord = new OdometerRecord()
                                    {
                                        VehicleId = vehicleId,
                                        Date = DateTime.Parse(importModel.Date),
                                        InitialMileage = string.IsNullOrWhiteSpace(importModel.InitialOdometer) ? 0 : decimal.ToInt32(decimal.Parse(importModel.InitialOdometer, NumberStyles.Any)),
                                        Mileage = decimal.ToInt32(decimal.Parse(importModel.Odometer, NumberStyles.Any)),
                                        Notes = string.IsNullOrWhiteSpace(importModel.Notes) ? "" : importModel.Notes,
                                        Tags = string.IsNullOrWhiteSpace(importModel.Tags) ? [] : importModel.Tags.Split(" ").ToList()
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
                                        Cost = decimal.Parse(importModel.Cost, NumberStyles.Any),
                                        Tags = string.IsNullOrWhiteSpace(importModel.Tags) ? [] : importModel.Tags.Split(" ").ToList()
                                    };
                                    _collisionRecordDataAccess.SaveCollisionRecordToVehicle(convertedRecord);
                                    if (_config.GetUserConfig(User).EnableAutoOdometerInsert)
                                    {
                                        _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                                        {
                                            Date = convertedRecord.Date,
                                            VehicleId = convertedRecord.VehicleId,
                                            Mileage = convertedRecord.Mileage,
                                            Notes = $"Auto Insert From Repair Record via CSV Import. {convertedRecord.Notes}"
                                        });
                                    }
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
                                        Cost = decimal.Parse(importModel.Cost, NumberStyles.Any),
                                        Tags = string.IsNullOrWhiteSpace(importModel.Tags) ? [] : importModel.Tags.Split(" ").ToList()
                                    };
                                    _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(convertedRecord);
                                    if (_config.GetUserConfig(User).EnableAutoOdometerInsert)
                                    {
                                        _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                                        {
                                            Date = convertedRecord.Date,
                                            VehicleId = convertedRecord.VehicleId,
                                            Mileage = convertedRecord.Mileage,
                                            Notes = $"Auto Insert From Upgrade Record via CSV Import. {convertedRecord.Notes}"
                                        });
                                    }
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
                                        Notes = importModel.Notes,
                                        Tags = string.IsNullOrWhiteSpace(importModel.Tags) ? [] : importModel.Tags.Split(" ").ToList()
                                    };
                                    _supplyRecordDataAccess.SaveSupplyRecordToVehicle(convertedRecord);
                                }
                                else if (mode == ImportMode.TaxRecord)
                                {
                                    var convertedRecord = new TaxRecord()
                                    {
                                        VehicleId = vehicleId,
                                        Date = DateTime.Parse(importModel.Date),
                                        Description = string.IsNullOrWhiteSpace(importModel.Description) ? $"Tax Record on {importModel.Date}" : importModel.Description,
                                        Notes = string.IsNullOrWhiteSpace(importModel.Notes) ? "" : importModel.Notes,
                                        Cost = decimal.Parse(importModel.Cost, NumberStyles.Any),
                                        Tags = string.IsNullOrWhiteSpace(importModel.Tags) ? [] : importModel.Tags.Split(" ").ToList()
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
                _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                {
                    Date = DateTime.Parse(gasRecord.Date),
                    VehicleId = gasRecord.VehicleId,
                    Mileage = gasRecord.Mileage,
                    Notes = $"Auto Insert From Gas Record. {gasRecord.Notes}"
                });
            }
            gasRecord.Files = gasRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var result = _gasRecordDataAccess.SaveGasRecordToVehicle(gasRecord.ToGasRecord());
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), gasRecord.VehicleId, User.Identity.Name, $"{(gasRecord.Id == default ? "Created" : "Edited")} Gas Record - Mileage: {gasRecord.Mileage.ToString()}");
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddGasRecordPartialView(int vehicleId)
        {
            var vehicleData = _dataAccess.GetVehicleById(vehicleId);
            var vehicleIsElectric = vehicleData.IsElectric;
            var vehicleUseHours = vehicleData.UseHours;
            return PartialView("_GasModal", new GasRecordInputContainer() { UseKwh = vehicleIsElectric, UseHours = vehicleUseHours, GasRecord = new GasRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.GasRecord).ExtraFields } });
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
                Notes = result.Notes,
                Tags = result.Tags,
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.GasRecord).ExtraFields)
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
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Gas Record - Id: {gasRecordId}");
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult SaveUserGasTabPreferences(string gasUnit, string fuelMileageUnit)
        {
            var currentConfig = _config.GetUserConfig(User);
            currentConfig.PreferredGasUnit = gasUnit;
            currentConfig.PreferredGasMileageUnit = fuelMileageUnit;
            var result = _config.SaveUserConfig(User, currentConfig);
            return Json(result);
        }
        [HttpPost]
        public IActionResult GetGasRecordsEditModal(List<int> recordIds)
        {
            return PartialView("_GasRecordsModal", new GasRecordEditModel { RecordIds = recordIds });
        }
        [HttpPost]
        public IActionResult SaveMultipleGasRecords(GasRecordEditModel editModel)
        {
            var dateIsEdited = editModel.EditRecord.Date != default;
            var mileageIsEdited = editModel.EditRecord.Mileage != default;
            var consumptionIsEdited = editModel.EditRecord.Gallons != default;
            var costIsEdited = editModel.EditRecord.Cost != default;
            var noteIsEdited = !string.IsNullOrWhiteSpace(editModel.EditRecord.Notes);
            var tagsIsEdited = editModel.EditRecord.Tags.Any();
            //handle clear overrides
            if (tagsIsEdited && editModel.EditRecord.Tags.Contains("---"))
            {
                editModel.EditRecord.Tags = new List<string>();
            }
            if (noteIsEdited && editModel.EditRecord.Notes == "---")
            {
                editModel.EditRecord.Notes = "";
            }
            bool result = false;
            foreach (int recordId in editModel.RecordIds)
            {
                var existingRecord = _gasRecordDataAccess.GetGasRecordById(recordId);
                if (dateIsEdited)
                {
                    existingRecord.Date = editModel.EditRecord.Date;
                }
                if (consumptionIsEdited)
                {
                    existingRecord.Gallons = editModel.EditRecord.Gallons;
                }
                if (costIsEdited)
                {
                    existingRecord.Cost = editModel.EditRecord.Cost;
                }
                if (mileageIsEdited)
                {
                    existingRecord.Mileage = editModel.EditRecord.Mileage;
                }
                if (noteIsEdited)
                {
                    existingRecord.Notes = editModel.EditRecord.Notes;
                }
                if (tagsIsEdited)
                {
                    existingRecord.Tags = editModel.EditRecord.Tags;
                }
                result = _gasRecordDataAccess.SaveGasRecordToVehicle(existingRecord);
            }
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
                _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                {
                    Date = DateTime.Parse(serviceRecord.Date),
                    VehicleId = serviceRecord.VehicleId,
                    Mileage = serviceRecord.Mileage,
                    Notes = $"Auto Insert From Service Record: {serviceRecord.Description}"
                });
            }
            //move files from temp.
            serviceRecord.Files = serviceRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            if (serviceRecord.Supplies.Any())
            {
                serviceRecord.RequisitionHistory = RequisitionSupplyRecordsByUsage(serviceRecord.Supplies, DateTime.Parse(serviceRecord.Date), serviceRecord.Description);
                if (serviceRecord.CopySuppliesAttachment)
                {
                    serviceRecord.Files.AddRange(GetSuppliesAttachments(serviceRecord.Supplies));
                }
            }
            //push back any reminders
            if (serviceRecord.ReminderRecordId.Any())
            {
                foreach(int reminderRecordId in serviceRecord.ReminderRecordId)
                {
                    PushbackRecurringReminderRecordWithChecks(reminderRecordId, DateTime.Parse(serviceRecord.Date), serviceRecord.Mileage);
                }
            }
            var result = _serviceRecordDataAccess.SaveServiceRecordToVehicle(serviceRecord.ToServiceRecord());
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), serviceRecord.VehicleId, User.Identity.Name, $"{(serviceRecord.Id == default ? "Created" : "Edited")} Service Record - Description: {serviceRecord.Description}");
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddServiceRecordPartialView()
        {
            return PartialView("_ServiceRecordModal", new ServiceRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.ServiceRecord).ExtraFields });
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
                Files = result.Files,
                Tags = result.Tags,
                RequisitionHistory = result.RequisitionHistory,
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.ServiceRecord).ExtraFields)
            };
            return PartialView("_ServiceRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeleteServiceRecordById(int serviceRecordId)
        {
            var result = _serviceRecordDataAccess.DeleteServiceRecordById(serviceRecordId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Service Record - Id: {serviceRecordId}");
            }
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
                _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                {
                    Date = DateTime.Parse(collisionRecord.Date),
                    VehicleId = collisionRecord.VehicleId,
                    Mileage = collisionRecord.Mileage,
                    Notes = $"Auto Insert From Repair Record: {collisionRecord.Description}"
                });
            }
            //move files from temp.
            collisionRecord.Files = collisionRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            if (collisionRecord.Supplies.Any())
            {
                collisionRecord.RequisitionHistory = RequisitionSupplyRecordsByUsage(collisionRecord.Supplies, DateTime.Parse(collisionRecord.Date), collisionRecord.Description);
                if (collisionRecord.CopySuppliesAttachment)
                {
                    collisionRecord.Files.AddRange(GetSuppliesAttachments(collisionRecord.Supplies));
                }
            }
            //push back any reminders
            if (collisionRecord.ReminderRecordId.Any())
            {
                foreach (int reminderRecordId in collisionRecord.ReminderRecordId)
                {
                    PushbackRecurringReminderRecordWithChecks(reminderRecordId, DateTime.Parse(collisionRecord.Date), collisionRecord.Mileage);
                }
            }
            var result = _collisionRecordDataAccess.SaveCollisionRecordToVehicle(collisionRecord.ToCollisionRecord());
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), collisionRecord.VehicleId, User.Identity.Name, $"{(collisionRecord.Id == default ? "Created" : "Edited")} Repair Record - Description: {collisionRecord.Description}");
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddCollisionRecordPartialView()
        {
            return PartialView("_CollisionRecordModal", new CollisionRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.RepairRecord).ExtraFields });
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
                Files = result.Files,
                Tags = result.Tags,
                RequisitionHistory = result.RequisitionHistory,
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.RepairRecord).ExtraFields)
            };
            return PartialView("_CollisionRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeleteCollisionRecordById(int collisionRecordId)
        {
            var result = _collisionRecordDataAccess.DeleteCollisionRecordById(collisionRecordId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Repair Record - Id: {collisionRecordId}");
            }
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
                foreach (TaxRecord recurringFee in recurringFees)
                {
                    var newDate = new DateTime();
                    if (recurringFee.RecurringInterval != ReminderMonthInterval.Other)
                    {
                        newDate = recurringFee.Date.AddMonths((int)recurringFee.RecurringInterval);
                    }
                    else
                    {
                        newDate = recurringFee.Date.AddMonths(recurringFee.CustomMonthInterval);
                    }
                    if (DateTime.Now > newDate)
                    {
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
                            CustomMonthInterval = recurringFee.CustomMonthInterval,
                            Files = recurringFee.Files,
                            Tags = recurringFee.Tags,
                            ExtraFields = recurringFee.ExtraFields
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
            //push back any reminders
            if (taxRecord.ReminderRecordId.Any())
            {
                foreach (int reminderRecordId in taxRecord.ReminderRecordId)
                {
                    PushbackRecurringReminderRecordWithChecks(reminderRecordId, DateTime.Parse(taxRecord.Date), null);
                }
            }
            var result = _taxRecordDataAccess.SaveTaxRecordToVehicle(taxRecord.ToTaxRecord());
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), taxRecord.VehicleId, User.Identity.Name, $"{(taxRecord.Id == default ? "Created" : "Edited")} Tax Record - Description: {taxRecord.Description}");
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddTaxRecordPartialView()
        {
            return PartialView("_TaxRecordModal", new TaxRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.TaxRecord).ExtraFields });
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
                CustomMonthInterval = result.CustomMonthInterval,
                Files = result.Files,
                Tags = result.Tags,
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.TaxRecord).ExtraFields)
            };
            return PartialView("_TaxRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeleteTaxRecordById(int taxRecordId)
        {
            var result = _taxRecordDataAccess.DeleteTaxRecordById(taxRecordId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Tax Record - Id: {taxRecordId}");
            }
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
            var odometerRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
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
            allCosts.AddRange(_reportHelper.GetOdometerRecordSum(odometerRecords, 0));
            viewModel.CostForVehicleByMonth = allCosts.GroupBy(x => new { x.MonthName, x.MonthId }).OrderBy(x => x.Key.MonthId).Select(x => new CostForVehicleByMonth
            {
                MonthName = x.Key.MonthName,
                Cost = x.Sum(y => y.Cost),
                DistanceTraveled = x.Max(y => y.DistanceTraveled)
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
                Cost = x.Sum(y => y.Cost)
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
        [HttpPost]
        public IActionResult GetVehicleAttachments(int vehicleId, List<ImportMode> exportTabs)
        {
            List<GenericReportModel> attachmentData = new List<GenericReportModel>();
            if (exportTabs.Contains(ImportMode.ServiceRecord))
            {
                var records = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId).Where(x => x.Files.Any());
                attachmentData.AddRange(records.Select(x => new GenericReportModel
                {
                    DataType = ImportMode.ServiceRecord,
                    Date = x.Date,
                    Odometer = x.Mileage,
                    Files = x.Files
                }));
            }
            if (exportTabs.Contains(ImportMode.RepairRecord))
            {
                var records = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId).Where(x => x.Files.Any());
                attachmentData.AddRange(records.Select(x => new GenericReportModel
                {
                    DataType = ImportMode.RepairRecord,
                    Date = x.Date,
                    Odometer = x.Mileage,
                    Files = x.Files
                }));
            }
            if (exportTabs.Contains(ImportMode.UpgradeRecord))
            {
                var records = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId).Where(x => x.Files.Any());
                attachmentData.AddRange(records.Select(x => new GenericReportModel
                {
                    DataType = ImportMode.UpgradeRecord,
                    Date = x.Date,
                    Odometer = x.Mileage,
                    Files = x.Files
                }));
            }
            if (exportTabs.Contains(ImportMode.GasRecord))
            {
                var records = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId).Where(x => x.Files.Any());
                attachmentData.AddRange(records.Select(x => new GenericReportModel
                {
                    DataType = ImportMode.GasRecord,
                    Date = x.Date,
                    Odometer = x.Mileage,
                    Files = x.Files
                }));
            }
            if (exportTabs.Contains(ImportMode.TaxRecord))
            {
                var records = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId).Where(x => x.Files.Any());
                attachmentData.AddRange(records.Select(x => new GenericReportModel
                {
                    DataType = ImportMode.TaxRecord,
                    Date = x.Date,
                    Odometer = 0,
                    Files = x.Files
                }));
            }
            if (exportTabs.Contains(ImportMode.OdometerRecord))
            {
                var records = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId).Where(x => x.Files.Any());
                attachmentData.AddRange(records.Select(x => new GenericReportModel
                {
                    DataType = ImportMode.OdometerRecord,
                    Date = x.Date,
                    Odometer = x.Mileage,
                    Files = x.Files
                }));
            }
            if (attachmentData.Any())
            {
                attachmentData = attachmentData.OrderBy(x => x.Date).ThenBy(x => x.Odometer).ToList();
                var result = _fileHelper.MakeAttachmentsExport(attachmentData);
                if (string.IsNullOrWhiteSpace(result))
                {
                    return Json(new OperationResponse { Success = false, Message = StaticHelper.GenericErrorMessage });
                }
                return Json(new OperationResponse { Success = true, Message = result });
            }
            else
            {
                return Json(new OperationResponse { Success = false, Message = "No Attachments Found" });
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        public IActionResult GetVehicleHistory(int vehicleId)
        {
            var vehicleHistory = new VehicleHistoryViewModel();
            vehicleHistory.VehicleData = _dataAccess.GetVehicleById(vehicleId);
            var maxMileage = _vehicleLogic.GetMaxMileage(vehicleId);
            vehicleHistory.Odometer = maxMileage.ToString("N0");
            var minMileage = _vehicleLogic.GetMinMileage(vehicleId);
            var distanceTraveled = maxMileage - minMileage;
            if (!string.IsNullOrWhiteSpace(vehicleHistory.VehicleData.PurchaseDate))
            {
                var endDate = vehicleHistory.VehicleData.SoldDate;
                int daysOwned = 0;
                if (string.IsNullOrWhiteSpace(endDate))
                {
                    endDate = DateTime.Now.ToShortDateString();
                }
                try
                {
                    daysOwned = (DateTime.Parse(endDate) - DateTime.Parse(vehicleHistory.VehicleData.PurchaseDate)).Days;
                    vehicleHistory.DaysOwned = daysOwned.ToString("N0");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    vehicleHistory.DaysOwned = string.Empty;
                }
                //calculate depreciation
                var totalDepreciation = vehicleHistory.VehicleData.PurchasePrice - vehicleHistory.VehicleData.SoldPrice;
                //we only calculate depreciation if a sold price is provided.
                if (totalDepreciation != default && vehicleHistory.VehicleData.SoldPrice != default)
                {
                    vehicleHistory.TotalDepreciation = totalDepreciation;
                    if (daysOwned != default)
                    {
                        vehicleHistory.DepreciationPerDay = Math.Abs(totalDepreciation / daysOwned);
                    }
                    if (distanceTraveled != default)
                    {
                        vehicleHistory.DepreciationPerMile = Math.Abs(totalDepreciation / distanceTraveled);
                    }
                }
            }
            List<GenericReportModel> reportData = new List<GenericReportModel>();
            var serviceRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
            var repairRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
            var upgradeRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
            var taxRecords = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
            var gasRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            bool useMPG = _config.GetUserConfig(User).UseMPG;
            bool useUKMPG = _config.GetUserConfig(User).UseUKMPG;
            string preferredFuelMileageUnit = _config.GetUserConfig(User).PreferredGasMileageUnit;
            vehicleHistory.DistanceUnit = vehicleHistory.VehicleData.UseHours ? "h" : useMPG ? "mi." : "km";
            vehicleHistory.TotalGasCost = gasRecords.Sum(x => x.Cost);
            vehicleHistory.TotalCost = serviceRecords.Sum(x => x.Cost) + repairRecords.Sum(x => x.Cost) + upgradeRecords.Sum(x => x.Cost) + taxRecords.Sum(x => x.Cost);
            if (distanceTraveled != default)
            {
                vehicleHistory.DistanceTraveled = distanceTraveled.ToString("N0");
                vehicleHistory.TotalCostPerMile = vehicleHistory.TotalCost / distanceTraveled;
                vehicleHistory.TotalGasCostPerMile = vehicleHistory.TotalGasCost / distanceTraveled;
            }
            var averageMPG = "0";
            var gasViewModels = _gasHelper.GetGasRecordViewModels(gasRecords, useMPG, useUKMPG);
            if (gasViewModels.Any())
            {
                averageMPG = _gasHelper.GetAverageGasMileage(gasViewModels, useMPG);
            }
            var fuelEconomyMileageUnit = StaticHelper.GetFuelEconomyUnit(vehicleHistory.VehicleData.IsElectric, vehicleHistory.VehicleData.UseHours, useMPG, useUKMPG);
            if (fuelEconomyMileageUnit == "l/100km" && preferredFuelMileageUnit == "km/l")
            {
                //conversion needed.
                var newAverageMPG = decimal.Parse(averageMPG, NumberStyles.Any);
                if (newAverageMPG != 0)
                {
                    newAverageMPG = 100 / newAverageMPG;
                }
                averageMPG = newAverageMPG.ToString("F");
                fuelEconomyMileageUnit = preferredFuelMileageUnit;
            }
            vehicleHistory.MPG = $"{averageMPG} {fuelEconomyMileageUnit}";
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
            if (selectedMetrics.Contains(ImportMode.OdometerRecord))
            {
                var odometerRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
                allCosts.AddRange(_reportHelper.GetOdometerRecordSum(odometerRecords, year));
            }
            var groupedRecord = allCosts.GroupBy(x => new { x.MonthName, x.MonthId }).OrderBy(x => x.Key.MonthId).Select(x => new CostForVehicleByMonth
            {
                MonthName = x.Key.MonthName,
                Cost = x.Sum(y => y.Cost),
                DistanceTraveled = x.Max(y => y.DistanceTraveled)
            }).ToList();
            return PartialView("_GasCostByMonthReport", groupedRecord);
        }
        #endregion
        #region "Reminders"
        private List<ReminderRecordViewModel> GetRemindersAndUrgency(int vehicleId, DateTime dateCompare)
        {
            var currentMileage = _vehicleLogic.GetMaxMileage(vehicleId);
            var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
            List<ReminderRecordViewModel> results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, dateCompare);
            return results;
        }
        private bool GetAndUpdateVehicleUrgentOrPastDueReminders(int vehicleId)
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
                        existingReminder = _reminderHelper.GetUpdatedRecurringReminderRecord(existingReminder, null, null);
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
                return true;
            }
            return false;
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetVehicleHaveUrgentOrPastDueReminders(int vehicleId)
        {
            var result = GetAndUpdateVehicleUrgentOrPastDueReminders(vehicleId);
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetReminderRecordsByVehicleId(int vehicleId)
        {
            var result = GetRemindersAndUrgency(vehicleId, DateTime.Now);
            result = result.OrderByDescending(x => x.Urgency).ToList();
            return PartialView("_ReminderRecords", result);
        }
        [HttpGet]
        public IActionResult GetRecurringReminderRecordsByVehicleId(int vehicleId)
        {
            var result = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
            result.RemoveAll(x => !x.IsRecurring);
            return PartialView("_RecurringReminderSelector", result);
        }
        [HttpPost]
        public IActionResult PushbackRecurringReminderRecord(int reminderRecordId)
        {
            var result = PushbackRecurringReminderRecordWithChecks(reminderRecordId, null, null);
            return Json(result);
        }
        private bool PushbackRecurringReminderRecordWithChecks(int reminderRecordId, DateTime? currentDate, int? currentMileage)
        {
            try
            {
                var existingReminder = _reminderRecordDataAccess.GetReminderRecordById(reminderRecordId);
                if (existingReminder is not null && existingReminder.Id != default && existingReminder.IsRecurring)
                {
                    existingReminder = _reminderHelper.GetUpdatedRecurringReminderRecord(existingReminder, currentDate, currentMileage);
                    //save to db.
                    var reminderUpdateResult = _reminderRecordDataAccess.SaveReminderRecordToVehicle(existingReminder);
                    if (!reminderUpdateResult)
                    {
                        _logger.LogError("Unable to update reminder either because the reminder no longer exists or is no longer recurring");
                        return false;
                    }
                    return true;
                }
                else
                {
                    _logger.LogError("Unable to update reminder because it no longer exists.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }
        [HttpPost]
        public IActionResult SaveReminderRecordToVehicleId(ReminderRecordInput reminderRecord)
        {
            var result = _reminderRecordDataAccess.SaveReminderRecordToVehicle(reminderRecord.ToReminderRecord());
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), reminderRecord.VehicleId, User.Identity.Name, $"{(reminderRecord.Id == default ? "Created" : "Edited")} Reminder - Description: {reminderRecord.Description}");
            }
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
                ReminderMonthInterval = result.ReminderMonthInterval,
                CustomMileageInterval = result.CustomMileageInterval,
                CustomMonthInterval = result.CustomMonthInterval,
                Tags = result.Tags
            };
            return PartialView("_ReminderRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeleteReminderRecordById(int reminderRecordId)
        {
            var result = _reminderRecordDataAccess.DeleteReminderRecordById(reminderRecordId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Reminder - Id: {reminderRecordId}");
            }
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
                _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                {
                    Date = DateTime.Parse(upgradeRecord.Date),
                    VehicleId = upgradeRecord.VehicleId,
                    Mileage = upgradeRecord.Mileage,
                    Notes = $"Auto Insert From Upgrade Record: {upgradeRecord.Description}"
                });
            }
            //move files from temp.
            upgradeRecord.Files = upgradeRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            if (upgradeRecord.Supplies.Any())
            {
                upgradeRecord.RequisitionHistory = RequisitionSupplyRecordsByUsage(upgradeRecord.Supplies, DateTime.Parse(upgradeRecord.Date), upgradeRecord.Description);
                if (upgradeRecord.CopySuppliesAttachment)
                {
                    upgradeRecord.Files.AddRange(GetSuppliesAttachments(upgradeRecord.Supplies));
                }
            }
            //push back any reminders
            if (upgradeRecord.ReminderRecordId.Any())
            {
                foreach (int reminderRecordId in upgradeRecord.ReminderRecordId)
                {
                    PushbackRecurringReminderRecordWithChecks(reminderRecordId, DateTime.Parse(upgradeRecord.Date), upgradeRecord.Mileage);
                }
            }
            var result = _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(upgradeRecord.ToUpgradeRecord());
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), upgradeRecord.VehicleId, User.Identity.Name, $"{(upgradeRecord.Id == default ? "Created" : "Edited")} Upgrade Record - Description: {upgradeRecord.Description}");
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddUpgradeRecordPartialView()
        {
            return PartialView("_UpgradeRecordModal", new UpgradeRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.UpgradeRecord).ExtraFields });
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
                Files = result.Files,
                Tags = result.Tags,
                RequisitionHistory = result.RequisitionHistory,
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.UpgradeRecord).ExtraFields)
            };
            return PartialView("_UpgradeRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeleteUpgradeRecordById(int upgradeRecordId)
        {
            var result = _upgradeRecordDataAccess.DeleteUpgradeRecordById(upgradeRecordId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Upgrade Record - Id: {upgradeRecordId}");
            }
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
            result = result.Where(x => x.Pinned).ToList();
            return Json(result);
        }
        [HttpPost]
        public IActionResult SaveNoteToVehicleId(Note note)
        {
            var result = _noteDataAccess.SaveNoteToVehicle(note);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), note.VehicleId, User.Identity.Name, $"{(note.Id == default ? "Created" : "Edited")} Note - Description: {note.Description}");
            }
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
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Note - Id: {noteId}");
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult PinNotes(List<int> noteIds, bool isToggle = false, bool pinStatus = false)
        {
            var result = false;
            foreach (int noteId in noteIds)
            {
                var existingNote = _noteDataAccess.GetNoteById(noteId);
                if (isToggle)
                {
                    existingNote.Pinned = !existingNote.Pinned;
                }
                else
                {
                    existingNote.Pinned = pinStatus;
                }
                result = _noteDataAccess.SaveNoteToVehicle(existingNote);
            }
            return Json(result);
        }
        #endregion
        #region "Supply Records"
        private List<string> CheckSupplyRecordsAvailability(List<SupplyUsage> supplyUsage)
        {
            //returns empty string if all supplies are available
            var result = new List<string>();
            foreach (SupplyUsage supply in supplyUsage)
            {
                //get supply record.
                var supplyData = _supplyRecordDataAccess.GetSupplyRecordById(supply.SupplyId);
                if (supplyData == null)
                {
                    result.Add("Missing Supplies, Please Delete This Template and Recreate It.");
                }
                else if (supply.Quantity > supplyData.Quantity)
                {
                    result.Add($"Insufficient Quantity for {supplyData.Description}, need: {supply.Quantity}, available: {supplyData.Quantity}");
                }
            }
            return result;
        }
        private List<UploadedFiles> GetSuppliesAttachments(List<SupplyUsage> supplyUsage)
        {
            List<UploadedFiles> results = new List<UploadedFiles>();
            foreach(SupplyUsage supply in supplyUsage)
            {
                var result = _supplyRecordDataAccess.GetSupplyRecordById(supply.SupplyId);
                results.AddRange(result.Files);
            }
            return results;
        }
        private List<SupplyUsageHistory> RequisitionSupplyRecordsByUsage(List<SupplyUsage> supplyUsage, DateTime dateRequisitioned, string usageDescription)
        {
            List<SupplyUsageHistory> results = new List<SupplyUsageHistory>();
            foreach (SupplyUsage supply in supplyUsage)
            {
                //get supply record.
                var result = _supplyRecordDataAccess.GetSupplyRecordById(supply.SupplyId);
                var unitCost = (result.Quantity != 0) ? result.Cost / result.Quantity : 0;
                //deduct quantity used.
                result.Quantity -= supply.Quantity;
                //deduct cost.
                result.Cost -= (supply.Quantity * unitCost);
                //check decimal places to ensure that it always has a max of 3 decimal places.
                var roundedDecimal = decimal.Round(result.Cost, 3);
                if (roundedDecimal != result.Cost)
                {
                    //Too many decimals
                    result.Cost = roundedDecimal;
                }
                //create new requisitionrrecord
                var requisitionRecord = new SupplyUsageHistory
                {
                    Date = dateRequisitioned,
                    Description = usageDescription,
                    Quantity = supply.Quantity,
                    Cost = (supply.Quantity * unitCost)
                };
                result.RequisitionHistory.Add(requisitionRecord);
                //save
                _supplyRecordDataAccess.SaveSupplyRecordToVehicle(result);
                requisitionRecord.Description = result.Description; //change the name of the description for plan/service/repair/upgrade records
                requisitionRecord.PartNumber = result.PartNumber; //populate part number if not displayed in supplies modal.
                results.Add(requisitionRecord);
            }
            return results;
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
            if (_config.GetServerEnableShopSupplies())
            {
                result.AddRange(_supplyRecordDataAccess.GetSupplyRecordsByVehicleId(0)); // add shop supplies
            }
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
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), supplyRecord.VehicleId, User.Identity.Name, $"{(supplyRecord.Id == default ? "Created" : "Edited")} Supply Record - Description: {supplyRecord.Description}");
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddSupplyRecordPartialView()
        {
            return PartialView("_SupplyRecordModal", new SupplyRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.SupplyRecord).ExtraFields });
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
                Files = result.Files,
                Tags = result.Tags,
                RequisitionHistory = result.RequisitionHistory,
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.SupplyRecord).ExtraFields)
            };
            return PartialView("_SupplyRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeleteSupplyRecordById(int supplyRecordId)
        {
            var result = _supplyRecordDataAccess.DeleteSupplyRecordById(supplyRecordId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Supply Record - Id: {supplyRecordId}");
            }
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
            if (planRecord.Supplies.Any())
            {
                planRecord.RequisitionHistory = RequisitionSupplyRecordsByUsage(planRecord.Supplies, DateTime.Parse(planRecord.DateCreated), planRecord.Description);
                if (planRecord.CopySuppliesAttachment)
                {
                    planRecord.Files.AddRange(GetSuppliesAttachments(planRecord.Supplies));
                }
            }
            var result = _planRecordDataAccess.SavePlanRecordToVehicle(planRecord.ToPlanRecord());
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), planRecord.VehicleId, User.Identity.Name, $"{(planRecord.Id == default ? "Created" : "Edited")} Plan Record - Description: {planRecord.Description}");
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult SavePlanRecordTemplateToVehicleId(PlanRecordInput planRecord)
        {
            //check if template name already taken.
            var existingRecord = _planRecordTemplateDataAccess.GetPlanRecordTemplatesByVehicleId(planRecord.VehicleId).Where(x => x.Description == planRecord.Description).Any();
            if (existingRecord)
            {
                return Json(new OperationResponse { Success = false, Message = "A template with that description already exists for this vehicle" });
            }
            planRecord.Files = planRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            if (planRecord.Supplies.Any() && planRecord.CopySuppliesAttachment)
            {
                planRecord.Files.AddRange(GetSuppliesAttachments(planRecord.Supplies));
            }
            var result = _planRecordTemplateDataAccess.SavePlanRecordTemplateToVehicle(planRecord);
            return Json(new OperationResponse { Success = result, Message = result ? "Template Added" : StaticHelper.GenericErrorMessage });
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetPlanRecordTemplatesForVehicleId(int vehicleId)
        {
            var result = _planRecordTemplateDataAccess.GetPlanRecordTemplatesByVehicleId(vehicleId);
            return PartialView("_PlanRecordTemplateModal", result);
        }
        [HttpPost]
        public IActionResult DeletePlanRecordTemplateById(int planRecordTemplateId)
        {
            var result = _planRecordTemplateDataAccess.DeletePlanRecordTemplateById(planRecordTemplateId);
            return Json(result);
        }
        [HttpPost]
        public IActionResult ConvertPlanRecordTemplateToPlanRecord(int planRecordTemplateId)
        {
            var existingRecord = _planRecordTemplateDataAccess.GetPlanRecordTemplateById(planRecordTemplateId);
            if (existingRecord.Id == default)
            {
                return Json(new OperationResponse { Success = false, Message = "Unable to find template" });
            }
            if (existingRecord.Supplies.Any())
            {
                //check if all supplies are available
                var supplyAvailability = CheckSupplyRecordsAvailability(existingRecord.Supplies);
                if (supplyAvailability.Any())
                {
                    return Json(new OperationResponse { Success = false, Message = string.Join("<br>", supplyAvailability) });
                }
            }
            if (existingRecord.ReminderRecordId != default)
            {
                //check if reminder still exists and is still recurring.
                var existingReminder = _reminderRecordDataAccess.GetReminderRecordById(existingRecord.ReminderRecordId);
                if (existingReminder is null || existingReminder.Id == default || !existingReminder.IsRecurring)
                {
                    return Json(new OperationResponse { Success = false, Message = "Missing or Non-recurring Reminder, Please Delete This Template and Recreate It." });
                }
            }
            //populate createdDate
            existingRecord.DateCreated = DateTime.Now.ToString("G");
            existingRecord.DateModified = DateTime.Now.ToString("G");
            existingRecord.Id = default;
            if (existingRecord.Supplies.Any())
            {
                existingRecord.RequisitionHistory = RequisitionSupplyRecordsByUsage(existingRecord.Supplies, DateTime.Parse(existingRecord.DateCreated), existingRecord.Description);
            }
            var result = _planRecordDataAccess.SavePlanRecordToVehicle(existingRecord.ToPlanRecord());
            return Json(new OperationResponse { Success = result, Message = result ? "Plan Record Added" : StaticHelper.GenericErrorMessage });
        }
        [HttpGet]
        public IActionResult GetAddPlanRecordPartialView()
        {
            return PartialView("_PlanRecordModal", new PlanRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.PlanRecord).ExtraFields });
        }
        [HttpPost]
        public IActionResult GetAddPlanRecordPartialView(PlanRecordInput? planModel)
        {
            if (planModel is not null)
            {
                planModel.ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.PlanRecord).ExtraFields;
                return PartialView("_PlanRecordModal", planModel);
            }
            return PartialView("_PlanRecordModal", new PlanRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.PlanRecord).ExtraFields });
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
                    _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                    {
                        Date = DateTime.Now,
                        VehicleId = existingRecord.VehicleId,
                        Mileage = odometer,
                        Notes = $"Auto Insert From Plan Record: {existingRecord.Description}",
                        ExtraFields = existingRecord.ExtraFields
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
                        Files = existingRecord.Files,
                        RequisitionHistory = existingRecord.RequisitionHistory,
                        ExtraFields = existingRecord.ExtraFields
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
                        Files = existingRecord.Files,
                        RequisitionHistory = existingRecord.RequisitionHistory,
                        ExtraFields = existingRecord.ExtraFields
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
                        Files = existingRecord.Files,
                        RequisitionHistory = existingRecord.RequisitionHistory,
                        ExtraFields = existingRecord.ExtraFields
                    };
                    _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(newRecord);
                }
                //push back any reminders
                if (existingRecord.ReminderRecordId != default)
                {
                    PushbackRecurringReminderRecordWithChecks(existingRecord.ReminderRecordId, DateTime.Now, odometer);
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
                Files = result.Files,
                RequisitionHistory = result.RequisitionHistory,
                ReminderRecordId = result.ReminderRecordId,
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.PlanRecord).ExtraFields)
            };
            return PartialView("_PlanRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeletePlanRecordById(int planRecordId)
        {
            var result = _planRecordDataAccess.DeletePlanRecordById(planRecordId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Plan Record - Id: {planRecordId}");
            }
            return Json(result);
        }
        #endregion
        #region "Odometer Records"
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        public IActionResult ForceRecalculateDistanceByVehicleId(int vehicleId)
        {
            var result = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
            result = _odometerLogic.AutoConvertOdometerRecord(result);
            return Json(result.Any());
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetOdometerRecordsByVehicleId(int vehicleId)
        {
            var result = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
            //determine if conversion is needed.
            if (result.All(x => x.InitialMileage == default))
            {
                result = _odometerLogic.AutoConvertOdometerRecord(result);
            }
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
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), odometerRecord.VehicleId, User.Identity.Name, $"{(odometerRecord.Id == default ? "Created" : "Edited")} Odometer Record - Mileage: {odometerRecord.Mileage.ToString()}");
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddOdometerRecordPartialView(int vehicleId)
        {
            return PartialView("_OdometerRecordModal", new OdometerRecordInput() { InitialMileage = _odometerLogic.GetLastOdometerRecordMileage(vehicleId, new List<OdometerRecord>()), ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.OdometerRecord).ExtraFields });
        }
        [HttpPost]
        public IActionResult GetOdometerRecordsEditModal(List<int> recordIds)
        {
            return PartialView("_OdometerRecordsModal", new OdometerRecordEditModel { RecordIds = recordIds });
        }
        [HttpPost]
        public IActionResult SaveMultipleOdometerRecords(OdometerRecordEditModel editModel)
        {
            var dateIsEdited = editModel.EditRecord.Date != default;
            var initialMileageIsEdited = editModel.EditRecord.InitialMileage != default;
            var mileageIsEdited = editModel.EditRecord.Mileage != default;
            var noteIsEdited = !string.IsNullOrWhiteSpace(editModel.EditRecord.Notes);
            var tagsIsEdited = editModel.EditRecord.Tags.Any();
            //handle clear overrides
            if (tagsIsEdited && editModel.EditRecord.Tags.Contains("---"))
            {
                editModel.EditRecord.Tags = new List<string>();
            }
            if (noteIsEdited && editModel.EditRecord.Notes == "---")
            {
                editModel.EditRecord.Notes = "";
            }
            bool result = false;
            foreach (int recordId in editModel.RecordIds)
            {
                var existingRecord = _odometerRecordDataAccess.GetOdometerRecordById(recordId);
                if (dateIsEdited)
                {
                    existingRecord.Date = editModel.EditRecord.Date;
                }
                if (initialMileageIsEdited)
                {
                    existingRecord.InitialMileage = editModel.EditRecord.InitialMileage;
                }
                if (mileageIsEdited)
                {
                    existingRecord.Mileage = editModel.EditRecord.Mileage;
                }
                if (noteIsEdited)
                {
                    existingRecord.Notes = editModel.EditRecord.Notes;
                }
                if (tagsIsEdited)
                {
                    existingRecord.Tags = editModel.EditRecord.Tags;
                }
                result = _odometerRecordDataAccess.SaveOdometerRecordToVehicle(existingRecord);
            }
            return Json(result);
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
                InitialMileage = result.InitialMileage,
                Mileage = result.Mileage,
                Notes = result.Notes,
                VehicleId = result.VehicleId,
                Files = result.Files,
                Tags = result.Tags,
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.OdometerRecord).ExtraFields)
            };
            return PartialView("_OdometerRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeleteOdometerRecordById(int odometerRecordId)
        {
            var result = _odometerRecordDataAccess.DeleteOdometerRecordById(odometerRecordId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Odometer Record - Id: {odometerRecordId}");
            }
            return Json(result);
        }
        #endregion
        #region "Shared Methods"
        [TypeFilter(typeof(CollaboratorFilter))]
        public IActionResult GetMaxMileage(int vehicleId)
        {
            var result = _vehicleLogic.GetMaxMileage(vehicleId);
            return Json(result);
        }
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
        public IActionResult MoveRecords(List<int> recordIds, ImportMode source, ImportMode destination)
        {
            var genericRecord = new GenericRecord();
            bool result = false;
            foreach (int recordId in recordIds)
            {
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
            }
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Moved multiple {source.ToString()} to {destination.ToString()} - Ids: {string.Join(",", recordIds)}");
            }
            return Json(result);
        }
        public IActionResult DeleteRecords(List<int> recordIds, ImportMode importMode)
        {
            bool result = false;
            foreach (int recordId in recordIds)
            {
                switch (importMode)
                {
                    case ImportMode.ServiceRecord:
                        result = _serviceRecordDataAccess.DeleteServiceRecordById(recordId);
                        break;
                    case ImportMode.RepairRecord:
                        result = _collisionRecordDataAccess.DeleteCollisionRecordById(recordId);
                        break;
                    case ImportMode.UpgradeRecord:
                        result = _upgradeRecordDataAccess.DeleteUpgradeRecordById(recordId);
                        break;
                    case ImportMode.GasRecord:
                        result = _gasRecordDataAccess.DeleteGasRecordById(recordId);
                        break;
                    case ImportMode.TaxRecord:
                        result = _taxRecordDataAccess.DeleteTaxRecordById(recordId);
                        break;
                    case ImportMode.SupplyRecord:
                        result = _supplyRecordDataAccess.DeleteSupplyRecordById(recordId);
                        break;
                    case ImportMode.NoteRecord:
                        result = _noteDataAccess.DeleteNoteById(recordId);
                        break;
                    case ImportMode.OdometerRecord:
                        result = _odometerRecordDataAccess.DeleteOdometerRecordById(recordId);
                        break;
                    case ImportMode.ReminderRecord:
                        result = _reminderRecordDataAccess.DeleteReminderRecordById(recordId);
                        break;
                }
            }
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted multiple {importMode.ToString()} - Ids: {string.Join(",", recordIds)}");
            }
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        public IActionResult AdjustRecordsOdometer(List<int> recordIds, int vehicleId, ImportMode importMode)
        {
            bool result = false;
            //get vehicle's odometer adjustments
            var vehicleData = _dataAccess.GetVehicleById(vehicleId);
            foreach (int recordId in recordIds)
            {
                switch (importMode)
                {
                    case ImportMode.ServiceRecord:
                        {
                            var existingRecord = _serviceRecordDataAccess.GetServiceRecordById(recordId);
                            existingRecord.Mileage += int.Parse(vehicleData.OdometerDifference);
                            existingRecord.Mileage = decimal.ToInt32(existingRecord.Mileage * decimal.Parse(vehicleData.OdometerMultiplier, NumberStyles.Any));
                            result = _serviceRecordDataAccess.SaveServiceRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.RepairRecord:
                        {
                            var existingRecord = _collisionRecordDataAccess.GetCollisionRecordById(recordId);
                            existingRecord.Mileage += int.Parse(vehicleData.OdometerDifference);
                            existingRecord.Mileage = decimal.ToInt32(existingRecord.Mileage * decimal.Parse(vehicleData.OdometerMultiplier, NumberStyles.Any));
                            result = _collisionRecordDataAccess.SaveCollisionRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.UpgradeRecord:
                        {
                            var existingRecord = _upgradeRecordDataAccess.GetUpgradeRecordById(recordId);
                            existingRecord.Mileage += int.Parse(vehicleData.OdometerDifference);
                            existingRecord.Mileage = decimal.ToInt32(existingRecord.Mileage * decimal.Parse(vehicleData.OdometerMultiplier, NumberStyles.Any));
                            result = _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.GasRecord:
                        {
                            var existingRecord = _gasRecordDataAccess.GetGasRecordById(recordId);
                            existingRecord.Mileage += int.Parse(vehicleData.OdometerDifference);
                            existingRecord.Mileage = decimal.ToInt32(existingRecord.Mileage * decimal.Parse(vehicleData.OdometerMultiplier, NumberStyles.Any));
                            result = _gasRecordDataAccess.SaveGasRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.OdometerRecord:
                        {
                            var existingRecord = _odometerRecordDataAccess.GetOdometerRecordById(recordId);
                            existingRecord.Mileage += int.Parse(vehicleData.OdometerDifference);
                            existingRecord.Mileage = decimal.ToInt32(existingRecord.Mileage * decimal.Parse(vehicleData.OdometerMultiplier, NumberStyles.Any));
                            result = _odometerRecordDataAccess.SaveOdometerRecordToVehicle(existingRecord);
                        }
                        break;
                }
            }
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Adjusted odometer for multiple {importMode.ToString()} - Ids: {string.Join(",", recordIds)}");
            }
            return Json(result);
        }
        public IActionResult DuplicateRecords(List<int> recordIds, ImportMode importMode)
        {
            bool result = false;
            foreach (int recordId in recordIds)
            {
                switch (importMode)
                {
                    case ImportMode.ServiceRecord:
                        {
                            var existingRecord = _serviceRecordDataAccess.GetServiceRecordById(recordId);
                            existingRecord.Id = default;
                            result = _serviceRecordDataAccess.SaveServiceRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.RepairRecord:
                        {
                            var existingRecord = _collisionRecordDataAccess.GetCollisionRecordById(recordId);
                            existingRecord.Id = default;
                            result = _collisionRecordDataAccess.SaveCollisionRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.UpgradeRecord:
                        {
                            var existingRecord = _upgradeRecordDataAccess.GetUpgradeRecordById(recordId);
                            existingRecord.Id = default;
                            result = _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.GasRecord:
                        {
                            var existingRecord = _gasRecordDataAccess.GetGasRecordById(recordId);
                            existingRecord.Id = default;
                            result = _gasRecordDataAccess.SaveGasRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.TaxRecord:
                        {
                            var existingRecord = _taxRecordDataAccess.GetTaxRecordById(recordId);
                            existingRecord.Id = default;
                            result = _taxRecordDataAccess.SaveTaxRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.SupplyRecord:
                        {
                            var existingRecord = _supplyRecordDataAccess.GetSupplyRecordById(recordId);
                            existingRecord.Id = default;
                            result = _supplyRecordDataAccess.SaveSupplyRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.NoteRecord:
                        {
                            var existingRecord = _noteDataAccess.GetNoteById(recordId);
                            existingRecord.Id = default;
                            result = _noteDataAccess.SaveNoteToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.OdometerRecord:
                        {
                            var existingRecord = _odometerRecordDataAccess.GetOdometerRecordById(recordId);
                            existingRecord.Id = default;
                            result = _odometerRecordDataAccess.SaveOdometerRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.ReminderRecord:
                        {
                            var existingRecord = _reminderRecordDataAccess.GetReminderRecordById(recordId);
                            existingRecord.Id = default;
                            result = _reminderRecordDataAccess.SaveReminderRecordToVehicle(existingRecord);
                        }
                        break;
                }
            }
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Duplicated multiple {importMode.ToString()} - Ids: {string.Join(",", recordIds)}");
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult GetGenericRecordModal(List<int> recordIds, ImportMode dataType)
        {
            return PartialView("_GenericRecordModal", new GenericRecordEditModel() { DataType = dataType, RecordIds = recordIds });
        }
        [HttpPost]
        public IActionResult EditMultipleRecords(GenericRecordEditModel genericRecordEditModel)
        {
            var dateIsEdited = genericRecordEditModel.EditRecord.Date != default;
            var descriptionIsEdited = !string.IsNullOrWhiteSpace(genericRecordEditModel.EditRecord.Description);
            var mileageIsEdited = genericRecordEditModel.EditRecord.Mileage != default;
            var costIsEdited = genericRecordEditModel.EditRecord.Cost != default;
            var noteIsEdited = !string.IsNullOrWhiteSpace(genericRecordEditModel.EditRecord.Notes);
            var tagsIsEdited = genericRecordEditModel.EditRecord.Tags.Any();
            //handle clear overrides
            if (tagsIsEdited && genericRecordEditModel.EditRecord.Tags.Contains("---"))
            {
                genericRecordEditModel.EditRecord.Tags = new List<string>();
            }
            if (noteIsEdited && genericRecordEditModel.EditRecord.Notes == "---")
            {
                genericRecordEditModel.EditRecord.Notes = "";
            }
            bool result = false;
            foreach (int recordId in genericRecordEditModel.RecordIds)
            {
                switch (genericRecordEditModel.DataType)
                {
                    case ImportMode.ServiceRecord:
                        {
                            var existingRecord = _serviceRecordDataAccess.GetServiceRecordById(recordId);
                            if (dateIsEdited)
                            {
                                existingRecord.Date = genericRecordEditModel.EditRecord.Date;
                            }
                            if (descriptionIsEdited)
                            {
                                existingRecord.Description = genericRecordEditModel.EditRecord.Description;
                            }
                            if (mileageIsEdited)
                            {
                                existingRecord.Mileage = genericRecordEditModel.EditRecord.Mileage;
                            }
                            if (costIsEdited)
                            {
                                existingRecord.Cost = genericRecordEditModel.EditRecord.Cost;
                            }
                            if (noteIsEdited)
                            {
                                existingRecord.Notes = genericRecordEditModel.EditRecord.Notes;
                            }
                            if (tagsIsEdited)
                            {
                                existingRecord.Tags = genericRecordEditModel.EditRecord.Tags;
                            }
                            result = _serviceRecordDataAccess.SaveServiceRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.RepairRecord:
                        {
                            var existingRecord = _collisionRecordDataAccess.GetCollisionRecordById(recordId);
                            if (dateIsEdited)
                            {
                                existingRecord.Date = genericRecordEditModel.EditRecord.Date;
                            }
                            if (descriptionIsEdited)
                            {
                                existingRecord.Description = genericRecordEditModel.EditRecord.Description;
                            }
                            if (mileageIsEdited)
                            {
                                existingRecord.Mileage = genericRecordEditModel.EditRecord.Mileage;
                            }
                            if (costIsEdited)
                            {
                                existingRecord.Cost = genericRecordEditModel.EditRecord.Cost;
                            }
                            if (noteIsEdited)
                            {
                                existingRecord.Notes = genericRecordEditModel.EditRecord.Notes;
                            }
                            if (tagsIsEdited)
                            {
                                existingRecord.Tags = genericRecordEditModel.EditRecord.Tags;
                            }
                            result = _collisionRecordDataAccess.SaveCollisionRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.UpgradeRecord:
                        {
                            var existingRecord = _upgradeRecordDataAccess.GetUpgradeRecordById(recordId);
                            if (dateIsEdited)
                            {
                                existingRecord.Date = genericRecordEditModel.EditRecord.Date;
                            }
                            if (descriptionIsEdited)
                            {
                                existingRecord.Description = genericRecordEditModel.EditRecord.Description;
                            }
                            if (mileageIsEdited)
                            {
                                existingRecord.Mileage = genericRecordEditModel.EditRecord.Mileage;
                            }
                            if (costIsEdited)
                            {
                                existingRecord.Cost = genericRecordEditModel.EditRecord.Cost;
                            }
                            if (noteIsEdited)
                            {
                                existingRecord.Notes = genericRecordEditModel.EditRecord.Notes;
                            }
                            if (tagsIsEdited)
                            {
                                existingRecord.Tags = genericRecordEditModel.EditRecord.Tags;
                            }
                            result = _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(existingRecord);
                        }
                        break;
                }
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult SaveUserColumnPreferences(UserColumnPreference columnPreference)
        {
            try
            {
                var userConfig = _config.GetUserConfig(User);
                var existingUserColumnPreference = userConfig.UserColumnPreferences.Where(x => x.Tab == columnPreference.Tab);
                if (existingUserColumnPreference.Any())
                {
                    var existingPreference = existingUserColumnPreference.Single();
                    existingPreference.VisibleColumns = columnPreference.VisibleColumns;
                }
                else
                {
                    userConfig.UserColumnPreferences.Add(columnPreference);
                }
                var result = _config.SaveUserConfig(User, userConfig);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(false);
            }
        }
        #endregion
    }
}

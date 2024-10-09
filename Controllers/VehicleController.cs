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
using System.Text.Json;

namespace CarCareTracker.Controllers
{
    [Authorize]
    public partial class VehicleController : Controller
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
            var fileNameToExport = $"temp/{Guid.NewGuid()}.csv";
            var fullExportFilePath = _fileHelper.GetFullFilePath(fileNameToExport, false);
            if (mode == ImportMode.ServiceRecord)
            {
                var vehicleRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
                if (vehicleRecords.Any())
                {
                    var exportData = vehicleRecords.Select(x => new GenericRecordExportModel { 
                        Date = x.Date.ToShortDateString(), 
                        Description = x.Description, 
                        Cost = x.Cost.ToString("C"), 
                        Notes = x.Notes, 
                        Odometer = x.Mileage.ToString(), 
                        Tags = string.Join(" ", x.Tags), 
                        ExtraFields = x.ExtraFields 
                    });
                    using (var writer = new StreamWriter(fullExportFilePath))
                    {
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            //custom writer
                            StaticHelper.WriteGenericRecordExportModel(csv, exportData);
                        }
                        writer.Dispose();
                    }
                    return Json($"/{fileNameToExport}");
                }
            }
            else if (mode == ImportMode.RepairRecord)
            {
                var vehicleRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
                if (vehicleRecords.Any())
                {
                    var exportData = vehicleRecords.Select(x => new GenericRecordExportModel { 
                        Date = x.Date.ToShortDateString(), 
                        Description = x.Description, 
                        Cost = x.Cost.ToString("C"), 
                        Notes = x.Notes, 
                        Odometer = x.Mileage.ToString(), 
                        Tags = string.Join(" ", x.Tags), 
                        ExtraFields = x.ExtraFields 
                    });
                    using (var writer = new StreamWriter(fullExportFilePath))
                    {
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            StaticHelper.WriteGenericRecordExportModel(csv, exportData);
                        }
                    }
                    return Json($"/{fileNameToExport}");
                }
            }
            else if (mode == ImportMode.UpgradeRecord)
            {
                var vehicleRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
                if (vehicleRecords.Any())
                {
                    var exportData = vehicleRecords.Select(x => new GenericRecordExportModel { 
                        Date = x.Date.ToShortDateString(), 
                        Description = x.Description, 
                        Cost = x.Cost.ToString("C"), 
                        Notes = x.Notes, 
                        Odometer = x.Mileage.ToString(), 
                        Tags = string.Join(" ", x.Tags), 
                        ExtraFields = x.ExtraFields 
                    });
                    using (var writer = new StreamWriter(fullExportFilePath))
                    {
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            StaticHelper.WriteGenericRecordExportModel(csv, exportData);
                        }
                    }
                    return Json($"/{fileNameToExport}");
                }
            }
            else if (mode == ImportMode.OdometerRecord)
            {
                var vehicleRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
                if (vehicleRecords.Any())
                {
                    var exportData = vehicleRecords.Select(x => new OdometerRecordExportModel { 
                        Date = x.Date.ToShortDateString(), 
                        Notes = x.Notes, 
                        InitialOdometer = x.InitialMileage.ToString(), 
                        Odometer = x.Mileage.ToString(), 
                        Tags = string.Join(" ", x.Tags), 
                        ExtraFields = x.ExtraFields 
                    });
                    using (var writer = new StreamWriter(fullExportFilePath))
                    {
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            StaticHelper.WriteOdometerRecordExportModel(csv, exportData);
                        }
                    }
                    return Json($"/{fileNameToExport}");
                }
            }
            else if (mode == ImportMode.SupplyRecord)
            {
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
                        Tags = string.Join(" ", x.Tags),
                        ExtraFields = x.ExtraFields
                    });
                    using (var writer = new StreamWriter(fullExportFilePath))
                    {
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            StaticHelper.WriteSupplyRecordExportModel(csv, exportData);
                        }
                    }
                    return Json($"/{fileNameToExport}");
                }
            }
            else if (mode == ImportMode.TaxRecord)
            {
                var vehicleRecords = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
                if (vehicleRecords.Any())
                {
                    var exportData = vehicleRecords.Select(x => new TaxRecordExportModel { 
                        Date = x.Date.ToShortDateString(), 
                        Description = x.Description, 
                        Cost = x.Cost.ToString("C"), 
                        Notes = x.Notes, 
                        Tags = string.Join(" ", x.Tags), 
                        ExtraFields = x.ExtraFields 
                    });
                    using (var writer = new StreamWriter(fullExportFilePath))
                    {
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            StaticHelper.WriteTaxRecordExportModel(csv, exportData);
                        }
                    }
                    return Json($"/{fileNameToExport}");
                }
            }
            else if (mode == ImportMode.PlanRecord)
            {
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
                        Notes = x.Notes,
                        ExtraFields = x.ExtraFields
                    });
                    using (var writer = new StreamWriter(fullExportFilePath))
                    {
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            StaticHelper.WritePlanRecordExportModel(csv, exportData);
                        }
                    }
                    return Json($"/{fileNameToExport}");
                }
            }
            else if (mode == ImportMode.GasRecord)
            {
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
                    Tags = string.Join(" ", x.Tags),
                    ExtraFields = x.ExtraFields
                });
                using (var writer = new StreamWriter(fullExportFilePath))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        StaticHelper.WriteGasRecordExportModel(csv, exportData);
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
                    var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture);
                    config.MissingFieldFound = null;
                    config.HeaderValidated = null;
                    config.PrepareHeaderForMatch = args => { return args.Header.Trim().ToLower(); };
                    using (var csv = new CsvReader(reader, config))
                    {
                        csv.Context.RegisterClassMap<ImportMapper>();
                        var records = csv.GetRecords<ImportModel>().ToList();
                        if (records.Any())
                        {
                            var requiredExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)mode).ExtraFields.Where(x=>x.IsRequired).Select(y=>y.Name);
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
                                        Tags = string.IsNullOrWhiteSpace(importModel.Tags) ? [] : importModel.Tags.Split(" ").ToList(),
                                        ExtraFields = importModel.ExtraFields.Any() ? importModel.ExtraFields.Select(x => new ExtraField { Name = x.Key, Value = x.Value, IsRequired = requiredExtraFields.Contains(x.Key) }).ToList() : new List<ExtraField>()
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
                                        Tags = string.IsNullOrWhiteSpace(importModel.Tags) ? [] : importModel.Tags.Split(" ").ToList(),
                                        ExtraFields = importModel.ExtraFields.Any() ? importModel.ExtraFields.Select(x => new ExtraField { Name = x.Key, Value = x.Value, IsRequired = requiredExtraFields.Contains(x.Key) }).ToList() : new List<ExtraField>()
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
                                        Tags = string.IsNullOrWhiteSpace(importModel.Tags) ? [] : importModel.Tags.Split(" ").ToList(),
                                        ExtraFields = importModel.ExtraFields.Any() ? importModel.ExtraFields.Select(x => new ExtraField { Name = x.Key, Value = x.Value, IsRequired = requiredExtraFields.Contains(x.Key) }).ToList() : new List<ExtraField>()
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
                                        Cost = decimal.Parse(importModel.Cost, NumberStyles.Any),
                                        ExtraFields = importModel.ExtraFields.Any() ? importModel.ExtraFields.Select(x => new ExtraField { Name = x.Key, Value = x.Value, IsRequired = requiredExtraFields.Contains(x.Key) }).ToList() : new List<ExtraField>()
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
                                        Tags = string.IsNullOrWhiteSpace(importModel.Tags) ? [] : importModel.Tags.Split(" ").ToList(),
                                        ExtraFields = importModel.ExtraFields.Any() ? importModel.ExtraFields.Select(x => new ExtraField { Name = x.Key, Value = x.Value, IsRequired = requiredExtraFields.Contains(x.Key) }).ToList() : new List<ExtraField>()
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
                                        Tags = string.IsNullOrWhiteSpace(importModel.Tags) ? [] : importModel.Tags.Split(" ").ToList(),
                                        ExtraFields = importModel.ExtraFields.Any() ? importModel.ExtraFields.Select(x => new ExtraField { Name = x.Key, Value = x.Value, IsRequired = requiredExtraFields.Contains(x.Key) }).ToList() : new List<ExtraField>()
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
                                        Tags = string.IsNullOrWhiteSpace(importModel.Tags) ? [] : importModel.Tags.Split(" ").ToList(),
                                        ExtraFields = importModel.ExtraFields.Any() ? importModel.ExtraFields.Select(x => new ExtraField { Name = x.Key, Value = x.Value, IsRequired = requiredExtraFields.Contains(x.Key) }).ToList() : new List<ExtraField>()
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
                                        Tags = string.IsNullOrWhiteSpace(importModel.Tags) ? [] : importModel.Tags.Split(" ").ToList(),
                                        ExtraFields = importModel.ExtraFields.Any() ? importModel.ExtraFields.Select(x => new ExtraField { Name = x.Key, Value = x.Value, IsRequired = requiredExtraFields.Contains(x.Key) }).ToList() : new List<ExtraField>()
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
        #region "Shared Methods"
        [HttpPost]
        public IActionResult GetFilesPendingUpload(List<UploadedFiles> uploadedFiles)
        {
            var filesPendingUpload = uploadedFiles.Where(x => x.Location.StartsWith("/temp/")).ToList();
            return PartialView("_FilesToUpload", filesPendingUpload);
        }
        [HttpPost]
        [TypeFilter(typeof(CollaboratorFilter))]
        public IActionResult SearchRecords(int vehicleId, string searchQuery)
        {
            List<SearchResult> searchResults = new List<SearchResult>();
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return Json(searchResults);
            }
            foreach(ImportMode visibleTab in _config.GetUserConfig(User).VisibleTabs)
            {
                switch (visibleTab)
                {
                    case ImportMode.ServiceRecord:
                        {
                            var results = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.ServiceRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                        }
                        break;
                    case ImportMode.RepairRecord:
                        {
                            var results = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.RepairRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                        }
                        break;
                    case ImportMode.UpgradeRecord:
                        {
                            var results = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.UpgradeRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                        }
                        break;
                    case ImportMode.TaxRecord:
                        {
                            var results = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.TaxRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                        }
                        break;
                    case ImportMode.SupplyRecord:
                        {
                            var results = _supplyRecordDataAccess.GetSupplyRecordsByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.SupplyRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                        }
                        break;
                    case ImportMode.PlanRecord:
                        {
                            var results = _planRecordDataAccess.GetPlanRecordsByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.PlanRecord, Description = $"{x.DateCreated.ToShortDateString()} - {x.Description}" }));
                        }
                        break;
                    case ImportMode.OdometerRecord:
                        {
                            var results = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.OdometerRecord, Description = $"{x.Date.ToShortDateString()} - {x.Mileage}" }));
                        }
                        break;
                    case ImportMode.GasRecord:
                        {
                            var results = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.GasRecord, Description = $"{x.Date.ToShortDateString()} - {x.Mileage}" }));
                        }
                        break;
                    case ImportMode.NoteRecord:
                        {
                            var results = _noteDataAccess.GetNotesByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.NoteRecord, Description = $"{x.Description}" }));
                        }
                        break;
                    case ImportMode.ReminderRecord:
                        {
                            var results = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.ReminderRecord, Description = $"{x.Description}" }));
                        }
                        break;
                }
            }
            return PartialView("_GlobalSearchResult", searchResults);
        }
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

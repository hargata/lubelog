using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.MapProfile;
using CarCareTracker.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CarCareTracker.Controllers
{
    public partial class VehicleController
    {
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
                    var exportData = vehicleRecords.Select(x => new GenericRecordExportModel
                    {
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
                    var exportData = vehicleRecords.Select(x => new GenericRecordExportModel
                    {
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
                    var exportData = vehicleRecords.Select(x => new GenericRecordExportModel
                    {
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
                    var exportData = vehicleRecords.Select(x => new OdometerRecordExportModel
                    {
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
                    var exportData = vehicleRecords.Select(x => new TaxRecordExportModel
                    {
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
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture);
                    config.MissingFieldFound = null;
                    config.HeaderValidated = null;
                    config.PrepareHeaderForMatch = args => { return args.Header.Trim().ToLower(); };
                    using (var csv = new CsvReader(reader, config))
                    {
                        csv.Context.RegisterClassMap<ImportMapper>();
                        var records = csv.GetRecords<ImportModel>().ToList();
                        if (records.Any())
                        {
                            var requiredExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)mode).ExtraFields.Where(x => x.IsRequired).Select(y => y.Name);
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
    }
}

using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CarCareTracker.Controllers
{
    public partial class VehicleController
    {
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetReportPartialView(int vehicleId)
        {
            //get records
            var vehicleData = _dataAccess.GetVehicleById(vehicleId);
            var vehicleRecords = _vehicleLogic.GetVehicleRecords(vehicleId);
            var serviceRecords = vehicleRecords.ServiceRecords;
            var gasRecords = vehicleRecords.GasRecords;
            var collisionRecords = vehicleRecords.CollisionRecords;
            var taxRecords = vehicleRecords.TaxRecords;
            var upgradeRecords = vehicleRecords.UpgradeRecords;
            var odometerRecords = vehicleRecords.OdometerRecords;
            var userConfig = _config.GetUserConfig(User);
            var viewModel = new ReportViewModel() { ReportHeaderForVehicle = new ReportHeader() };
            //check if custom widgets are configured
            viewModel.CustomWidgetsConfigured = _fileHelper.WidgetsExist();
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

            //set available metrics
            var visibleTabs = userConfig.VisibleTabs;
            if (visibleTabs.Contains(ImportMode.OdometerRecord) || odometerRecords.Any())
            {
                viewModel.AvailableMetrics.Add(ImportMode.OdometerRecord);
            }
            if (visibleTabs.Contains(ImportMode.ServiceRecord) || serviceRecords.Any())
            {
                viewModel.AvailableMetrics.Add(ImportMode.ServiceRecord);
            }
            if (visibleTabs.Contains(ImportMode.RepairRecord) || collisionRecords.Any())
            {
                viewModel.AvailableMetrics.Add(ImportMode.RepairRecord);
            }
            if (visibleTabs.Contains(ImportMode.UpgradeRecord) || upgradeRecords.Any())
            {
                viewModel.AvailableMetrics.Add(ImportMode.UpgradeRecord);
            }
            if (visibleTabs.Contains(ImportMode.GasRecord) || gasRecords.Any())
            {
                viewModel.AvailableMetrics.Add(ImportMode.GasRecord);
            }
            if (visibleTabs.Contains(ImportMode.TaxRecord) || taxRecords.Any())
            {
                viewModel.AvailableMetrics.Add(ImportMode.TaxRecord);
            }

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
            if (odometerRecords.Any())
            {
                numbersArray.Add(odometerRecords.Min(x => x.Date.Year));
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
            var mileageData = _gasHelper.GetGasRecordViewModels(gasRecords, userConfig.UseMPG, userConfig.UseUKMPG);
            string preferredFuelMileageUnit = _config.GetUserConfig(User).PreferredGasMileageUnit;
            var fuelEconomyMileageUnit = StaticHelper.GetFuelEconomyUnit(vehicleData.IsElectric, vehicleData.UseHours, userConfig.UseMPG, userConfig.UseUKMPG);
            var averageMPG = _gasHelper.GetAverageGasMileage(mileageData, userConfig.UseMPG);
            mileageData.RemoveAll(x => x.MilesPerGallon == default);
            bool invertedFuelMileageUnit = fuelEconomyMileageUnit == "l/100km" && preferredFuelMileageUnit == "km/l";
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
            if (invertedFuelMileageUnit)
            {
                foreach (CostForVehicleByMonth monthMileage in monthlyMileageData)
                {
                    if (monthMileage.Cost != default)
                    {
                        monthMileage.Cost = 100 / monthMileage.Cost;
                    }
                }
                var newAverageMPG = decimal.Parse(averageMPG, NumberStyles.Any);
                if (newAverageMPG != 0)
                {
                    newAverageMPG = 100 / newAverageMPG;
                }
                averageMPG = newAverageMPG.ToString("F");
            }
            var mpgViewModel = new MPGForVehicleByMonth {
                CostData = monthlyMileageData,
                Unit = invertedFuelMileageUnit ? preferredFuelMileageUnit : fuelEconomyMileageUnit,
                SortedCostData = (userConfig.UseMPG || invertedFuelMileageUnit) ? monthlyMileageData.OrderByDescending(x => x.Cost).ToList() : monthlyMileageData.OrderBy(x => x.Cost).ToList()
            };
            viewModel.FuelMileageForVehicleByMonth = mpgViewModel;
            //report header

            var maxMileage = _vehicleLogic.GetMaxMileage(vehicleRecords);
            var minMileage = _vehicleLogic.GetMinMileage(vehicleRecords);

            viewModel.ReportHeaderForVehicle.TotalCost = _vehicleLogic.GetVehicleTotalCost(vehicleRecords);
            viewModel.ReportHeaderForVehicle.AverageMPG = $"{averageMPG} {mpgViewModel.Unit}";
            viewModel.ReportHeaderForVehicle.MaxOdometer = maxMileage;
            viewModel.ReportHeaderForVehicle.DistanceTraveled = maxMileage - minMileage;
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
        [HttpPost]
        public IActionResult GetSummaryForVehicle(int vehicleId, int year = 0)
        {
            var vehicleData = _dataAccess.GetVehicleById(vehicleId);
            var vehicleRecords = _vehicleLogic.GetVehicleRecords(vehicleId);

            var serviceRecords = vehicleRecords.ServiceRecords;
            var gasRecords = vehicleRecords.GasRecords;
            var collisionRecords = vehicleRecords.CollisionRecords;
            var taxRecords = vehicleRecords.TaxRecords;
            var upgradeRecords = vehicleRecords.UpgradeRecords;
            var odometerRecords = vehicleRecords.OdometerRecords;

            if (year != default)
            {
                serviceRecords.RemoveAll(x => x.Date.Year != year);
                gasRecords.RemoveAll(x => x.Date.Year != year);
                collisionRecords.RemoveAll(x => x.Date.Year != year);
                taxRecords.RemoveAll(x => x.Date.Year != year);
                upgradeRecords.RemoveAll(x => x.Date.Year != year);
                odometerRecords.RemoveAll(x => x.Date.Year != year);
            }

            var userConfig = _config.GetUserConfig(User);

            var mileageData = _gasHelper.GetGasRecordViewModels(gasRecords, userConfig.UseMPG, userConfig.UseUKMPG);
            string preferredFuelMileageUnit = _config.GetUserConfig(User).PreferredGasMileageUnit;
            var fuelEconomyMileageUnit = StaticHelper.GetFuelEconomyUnit(vehicleData.IsElectric, vehicleData.UseHours, userConfig.UseMPG, userConfig.UseUKMPG);
            var averageMPG = _gasHelper.GetAverageGasMileage(mileageData, userConfig.UseMPG);
            bool invertedFuelMileageUnit = fuelEconomyMileageUnit == "l/100km" && preferredFuelMileageUnit == "km/l";

            if (invertedFuelMileageUnit)
            {
                var newAverageMPG = decimal.Parse(averageMPG, NumberStyles.Any);
                if (newAverageMPG != 0)
                {
                    newAverageMPG = 100 / newAverageMPG;
                }
                averageMPG = newAverageMPG.ToString("F");
            }

            var mpgUnit = invertedFuelMileageUnit ? preferredFuelMileageUnit : fuelEconomyMileageUnit;

            var maxMileage = _vehicleLogic.GetMaxMileage(vehicleRecords);
            var minMileage = _vehicleLogic.GetMinMileage(vehicleRecords);

            var viewModel = new ReportHeader()
            {
                TotalCost = _vehicleLogic.GetVehicleTotalCost(vehicleRecords),
                AverageMPG = $"{averageMPG} {mpgUnit}",
                MaxOdometer = maxMileage,
                DistanceTraveled = maxMileage - minMileage
            };

            return PartialView("_ReportHeader", viewModel);
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
        [HttpGet]
        public IActionResult GetCostTableForVehicle(int vehicleId, int year = 0)
        {
            var vehicleRecords = _vehicleLogic.GetVehicleRecords(vehicleId);
            var serviceRecords = vehicleRecords.ServiceRecords;
            var gasRecords = vehicleRecords.GasRecords;
            var collisionRecords = vehicleRecords.CollisionRecords;
            var taxRecords = vehicleRecords.TaxRecords;
            var upgradeRecords = vehicleRecords.UpgradeRecords;
            var odometerRecords = vehicleRecords.OdometerRecords;
            if (year != default)
            {
                serviceRecords.RemoveAll(x => x.Date.Year != year);
                gasRecords.RemoveAll(x => x.Date.Year != year);
                collisionRecords.RemoveAll(x => x.Date.Year != year);
                taxRecords.RemoveAll(x => x.Date.Year != year);
                upgradeRecords.RemoveAll(x => x.Date.Year != year);
                odometerRecords.RemoveAll(x => x.Date.Year != year);
            }
            var maxMileage = _vehicleLogic.GetMaxMileage(vehicleRecords);
            var minMileage = _vehicleLogic.GetMinMileage(vehicleRecords);
            var vehicleData = _dataAccess.GetVehicleById(vehicleId);
            var userConfig = _config.GetUserConfig(User);
            var totalDistanceTraveled = maxMileage - minMileage;
            var totalDays = _vehicleLogic.GetOwnershipDays(vehicleData.PurchaseDate, vehicleData.SoldDate, year, serviceRecords, collisionRecords, gasRecords, upgradeRecords, odometerRecords, taxRecords);
            var viewModel = new CostTableForVehicle
            {
                ServiceRecordSum = serviceRecords.Sum(x => x.Cost),
                GasRecordSum = gasRecords.Sum(x => x.Cost),
                CollisionRecordSum = collisionRecords.Sum(x => x.Cost),
                TaxRecordSum = taxRecords.Sum(x => x.Cost),
                UpgradeRecordSum = upgradeRecords.Sum(x => x.Cost),
                TotalDistance = totalDistanceTraveled,
                DistanceUnit = vehicleData.UseHours ? "Cost Per Hour" : userConfig.UseMPG ? "Cost Per Mile" : "Cost Per Kilometer",
                NumberOfDays = totalDays
            };
            return PartialView("_CostTableReport", viewModel);
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
            if (exportTabs.Contains(ImportMode.NoteRecord))
            {
                var records = _noteDataAccess.GetNotesByVehicleId(vehicleId).Where(x => x.Files.Any());
                attachmentData.AddRange(records.Select(x => new GenericReportModel
                {
                    DataType = ImportMode.NoteRecord,
                    Date = DateTime.Now,
                    Odometer = 0,
                    Files = x.Files
                }));
            }
            if (attachmentData.Any())
            {
                attachmentData = attachmentData.OrderBy(x => x.Date).ThenBy(x => x.Odometer).ToList();
                var result = _fileHelper.MakeAttachmentsExport(attachmentData);
                if (string.IsNullOrWhiteSpace(result))
                {
                    return Json(OperationResponse.Failed());
                }
                return Json(OperationResponse.Succeed(result));
            }
            else
            {
                return Json(OperationResponse.Failed("No Attachments Found"));
            }
        }
        public IActionResult GetReportParameters()
        {
            var viewModel = new ReportParameter() { 
                VisibleColumns = new List<string> {
                    nameof(GenericReportModel.DataType),
                    nameof(GenericReportModel.Date),
                    nameof(GenericReportModel.Odometer),
                    nameof(GenericReportModel.Description),
                    nameof(GenericReportModel.Cost),
                    nameof(GenericReportModel.Notes) 
                }
            };
            //get all extra fields from service records, repairs, upgrades, and tax records.
            var recordTypes = new List<int>() { 0, 1, 3, 4 };
            var extraFields = new List<string>();
            foreach(int recordType in recordTypes)
            {
                extraFields.AddRange(_extraFieldDataAccess.GetExtraFieldsById(recordType).ExtraFields.Select(x => x.Name));
            }
            viewModel.ExtraFields = extraFields.Distinct().ToList();

            return PartialView("_ReportParameters", viewModel);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        public IActionResult GetVehicleHistory(int vehicleId, ReportParameter reportParameter)
        {
            var vehicleHistory = new VehicleHistoryViewModel();
            vehicleHistory.ReportParameters = reportParameter;
            vehicleHistory.VehicleData = _dataAccess.GetVehicleById(vehicleId);
            var vehicleRecords = _vehicleLogic.GetVehicleRecords(vehicleId);
            bool useMPG = _config.GetUserConfig(User).UseMPG;
            bool useUKMPG = _config.GetUserConfig(User).UseUKMPG;
            var gasViewModels = _gasHelper.GetGasRecordViewModels(vehicleRecords.GasRecords, useMPG, useUKMPG);
            //filter by tags
            if (reportParameter.Tags.Any())
            {
                if (reportParameter.TagFilter == TagFilter.Exclude)
                {
                    vehicleRecords.OdometerRecords.RemoveAll(x => x.Tags.Any(y => reportParameter.Tags.Contains(y)));
                    vehicleRecords.ServiceRecords.RemoveAll(x => x.Tags.Any(y => reportParameter.Tags.Contains(y)));
                    vehicleRecords.CollisionRecords.RemoveAll(x => x.Tags.Any(y => reportParameter.Tags.Contains(y)));
                    vehicleRecords.UpgradeRecords.RemoveAll(x => x.Tags.Any(y => reportParameter.Tags.Contains(y)));
                    vehicleRecords.TaxRecords.RemoveAll(x => x.Tags.Any(y => reportParameter.Tags.Contains(y)));
                    gasViewModels.RemoveAll(x => x.Tags.Any(y => reportParameter.Tags.Contains(y)));
                    vehicleRecords.GasRecords.RemoveAll(x => x.Tags.Any(y => reportParameter.Tags.Contains(y)));
                }
                else if (reportParameter.TagFilter == TagFilter.IncludeOnly)
                {
                    vehicleRecords.OdometerRecords.RemoveAll(x => !x.Tags.Any(y => reportParameter.Tags.Contains(y)));
                    vehicleRecords.ServiceRecords.RemoveAll(x => !x.Tags.Any(y => reportParameter.Tags.Contains(y)));
                    vehicleRecords.CollisionRecords.RemoveAll(x => !x.Tags.Any(y => reportParameter.Tags.Contains(y)));
                    vehicleRecords.UpgradeRecords.RemoveAll(x => !x.Tags.Any(y => reportParameter.Tags.Contains(y)));
                    vehicleRecords.TaxRecords.RemoveAll(x => !x.Tags.Any(y => reportParameter.Tags.Contains(y)));
                    gasViewModels.RemoveAll(x => !x.Tags.Any(y => reportParameter.Tags.Contains(y)));
                    vehicleRecords.GasRecords.RemoveAll(x => !x.Tags.Any(y => reportParameter.Tags.Contains(y)));
                }
            }
            //filter by date range.
            if (reportParameter.FilterByDateRange && !string.IsNullOrWhiteSpace(reportParameter.StartDate) && !string.IsNullOrWhiteSpace(reportParameter.EndDate))
            {
                var startDate = DateTime.Parse(reportParameter.StartDate).Date;
                var endDate = DateTime.Parse(reportParameter.EndDate).Date;
                //validate date range
                if (endDate >= startDate) //allow for same day.
                {
                    vehicleHistory.StartDate = reportParameter.StartDate;
                    vehicleHistory.EndDate = reportParameter.EndDate;
                    //remove all records with dates after the end date and dates before the start date.
                    vehicleRecords.OdometerRecords.RemoveAll(x => x.Date.Date > endDate || x.Date.Date < startDate);
                    vehicleRecords.ServiceRecords.RemoveAll(x => x.Date.Date > endDate || x.Date.Date < startDate);
                    vehicleRecords.CollisionRecords.RemoveAll(x => x.Date.Date > endDate || x.Date.Date < startDate);
                    vehicleRecords.UpgradeRecords.RemoveAll(x => x.Date.Date > endDate || x.Date.Date < startDate);
                    vehicleRecords.TaxRecords.RemoveAll(x => x.Date.Date > endDate || x.Date.Date < startDate);
                    gasViewModels.RemoveAll(x => DateTime.Parse(x.Date).Date > endDate || DateTime.Parse(x.Date).Date < startDate);
                    vehicleRecords.GasRecords.RemoveAll(x => x.Date.Date > endDate || x.Date.Date < startDate);
                }
            }
            var maxMileage = _vehicleLogic.GetMaxMileage(vehicleRecords);
            vehicleHistory.Odometer = maxMileage.ToString("N0");
            var minMileage = _vehicleLogic.GetMinMileage(vehicleRecords);
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
            string preferredFuelMileageUnit = _config.GetUserConfig(User).PreferredGasMileageUnit;
            vehicleHistory.DistanceUnit = vehicleHistory.VehicleData.UseHours ? "h" : useMPG ? "mi." : "km";
            vehicleHistory.TotalGasCost = gasViewModels.Sum(x => x.Cost);
            vehicleHistory.TotalCost = vehicleRecords.ServiceRecords.Sum(x => x.Cost) + vehicleRecords.CollisionRecords.Sum(x => x.Cost) + vehicleRecords.UpgradeRecords.Sum(x => x.Cost) + vehicleRecords.TaxRecords.Sum(x => x.Cost);
            if (distanceTraveled != default)
            {
                vehicleHistory.DistanceTraveled = distanceTraveled.ToString("N0");
                vehicleHistory.TotalCostPerMile = vehicleHistory.TotalCost / distanceTraveled;
                vehicleHistory.TotalGasCostPerMile = vehicleHistory.TotalGasCost / distanceTraveled;
            }
            var averageMPG = "0";
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
            reportData.AddRange(vehicleRecords.ServiceRecords.Select(x => new GenericReportModel
            {
                Date = x.Date,
                Odometer = x.Mileage,
                Description = x.Description,
                Notes = x.Notes,
                Cost = x.Cost,
                DataType = ImportMode.ServiceRecord,
                ExtraFields = x.ExtraFields,
                RequisitionHistory = x.RequisitionHistory
            }));
            //repair records
            reportData.AddRange(vehicleRecords.CollisionRecords.Select(x => new GenericReportModel
            {
                Date = x.Date,
                Odometer = x.Mileage,
                Description = x.Description,
                Notes = x.Notes,
                Cost = x.Cost,
                DataType = ImportMode.RepairRecord,
                ExtraFields = x.ExtraFields,
                RequisitionHistory = x.RequisitionHistory
            }));
            reportData.AddRange(vehicleRecords.UpgradeRecords.Select(x => new GenericReportModel
            {
                Date = x.Date,
                Odometer = x.Mileage,
                Description = x.Description,
                Notes = x.Notes,
                Cost = x.Cost,
                DataType = ImportMode.UpgradeRecord,
                ExtraFields = x.ExtraFields,
                RequisitionHistory = x.RequisitionHistory
            }));
            reportData.AddRange(vehicleRecords.TaxRecords.Select(x => new GenericReportModel
            {
                Date = x.Date,
                Odometer = 0,
                Description = x.Description,
                Notes = x.Notes,
                Cost = x.Cost,
                DataType = ImportMode.TaxRecord,
                ExtraFields = x.ExtraFields
            }));
            vehicleHistory.VehicleHistory = reportData.OrderBy(x => x.Date).ThenBy(x => x.Odometer).ToList();
            return PartialView("_VehicleHistory", vehicleHistory);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        public IActionResult GetMonthMPGByVehicle(int vehicleId, int year = 0)
        {
            var vehicleData = _dataAccess.GetVehicleById(vehicleId);
            var gasRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            var userConfig = _config.GetUserConfig(User);
            string preferredFuelMileageUnit = _config.GetUserConfig(User).PreferredGasMileageUnit;
            var fuelEconomyMileageUnit = StaticHelper.GetFuelEconomyUnit(vehicleData.IsElectric, vehicleData.UseHours, userConfig.UseMPG, userConfig.UseUKMPG);
            bool invertedFuelMileageUnit = fuelEconomyMileageUnit == "l/100km" && preferredFuelMileageUnit == "km/l";
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
            if (invertedFuelMileageUnit)
            {
                foreach (CostForVehicleByMonth monthMileage in monthlyMileageData)
                {
                    if (monthMileage.Cost != default)
                    {
                        monthMileage.Cost = 100 / monthMileage.Cost;
                    }
                }
            }
            var mpgViewModel = new MPGForVehicleByMonth
            {
                CostData = monthlyMileageData,
                Unit = invertedFuelMileageUnit ? preferredFuelMileageUnit : fuelEconomyMileageUnit,
                SortedCostData = (userConfig.UseMPG || invertedFuelMileageUnit) ? monthlyMileageData.OrderByDescending(x => x.Cost).ToList() : monthlyMileageData.OrderBy(x => x.Cost).ToList()
            };
            return PartialView("_MPGByMonthReport", mpgViewModel);
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
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        public IActionResult GetCostByMonthAndYearByVehicle(int vehicleId, List<ImportMode> selectedMetrics, int year = 0)
        {
            List<CostForVehicleByMonth> allCosts = StaticHelper.GetBaseLineCosts();
            if (selectedMetrics.Contains(ImportMode.ServiceRecord))
            {
                var serviceRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
                allCosts.AddRange(_reportHelper.GetServiceRecordSum(serviceRecords, year, true));
            }
            if (selectedMetrics.Contains(ImportMode.RepairRecord))
            {
                var repairRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
                allCosts.AddRange(_reportHelper.GetRepairRecordSum(repairRecords, year, true));
            }
            if (selectedMetrics.Contains(ImportMode.UpgradeRecord))
            {
                var upgradeRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
                allCosts.AddRange(_reportHelper.GetUpgradeRecordSum(upgradeRecords, year, true));
            }
            if (selectedMetrics.Contains(ImportMode.GasRecord))
            {
                var gasRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
                allCosts.AddRange(_reportHelper.GetGasRecordSum(gasRecords, year, true));
            }
            if (selectedMetrics.Contains(ImportMode.TaxRecord))
            {
                var taxRecords = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
                allCosts.AddRange(_reportHelper.GetTaxRecordSum(taxRecords, year, true));
            }
            if (selectedMetrics.Contains(ImportMode.OdometerRecord))
            {
                var odometerRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
                allCosts.AddRange(_reportHelper.GetOdometerRecordSum(odometerRecords, year, true));
            }
            var groupedRecord = allCosts.GroupBy(x => new { x.MonthName, x.MonthId, x.Year }).OrderByDescending(x=>x.Key.Year).Select(x => new CostForVehicleByMonth
            {
                Year = x.Key.Year,
                MonthName = x.Key.MonthName,
                Cost = x.Sum(y => y.Cost),
                DistanceTraveled = x.Max(y => y.DistanceTraveled),
                MonthId = x.Key.MonthId
            }).ToList();
            var vehicleData = _dataAccess.GetVehicleById(vehicleId);
            var userConfig = _config.GetUserConfig(User);
            var viewModel = new CostDistanceTableForVehicle { CostData = groupedRecord };
            viewModel.DistanceUnit = vehicleData.UseHours ? "h" : userConfig.UseMPG ? "mi." : "km";
            return PartialView("_CostDistanceTableReport", viewModel);
        }
        [HttpGet]
        public IActionResult GetAdditionalWidgets()
        {
            var widgets = _fileHelper.GetWidgets();
            return PartialView("_ReportWidgets", widgets);
        }
    }
}

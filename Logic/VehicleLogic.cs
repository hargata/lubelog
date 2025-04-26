using CarCareTracker.Controllers;
using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;

namespace CarCareTracker.Logic
{
    public interface IVehicleLogic
    {
        VehicleRecords GetVehicleRecords(int vehicleId);
        decimal GetVehicleTotalCost(VehicleRecords vehicleRecords);
        int GetMaxMileage(int vehicleId);
        int GetMaxMileage(VehicleRecords vehicleRecords);
        int GetMinMileage(int vehicleId);
        int GetMinMileage(VehicleRecords vehicleRecords);
        int GetOwnershipDays(string purchaseDate, string soldDate, int year, List<ServiceRecord> serviceRecords, List<CollisionRecord> repairRecords, List<GasRecord> gasRecords, List<UpgradeRecord> upgradeRecords, List<OdometerRecord> odometerRecords, List<TaxRecord> taxRecords);
        bool GetVehicleHasUrgentOrPastDueReminders(int vehicleId, int currentMileage);
        List<VehicleInfo> GetVehicleInfo(List<Vehicle> vehicles);
        List<ReminderRecordViewModel> GetReminders(List<Vehicle> vehicles, bool isCalendar);
        List<PlanRecord> GetPlans(List<Vehicle> vehicles, bool excludeDone);
        bool UpdateRecurringTaxes(int vehicleId);
        void RestoreSupplyRecordsByUsage(List<SupplyUsageHistory> supplyUsage, string usageDescription);
    }
    public class VehicleLogic: IVehicleLogic
    {
        private readonly IServiceRecordDataAccess _serviceRecordDataAccess;
        private readonly IGasRecordDataAccess _gasRecordDataAccess;
        private readonly ICollisionRecordDataAccess _collisionRecordDataAccess;
        private readonly IUpgradeRecordDataAccess _upgradeRecordDataAccess;
        private readonly ITaxRecordDataAccess _taxRecordDataAccess;
        private readonly IOdometerRecordDataAccess _odometerRecordDataAccess;
        private readonly IReminderRecordDataAccess _reminderRecordDataAccess;
        private readonly IPlanRecordDataAccess _planRecordDataAccess;
        private readonly IReminderHelper _reminderHelper;
        private readonly IVehicleDataAccess _dataAccess;
        private readonly ISupplyRecordDataAccess _supplyRecordDataAccess;
        private readonly ILogger<VehicleLogic> _logger;

        public VehicleLogic(
            IServiceRecordDataAccess serviceRecordDataAccess,
            IGasRecordDataAccess gasRecordDataAccess,
            ICollisionRecordDataAccess collisionRecordDataAccess,
            IUpgradeRecordDataAccess upgradeRecordDataAccess,
            ITaxRecordDataAccess taxRecordDataAccess,
            IOdometerRecordDataAccess odometerRecordDataAccess,
            IReminderRecordDataAccess reminderRecordDataAccess,
            IPlanRecordDataAccess planRecordDataAccess,
            IReminderHelper reminderHelper,
            IVehicleDataAccess dataAccess,
            ISupplyRecordDataAccess supplyRecordDataAccess,
            ILogger<VehicleLogic> logger
            ) {
            _serviceRecordDataAccess = serviceRecordDataAccess;
            _gasRecordDataAccess = gasRecordDataAccess;
            _collisionRecordDataAccess = collisionRecordDataAccess;
            _upgradeRecordDataAccess = upgradeRecordDataAccess;
            _taxRecordDataAccess = taxRecordDataAccess;
            _odometerRecordDataAccess = odometerRecordDataAccess;
            _planRecordDataAccess = planRecordDataAccess;
            _reminderRecordDataAccess = reminderRecordDataAccess;
            _reminderHelper = reminderHelper;
            _dataAccess = dataAccess;
            _supplyRecordDataAccess = supplyRecordDataAccess;
            _logger = logger;
        }
        public VehicleRecords GetVehicleRecords(int vehicleId)
        {
            return new VehicleRecords
            {
                ServiceRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId),
                GasRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId),
                CollisionRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId),
                TaxRecords = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId),
                UpgradeRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId),
                OdometerRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId),
            };
        }
        public decimal GetVehicleTotalCost(VehicleRecords vehicleRecords)
        {
            var serviceRecordSum = vehicleRecords.ServiceRecords.Sum(x => x.Cost);
            var repairRecordSum = vehicleRecords.CollisionRecords.Sum(x => x.Cost);
            var upgradeRecordSum = vehicleRecords.UpgradeRecords.Sum(x => x.Cost);
            var taxRecordSum = vehicleRecords.TaxRecords.Sum(x => x.Cost);
            var gasRecordSum = vehicleRecords.GasRecords.Sum(x => x.Cost);
            return serviceRecordSum + repairRecordSum + upgradeRecordSum + taxRecordSum + gasRecordSum;
        }
        public int GetMaxMileage(int vehicleId)
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
        public int GetMaxMileage(VehicleRecords vehicleRecords)
        {
            var numbersArray = new List<int>();
            if (vehicleRecords.ServiceRecords.Any())
            {
                numbersArray.Add(vehicleRecords.ServiceRecords.Max(x => x.Mileage));
            }
            if (vehicleRecords.CollisionRecords.Any())
            {
                numbersArray.Add(vehicleRecords.CollisionRecords.Max(x => x.Mileage));
            }
            if (vehicleRecords.GasRecords.Any())
            {
                numbersArray.Add(vehicleRecords.GasRecords.Max(x => x.Mileage));
            }
            if (vehicleRecords.UpgradeRecords.Any())
            {
                numbersArray.Add(vehicleRecords.UpgradeRecords.Max(x => x.Mileage));
            }
            if (vehicleRecords.OdometerRecords.Any())
            {
                numbersArray.Add(vehicleRecords.OdometerRecords.Max(x => x.Mileage));
            }
            return numbersArray.Any() ? numbersArray.Max() : 0;
        }
        public int GetMinMileage(int vehicleId)
        {
            var numbersArray = new List<int>();
            var serviceRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId).Where(x => x.Mileage != default);
            if (serviceRecords.Any())
            {
                numbersArray.Add(serviceRecords.Min(x => x.Mileage));
            }
            var repairRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId).Where(x => x.Mileage != default);
            if (repairRecords.Any())
            {
                numbersArray.Add(repairRecords.Min(x => x.Mileage));
            }
            var gasRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId).Where(x => x.Mileage != default);
            if (gasRecords.Any())
            {
                numbersArray.Add(gasRecords.Min(x => x.Mileage));
            }
            var upgradeRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId).Where(x => x.Mileage != default);
            if (upgradeRecords.Any())
            {
                numbersArray.Add(upgradeRecords.Min(x => x.Mileage));
            }
            var odometerRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId).Where(x => x.Mileage != default);
            if (odometerRecords.Any())
            {
                numbersArray.Add(odometerRecords.Min(x => x.Mileage));
            }
            return numbersArray.Any() ? numbersArray.Min() : 0;
        }
        public int GetMinMileage(VehicleRecords vehicleRecords)
        {
            var numbersArray = new List<int>();
            var _serviceRecords = vehicleRecords.ServiceRecords.Where(x => x.Mileage != default).ToList();
            if (_serviceRecords.Any())
            {
                numbersArray.Add(_serviceRecords.Min(x => x.Mileage));
            }
            var _repairRecords = vehicleRecords.CollisionRecords.Where(x => x.Mileage != default).ToList();
            if (_repairRecords.Any())
            {
                numbersArray.Add(_repairRecords.Min(x => x.Mileage));
            }
            var _gasRecords = vehicleRecords.GasRecords.Where(x => x.Mileage != default).ToList();
            if (_gasRecords.Any())
            {
                numbersArray.Add(_gasRecords.Min(x => x.Mileage));
            }
            var _upgradeRecords = vehicleRecords.UpgradeRecords.Where(x => x.Mileage != default).ToList();
            if (_upgradeRecords.Any())
            {
                numbersArray.Add(_upgradeRecords.Min(x => x.Mileage));
            }
            var _odometerRecords = vehicleRecords.OdometerRecords.Where(x => x.Mileage != default).ToList();
            if (_odometerRecords.Any())
            {
                numbersArray.Add(_odometerRecords.Min(x => x.Mileage));
            }
            return numbersArray.Any() ? numbersArray.Min() : 0;
        }
        public int GetOwnershipDays(string purchaseDate, string soldDate, int year, List<ServiceRecord> serviceRecords, List<CollisionRecord> repairRecords, List<GasRecord> gasRecords, List<UpgradeRecord> upgradeRecords, List<OdometerRecord> odometerRecords, List<TaxRecord> taxRecords)
        {
            var startDate = DateTime.Now;
            var endDate = DateTime.Now;
            bool usePurchaseDate = false;
            bool useSoldDate = false;
            if (!string.IsNullOrWhiteSpace(soldDate) && DateTime.TryParse(soldDate, out DateTime vehicleSoldDate))
            {
                if (year == default || year >= vehicleSoldDate.Year) //All Time is selected or the selected year is greater or equal to the year the vehicle is sold
                {
                    endDate = vehicleSoldDate; //cap end date to vehicle sold date.
                    useSoldDate = true;
                }
            }
            if (!string.IsNullOrWhiteSpace(purchaseDate) && DateTime.TryParse(purchaseDate, out DateTime vehiclePurchaseDate))
            {
                if (year == default || year <= vehiclePurchaseDate.Year) //All Time is selected or the selected year is less or equal to the year the vehicle is purchased
                {
                    startDate = vehiclePurchaseDate; //cap start date to vehicle purchase date
                    usePurchaseDate = true;
                }
            }
            if (year != default)
            {
                var calendarYearStart = new DateTime(year, 1, 1);
                var calendarYearEnd = new DateTime(year + 1, 1, 1);
                if (!useSoldDate)
                {
                    endDate = endDate > calendarYearEnd ? calendarYearEnd : endDate;
                }
                if (!usePurchaseDate)
                {
                    startDate = startDate > calendarYearStart ? calendarYearStart : startDate;
                }
                var timeElapsed = (int)Math.Floor((endDate - startDate).TotalDays);
                return timeElapsed;
            }
            var dateArray = new List<DateTime>() { startDate };
            dateArray.AddRange(serviceRecords.Select(x => x.Date));
            dateArray.AddRange(repairRecords.Select(x => x.Date));
            dateArray.AddRange(gasRecords.Select(x => x.Date));
            dateArray.AddRange(upgradeRecords.Select(x => x.Date));
            dateArray.AddRange(odometerRecords.Select(x => x.Date));
            dateArray.AddRange(taxRecords.Select(x => x.Date));
            if (dateArray.Any())
            {
                startDate = dateArray.Min();
                var timeElapsed = (int)Math.Floor((endDate - startDate).TotalDays);
                return timeElapsed;
            } else
            {
                return 1;
            }
        }
        public bool GetVehicleHasUrgentOrPastDueReminders(int vehicleId, int currentMileage)
        {
            var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
            var results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, DateTime.Now);
            return results.Any(x => x.Urgency == ReminderUrgency.VeryUrgent || x.Urgency == ReminderUrgency.PastDue);
        }

        public List<VehicleInfo> GetVehicleInfo(List<Vehicle> vehicles)
        {
            List<VehicleInfo> apiResult = new List<VehicleInfo>();

            foreach (Vehicle vehicle in vehicles)
            {
                var currentMileage = GetMaxMileage(vehicle.Id);
                var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicle.Id);
                var results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, DateTime.Now);

                var serviceRecords = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicle.Id);
                var repairRecords = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicle.Id);
                var upgradeRecords = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicle.Id);
                var gasRecords = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicle.Id);
                var taxRecords = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicle.Id);
                var planRecords = _planRecordDataAccess.GetPlanRecordsByVehicleId(vehicle.Id);

                var resultToAdd = new VehicleInfo()
                {
                    VehicleData = vehicle,
                    LastReportedOdometer = currentMileage,
                    ServiceRecordCount = serviceRecords.Count(),
                    ServiceRecordCost = serviceRecords.Sum(x => x.Cost),
                    RepairRecordCount = repairRecords.Count(),
                    RepairRecordCost = repairRecords.Sum(x => x.Cost),
                    UpgradeRecordCount = upgradeRecords.Count(),
                    UpgradeRecordCost = upgradeRecords.Sum(x => x.Cost),
                    GasRecordCount = gasRecords.Count(),
                    GasRecordCost = gasRecords.Sum(x => x.Cost),
                    TaxRecordCount = taxRecords.Count(),
                    TaxRecordCost = taxRecords.Sum(x => x.Cost),
                    VeryUrgentReminderCount = results.Count(x => x.Urgency == ReminderUrgency.VeryUrgent),
                    PastDueReminderCount = results.Count(x => x.Urgency == ReminderUrgency.PastDue),
                    UrgentReminderCount = results.Count(x => x.Urgency == ReminderUrgency.Urgent),
                    NotUrgentReminderCount = results.Count(x => x.Urgency == ReminderUrgency.NotUrgent),
                    PlanRecordBackLogCount = planRecords.Count(x => x.Progress == PlanProgress.Backlog),
                    PlanRecordInProgressCount = planRecords.Count(x => x.Progress == PlanProgress.InProgress),
                    PlanRecordTestingCount = planRecords.Count(x => x.Progress == PlanProgress.Testing),
                    PlanRecordDoneCount = planRecords.Count(x => x.Progress == PlanProgress.Done)
                };
                //set next reminder
                if (results.Any(x => (x.Metric == ReminderMetric.Date || x.Metric == ReminderMetric.Both) && x.Date >= DateTime.Now.Date))
                {
                    resultToAdd.NextReminder = results.Where(x => x.Date >= DateTime.Now.Date).OrderBy(x => x.Date).Select(x => new ReminderExportModel { Id = x.Id.ToString(), Description = x.Description, Urgency = x.Urgency.ToString(), Metric = x.Metric.ToString(), Notes = x.Notes, DueDate = x.Date.ToShortDateString(), DueOdometer = x.Mileage.ToString(), Tags = string.Join(' ', x.Tags) }).First();
                }
                else if (results.Any(x => (x.Metric == ReminderMetric.Odometer || x.Metric == ReminderMetric.Both) && x.Mileage >= currentMileage))
                {
                    resultToAdd.NextReminder = results.Where(x => x.Mileage >= currentMileage).OrderBy(x => x.Mileage).Select(x => new ReminderExportModel { Id = x.Id.ToString(), Description = x.Description, Urgency = x.Urgency.ToString(), Metric = x.Metric.ToString(), Notes = x.Notes, DueDate = x.Date.ToShortDateString(), DueOdometer = x.Mileage.ToString(), Tags = string.Join(' ', x.Tags) }).First();
                }
                apiResult.Add(resultToAdd);
            }
            return apiResult;
        }
        public List<ReminderRecordViewModel> GetReminders(List<Vehicle> vehicles, bool isCalendar)
        {
            List<ReminderRecordViewModel> reminders = new List<ReminderRecordViewModel>();
            foreach (Vehicle vehicle in vehicles)
            {
                var vehicleReminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicle.Id);
                if (isCalendar)
                {
                    vehicleReminders.RemoveAll(x => x.Metric == ReminderMetric.Odometer);
                    //we don't care about mileages so we can basically fake the current vehicle mileage.
                }
                if (vehicleReminders.Any())
                {
                    var vehicleMileage = isCalendar ? 0 : GetMaxMileage(vehicle.Id);
                    var reminderUrgency = _reminderHelper.GetReminderRecordViewModels(vehicleReminders, vehicleMileage, DateTime.Now);
                    reminderUrgency = reminderUrgency.Select(x => new ReminderRecordViewModel { Id = x.Id, Metric = x.Metric, Date = x.Date, Notes = x.Notes, Mileage = x.Mileage, Urgency = x.Urgency, Description = $"{vehicle.Year} {vehicle.Make} {vehicle.Model} #{StaticHelper.GetVehicleIdentifier(vehicle)} - {x.Description}" }).ToList();
                    reminders.AddRange(reminderUrgency);
                }
            }
            return reminders.OrderByDescending(x=>x.Urgency).ToList();
        }
        public List<PlanRecord> GetPlans(List<Vehicle> vehicles, bool excludeDone)
        {
            List<PlanRecord> plans = new List<PlanRecord>();
            foreach (Vehicle vehicle in vehicles)
            {
                var vehiclePlans = _planRecordDataAccess.GetPlanRecordsByVehicleId(vehicle.Id);
                if (excludeDone)
                {
                    vehiclePlans.RemoveAll(x => x.Progress == PlanProgress.Done);
                }
                if (vehiclePlans.Any())
                {
                    var convertedPlans = vehiclePlans.Select(x => new PlanRecord { ImportMode = x.ImportMode, Priority = x.Priority, Progress = x.Progress, Notes = x.Notes, RequisitionHistory = x.RequisitionHistory, Description = $"{vehicle.Year} {vehicle.Make} {vehicle.Model} #{StaticHelper.GetVehicleIdentifier(vehicle)} - {x.Description}" });
                    plans.AddRange(convertedPlans);
                }
            }
            return plans.OrderBy(x => x.Priority).ThenBy(x=>x.Progress).ToList();
        }
        public bool UpdateRecurringTaxes(int vehicleId)
        {
            var vehicleData = _dataAccess.GetVehicleById(vehicleId);
            if (!string.IsNullOrWhiteSpace(vehicleData.SoldDate))
            {
                return false;
            }
            bool RecurringTaxIsOutdated(TaxRecord taxRecord)
            {
                var monthInterval = taxRecord.RecurringInterval != ReminderMonthInterval.Other ? (int)taxRecord.RecurringInterval : taxRecord.CustomMonthInterval;
                bool addDays = taxRecord.RecurringInterval == ReminderMonthInterval.Other && taxRecord.CustomMonthIntervalUnit == ReminderIntervalUnit.Days;
                return addDays ? DateTime.Now > taxRecord.Date.AddDays(monthInterval) : DateTime.Now > taxRecord.Date.AddMonths(monthInterval);
            }
            var result = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
            var outdatedRecurringFees = result.Where(x => x.IsRecurring && RecurringTaxIsOutdated(x));
            if (outdatedRecurringFees.Any())
            {
                var success = false;
                foreach (TaxRecord recurringFee in outdatedRecurringFees)
                {
                    var monthInterval = recurringFee.RecurringInterval != ReminderMonthInterval.Other ? (int)recurringFee.RecurringInterval : recurringFee.CustomMonthInterval;
                    bool isOutdated = true;
                    bool addDays = recurringFee.RecurringInterval == ReminderMonthInterval.Other && recurringFee.CustomMonthIntervalUnit == ReminderIntervalUnit.Days;
                    //update the original outdated tax record
                    recurringFee.IsRecurring = false;
                    _taxRecordDataAccess.SaveTaxRecordToVehicle(recurringFee);
                    //month multiplier for severely outdated monthly tax records.
                    int monthMultiplier = 1;
                    var originalDate = recurringFee.Date;
                    while (isOutdated)
                    {
                        try
                        {
                            var nextDate = addDays ? originalDate.AddDays(monthInterval * monthMultiplier) : originalDate.AddMonths(monthInterval * monthMultiplier);
                            monthMultiplier++;
                            var nextnextDate = addDays ? originalDate.AddDays(monthInterval * monthMultiplier) : originalDate.AddMonths(monthInterval * monthMultiplier);
                            recurringFee.Date = nextDate;
                            recurringFee.Id = default; //new record
                            recurringFee.IsRecurring = DateTime.Now <= nextnextDate;
                            _taxRecordDataAccess.SaveTaxRecordToVehicle(recurringFee);
                            isOutdated = !recurringFee.IsRecurring;
                            success = true;
                        }
                        catch (Exception)
                        {
                            isOutdated = false; //break out of loop if something broke.
                            success = false;
                        }
                    }
                }
                return success;
            }
            return false; //no outdated recurring tax records.
        }
        public void RestoreSupplyRecordsByUsage(List<SupplyUsageHistory> supplyUsage, string usageDescription)
        {
            foreach (SupplyUsageHistory supply in supplyUsage)
            {
                try
                {
                    if (supply.Id == default)
                    {
                        continue; //no id, skip current supply.
                    }
                    var result = _supplyRecordDataAccess.GetSupplyRecordById(supply.Id);
                    if (result != null && result.Id != default)
                    {
                        //supply exists, re-add the quantity and cost
                        result.Quantity += supply.Quantity;
                        result.Cost += supply.Cost;
                        var requisitionRecord = new SupplyUsageHistory
                        {
                            Id = supply.Id,
                            Date = DateTime.Now.Date,
                            Description = $"Restored from {usageDescription}",
                            Quantity = supply.Quantity,
                            Cost = supply.Cost
                        };
                        result.RequisitionHistory.Add(requisitionRecord);
                        //save
                        _supplyRecordDataAccess.SaveSupplyRecordToVehicle(result);
                    }
                    else
                    {
                        _logger.LogError($"Unable to find supply with id {supply.Id}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error restoring supply with id {supply.Id} : {ex.Message}");
                }
            }
        }
    }
}

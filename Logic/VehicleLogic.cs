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
        int GetOwnershipDays(string purchaseDate, string soldDate, List<ServiceRecord> serviceRecords, List<CollisionRecord> repairRecords, List<GasRecord> gasRecords, List<UpgradeRecord> upgradeRecords, List<OdometerRecord> odometerRecords, List<TaxRecord> taxRecords);
        bool GetVehicleHasUrgentOrPastDueReminders(int vehicleId, int currentMileage);
        List<VehicleInfo> GetVehicleInfo(List<Vehicle> vehicles);
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
        public VehicleLogic(
            IServiceRecordDataAccess serviceRecordDataAccess,
            IGasRecordDataAccess gasRecordDataAccess,
            ICollisionRecordDataAccess collisionRecordDataAccess,
            IUpgradeRecordDataAccess upgradeRecordDataAccess,
            ITaxRecordDataAccess taxRecordDataAccess,
            IOdometerRecordDataAccess odometerRecordDataAccess,
            IReminderRecordDataAccess reminderRecordDataAccess,
            IPlanRecordDataAccess planRecordDataAccess,
            IReminderHelper reminderHelper
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
        public int GetOwnershipDays(string purchaseDate, string soldDate, List<ServiceRecord> serviceRecords, List<CollisionRecord> repairRecords, List<GasRecord> gasRecords, List<UpgradeRecord> upgradeRecords, List<OdometerRecord> odometerRecords, List<TaxRecord> taxRecords)
        {
            var startDate = DateTime.Now;
            var endDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(soldDate))
            {
                endDate = DateTime.Parse(soldDate);
            }
            if (!string.IsNullOrWhiteSpace(purchaseDate))
            {
                //if purchase date is provided, then we just have to subtract the begin date to end date and return number of months
                startDate = DateTime.Parse(purchaseDate);
                var timeElapsed = (int)Math.Floor((endDate - startDate).TotalDays);
                return timeElapsed;
            }
            var dateArray = new List<DateTime>();
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
                    resultToAdd.NextReminder = results.Where(x => x.Date >= DateTime.Now.Date).OrderBy(x => x.Date).Select(x => new ReminderExportModel { Description = x.Description, Urgency = x.Urgency.ToString(), Metric = x.Metric.ToString(), Notes = x.Notes, DueDate = x.Date.ToShortDateString(), DueOdometer = x.Mileage.ToString() }).First();
                }
                else if (results.Any(x => (x.Metric == ReminderMetric.Odometer || x.Metric == ReminderMetric.Both) && x.Mileage >= currentMileage))
                {
                    resultToAdd.NextReminder = results.Where(x => x.Mileage >= currentMileage).OrderBy(x => x.Mileage).Select(x => new ReminderExportModel { Description = x.Description, Urgency = x.Urgency.ToString(), Metric = x.Metric.ToString(), Notes = x.Notes, DueDate = x.Date.ToShortDateString(), DueOdometer = x.Mileage.ToString() }).First();
                }
                apiResult.Add(resultToAdd);
            }
            return apiResult;
        }
    }
}

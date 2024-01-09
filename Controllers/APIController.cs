using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public class APIController : Controller
    {
        private readonly IVehicleDataAccess _dataAccess;
        private readonly INoteDataAccess _noteDataAccess;
        private readonly IServiceRecordDataAccess _serviceRecordDataAccess;
        private readonly IGasRecordDataAccess _gasRecordDataAccess;
        private readonly ICollisionRecordDataAccess _collisionRecordDataAccess;
        private readonly ITaxRecordDataAccess _taxRecordDataAccess;
        private readonly IReminderRecordDataAccess _reminderRecordDataAccess;
        private readonly IUpgradeRecordDataAccess _upgradeRecordDataAccess;
        public APIController(IVehicleDataAccess dataAccess,
            INoteDataAccess noteDataAccess,
            IServiceRecordDataAccess serviceRecordDataAccess,
            IGasRecordDataAccess gasRecordDataAccess,
            ICollisionRecordDataAccess collisionRecordDataAccess,
            ITaxRecordDataAccess taxRecordDataAccess,
            IReminderRecordDataAccess reminderRecordDataAccess,
            IUpgradeRecordDataAccess upgradeRecordDataAccess) 
        {
            _dataAccess = dataAccess;
            _noteDataAccess = noteDataAccess;
            _serviceRecordDataAccess = serviceRecordDataAccess;
            _gasRecordDataAccess = gasRecordDataAccess;
            _collisionRecordDataAccess = collisionRecordDataAccess;
            _taxRecordDataAccess = taxRecordDataAccess;
            _reminderRecordDataAccess = reminderRecordDataAccess;
            _upgradeRecordDataAccess = upgradeRecordDataAccess;
        }
        [HttpGet]
        public IActionResult Vehicles()
        {
            var result = _dataAccess.GetVehicles();
            return Json(result);
        }
        [HttpGet]
        public IActionResult ServiceRecords(int vehicleId)
        {
            var result = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
            return Json(result);
        }
        [HttpGet]
        public IActionResult RepairRecords(int vehicleId)
        {
            var result = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
            return Json(result);
        }
        [HttpGet]
        public IActionResult UpgradeRecords(int vehicleId)
        {
            var result = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
            return Json(result);
        }
        [HttpGet]
        public IActionResult TaxRecords(int vehicleId)
        {
            var result = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
            return Json(result);
        }
        [HttpGet]
        public IActionResult Reminders(int vehicleId)
        {
            var result = GetRemindersAndUrgency(vehicleId);
            return Json(result);
        }
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
    }
}

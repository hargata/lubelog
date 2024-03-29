﻿using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;

namespace CarCareTracker.Logic
{
    public interface IVehicleLogic
    {
        int GetMaxMileage(int vehicleId);
        int GetMinMileage(int vehicleId);
        bool GetVehicleHasUrgentOrPastDueReminders(int vehicleId);
    }
    public class VehicleLogic: IVehicleLogic
    {
        private readonly IServiceRecordDataAccess _serviceRecordDataAccess;
        private readonly IGasRecordDataAccess _gasRecordDataAccess;
        private readonly ICollisionRecordDataAccess _collisionRecordDataAccess;
        private readonly IUpgradeRecordDataAccess _upgradeRecordDataAccess;
        private readonly IOdometerRecordDataAccess _odometerRecordDataAccess;
        private readonly IReminderRecordDataAccess _reminderRecordDataAccess;
        private readonly IReminderHelper _reminderHelper;
        public VehicleLogic(
            IServiceRecordDataAccess serviceRecordDataAccess,
            IGasRecordDataAccess gasRecordDataAccess,
            ICollisionRecordDataAccess collisionRecordDataAccess,
            IUpgradeRecordDataAccess upgradeRecordDataAccess,
            IOdometerRecordDataAccess odometerRecordDataAccess,
            IReminderRecordDataAccess reminderRecordDataAccess,
            IReminderHelper reminderHelper
            ) {
            _serviceRecordDataAccess = serviceRecordDataAccess;
            _gasRecordDataAccess = gasRecordDataAccess;
            _collisionRecordDataAccess = collisionRecordDataAccess;
            _upgradeRecordDataAccess = upgradeRecordDataAccess;
            _odometerRecordDataAccess = odometerRecordDataAccess;
            _reminderRecordDataAccess = reminderRecordDataAccess;
            _reminderHelper = reminderHelper;
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
        public bool GetVehicleHasUrgentOrPastDueReminders(int vehicleId)
        {
            var currentMileage = GetMaxMileage(vehicleId);
            var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
            var results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, DateTime.Now);
            return results.Any(x => x.Urgency == ReminderUrgency.VeryUrgent || x.Urgency == ReminderUrgency.PastDue);
        }
    }
}

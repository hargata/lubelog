using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IUpgradeRecordDataAccess
    {
        public List<UpgradeRecord> GetUpgradeRecordsByVehicleId(int vehicleId);
        public UpgradeRecord GetUpgradeRecordById(int upgradeRecordId);
        public bool DeleteUpgradeRecordById(int upgradeRecordId);
        public bool SaveUpgradeRecordToVehicle(UpgradeRecord upgradeRecord);
        public bool DeleteAllUpgradeRecordsByVehicleId(int vehicleId);
    }
}

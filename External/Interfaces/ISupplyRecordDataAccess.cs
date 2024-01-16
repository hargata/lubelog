using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface ISupplyRecordDataAccess
    {
        public List<SupplyRecord> GetSupplyRecordsByVehicleId(int vehicleId);
        public SupplyRecord GetSupplyRecordById(int supplyRecordId);
        public bool DeleteSupplyRecordById(int supplyRecordId);
        public bool SaveSupplyRecordToVehicle(SupplyRecord supplyRecord);
        public bool DeleteAllSupplyRecordsByVehicleId(int vehicleId);
    }
}

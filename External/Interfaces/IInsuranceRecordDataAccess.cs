using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IInsuranceRecordDataAccess
    {
        public List<InsuranceRecord> GetInsuranceRecordsByVehicleId(int vehicleId);
        public InsuranceRecord GetInsuranceRecordById(int insuranceRecordId);
        public bool DeleteInsuranceRecordById(int insuranceRecordId);
        public bool SaveInsuranceRecordToVehicle(InsuranceRecord insuranceRecord);
        public bool DeleteAllInsuranceRecordsByVehicleId(int vehicleId);
    }
}

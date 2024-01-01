using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IServiceRecordDataAccess
    {
        public List<ServiceRecord> GetServiceRecordsByVehicleId(int vehicleId);
        public ServiceRecord GetServiceRecordById(int serviceRecordId);
        public bool DeleteServiceRecordById(int serviceRecordId);
        public bool SaveServiceRecordToVehicle(ServiceRecord serviceRecord);
    }
}

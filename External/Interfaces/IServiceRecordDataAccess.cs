using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IServiceRecordDataAccess
    {
        public List<ServiceRecord> GetServiceRecordsByVehicleId(int vehicleId);
        public bool SaveServiceRecordToVehicle(ServiceRecord serviceRecord);
    }
}

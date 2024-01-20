using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IOdometerRecordDataAccess
    {
        public List<OdometerRecord> GetOdometerRecordsByVehicleId(int vehicleId);
        public OdometerRecord GetOdometerRecordById(int odometerRecordId);
        public bool DeleteOdometerRecordById(int odometerRecordId);
        public bool SaveOdometerRecordToVehicle(OdometerRecord odometerRecord);
        public bool DeleteAllOdometerRecordsByVehicleId(int vehicleId);
    }
}

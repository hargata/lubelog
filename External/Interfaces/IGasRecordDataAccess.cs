using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IGasRecordDataAccess
    {
        public List<GasRecord> GetGasRecordsByVehicleId(int vehicleId);
        public List<GasRecord> GetGasRecordsByVehicleIdPaginated(int vehicleId, int from, int to);
        public int GetGasRecordsByVehicleIdCount(int vehicleId);
        public GasRecord GetGasRecordById(int gasRecordId);
        public bool DeleteGasRecordById(int gasRecordId);
        public bool SaveGasRecordToVehicle(GasRecord gasRecord);
        public bool DeleteAllGasRecordsByVehicleId(int vehicleId);
    }
}

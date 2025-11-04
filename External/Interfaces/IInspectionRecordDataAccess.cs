using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IInspectionRecordDataAccess
    {
        public List<InspectionRecord> GetInspectionRecordsByVehicleId(int vehicleId);
        public InspectionRecord GetInspectionRecordById(int inspectionRecordId);
        public bool DeleteInspectionRecordById(int inspectionRecordId);
        public bool SaveInspectionRecordToVehicle(InspectionRecord inspectionRecordId);
        public bool DeleteAllInspectionRecordsByVehicleId(int vehicleId);
    }
}

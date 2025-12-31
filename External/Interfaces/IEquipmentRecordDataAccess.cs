using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IEquipmentRecordDataAccess
    {
        public List<EquipmentRecord> GetEquipmentRecordsByVehicleId(int vehicleId);
        public EquipmentRecord GetEquipmentRecordById(int serviceRecordId);
        public bool DeleteEquipmentRecordById(int equipmentRecordId);
        public bool SaveEquipmentRecordToVehicle(EquipmentRecord serviceRecord);
        public bool DeleteAllEquipmentRecordsByVehicleId(int vehicleId);
    }
}

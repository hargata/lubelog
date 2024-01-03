using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface ICollisionRecordDataAccess
    {
        public List<CollisionRecord> GetCollisionRecordsByVehicleId(int vehicleId);
        public CollisionRecord GetCollisionRecordById(int serviceRecordId);
        public bool DeleteCollisionRecordById(int serviceRecordId);
        public bool SaveCollisionRecordToVehicle(CollisionRecord serviceRecord);
        public bool DeleteAllCollisionRecordsByVehicleId(int vehicleId);
    }
}

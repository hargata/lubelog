using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IPlanRecordDataAccess
    {
        public List<PlanRecord> GetPlanRecordsByVehicleId(int vehicleId);
        public PlanRecord GetPlanRecordById(int planRecordId);
        public bool DeletePlanRecordById(int planRecordId);
        public bool SavePlanRecordToVehicle(PlanRecord planRecord);
        public bool DeleteAllPlanRecordsByVehicleId(int vehicleId);
    }
}

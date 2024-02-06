using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IPlanRecordTemplateDataAccess
    {
        public List<PlanRecordInput> GetPlanRecordTemplatesByVehicleId(int vehicleId);
        public PlanRecordInput GetPlanRecordTemplateById(int planRecordId);
        public bool DeletePlanRecordTemplateById(int planRecordId);
        public bool SavePlanRecordTemplateToVehicle(PlanRecordInput planRecord);
        public bool DeleteAllPlanRecordTemplatesByVehicleId(int vehicleId);
    }
}

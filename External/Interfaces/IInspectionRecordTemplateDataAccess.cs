using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IInspectionRecordTemplateDataAccess
    {
        public List<InspectionRecordInput> GetInspectionRecordTemplatesByVehicleId(int vehicleId);
        public InspectionRecordInput GetInspectionRecordTemplateById(int inspectionRecordTemplateId);
        public bool DeleteInspectionRecordTemplateById(int inspectionRecordTemplateId);
        public bool SaveInspectionReportTemplateToVehicle(InspectionRecordInput inspectionRecordTemplate);
        public bool DeleteAllInspectionReportTemplatesByVehicleId(int vehicleId);
    }
}

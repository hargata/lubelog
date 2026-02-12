namespace CarCareTracker.Models
{
    public class ReportViewModel
    {
        public ReportHeader ReportHeaderForVehicle { get; set; } = new ReportHeader();
        public List<CostForVehicleByMonth> CostForVehicleByMonth { get; set; } = new List<CostForVehicleByMonth>();
        public MPGForVehicleByMonth FuelMileageForVehicleByMonth { get; set; } = new MPGForVehicleByMonth();
        public CostMakeUpForVehicle CostMakeUpForVehicle { get; set; } = new CostMakeUpForVehicle();
        public ReminderMakeUpForVehicle ReminderMakeUpForVehicle { get; set; } = new ReminderMakeUpForVehicle();
        public List<int> Years { get; set; } = new List<int>();
        public VehicleCollaboratorViewModel Collaborators { get; set; } = new VehicleCollaboratorViewModel();
        public bool CustomWidgetsConfigured { get; set; } = false;
        public List<ImportMode> AvailableMetrics { get; set; } = new List<ImportMode>();
        public bool HasVehicleImageMap { get; set; } = false;
    }
}

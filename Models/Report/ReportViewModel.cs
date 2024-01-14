namespace CarCareTracker.Models
{
    public class ReportViewModel
    {
        public List<CostForVehicleByMonth> CostForVehicleByMonth { get; set; } = new List<CostForVehicleByMonth>();
        public CostMakeUpForVehicle CostMakeUpForVehicle { get; set; } = new CostMakeUpForVehicle();
        public ReminderMakeUpForVehicle ReminderMakeUpForVehicle { get; set; } = new ReminderMakeUpForVehicle();
        public List<int> Years { get; set; } = new List<int>();
        public List<UserCollaborator> Collaborators { get; set; } = new List<UserCollaborator>();
    }
}

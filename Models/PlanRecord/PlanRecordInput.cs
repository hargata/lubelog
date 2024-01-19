using CarCareTracker.Enum;

namespace CarCareTracker.Models
{
    public class PlanRecordInput
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string DateCreated { get; set; } = DateTime.Now.ToShortDateString();
        public string DateModified { get; set; } = DateTime.Now.ToShortDateString();
        public string Description { get; set; }
        public string Notes { get; set; }
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public ImportMode ImportMode { get; set; }
        public PlanPriority Priority { get; set; }
        public PlanProgress Progress { get; set; }
        public List<PlanCostItem> Costs { get; set; } = new List<PlanCostItem>();
        public PlanRecord ToPlanRecord() { return new PlanRecord { 
            Id = Id, 
            VehicleId = VehicleId, 
            DateCreated = DateTime.Parse(DateCreated), 
            DateModified = DateTime.Parse(DateModified),
            Description = Description, 
            Notes = Notes, 
            Files = Files,
            ImportMode = ImportMode,
            Costs = Costs,
            Priority = Priority,
            Progress = Progress
        }; }
    }
}

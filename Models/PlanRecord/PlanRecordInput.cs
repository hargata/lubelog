namespace CarCareTracker.Models
{
    public class PlanRecordInput
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int ReminderRecordId { get; set; }
        public string DateCreated { get; set; } = DateTime.Now.ToShortDateString();
        public string DateModified { get; set; } = DateTime.Now.ToShortDateString();
        public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<SupplyUsage> Supplies { get; set; } = new List<SupplyUsage>();
        public ImportMode ImportMode { get; set; }
        public PlanPriority Priority { get; set; }
        public PlanProgress Progress { get; set; }
        public decimal Cost { get; set; }
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<SupplyUsageHistory> RequisitionHistory { get; set; } = new List<SupplyUsageHistory>();
        public List<SupplyUsageHistory> DeletedRequisitionHistory { get; set; } = new List<SupplyUsageHistory>();
        public bool CopySuppliesAttachment { get; set; } = false;
        public PlanRecord ToPlanRecord() { return new PlanRecord { 
            Id = Id, 
            VehicleId = VehicleId, 
            ReminderRecordId = ReminderRecordId,
            DateCreated = DateTime.Parse(DateCreated), 
            DateModified = DateTime.Parse(DateModified),
            Description = Description, 
            Notes = Notes, 
            Files = Files,
            ImportMode = ImportMode,
            Cost = Cost,
            Priority = Priority,
            Progress = Progress,
            ExtraFields = ExtraFields,
            RequisitionHistory = RequisitionHistory
        }; }
        /// <summary>
        /// only used to hide view template button on plan create modal.
        /// </summary>
        public bool CreatedFromReminder { get; set; }
    }
}

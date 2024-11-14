namespace CarCareTracker.Models
{
    public class VehicleInfo
    {
        public Vehicle VehicleData { get; set; } = new Vehicle();
        public int VeryUrgentReminderCount { get; set; }
        public int UrgentReminderCount { get; set;}
        public int NotUrgentReminderCount { get; set; }
        public int PastDueReminderCount { get; set; }
        public ReminderExportModel NextReminder { get; set; }
        public int ServiceRecordCount { get; set; }
        public decimal ServiceRecordCost { get; set; }
        public int RepairRecordCount { get; set; }
        public decimal RepairRecordCost { get; set; }
        public int UpgradeRecordCount { get; set; }
        public decimal UpgradeRecordCost { get; set; }
        public int TaxRecordCount { get; set; }
        public decimal TaxRecordCost { get; set; }
        public int GasRecordCount { get; set; }
        public decimal GasRecordCost { get; set; }
        public int LastReportedOdometer { get; set; }
        public int PlanRecordBackLogCount { get; set; }
        public int PlanRecordInProgressCount { get; set; }
        public int PlanRecordTestingCount { get; set; }
        public int PlanRecordDoneCount { get; set; }
    }
}

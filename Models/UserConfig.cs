namespace CarCareTracker.Models
{
    public class UserConfig
    {
        public bool UseDarkMode { get; set; }
        public bool EnableCsvImports { get; set; }
        public bool UseMPG { get; set; }
        public bool UseDescending { get; set; }
        public bool EnableAuth { get; set; }
        public bool HideZero { get; set; }
        public bool UseUKMPG {get;set;}
        public bool UseThreeDecimalGasCost { get; set; }
        public string UserNameHash { get; set; }
        public string UserPasswordHash { get; set;}
        public List<ImportMode> VisibleTabs { get; set; } = new List<ImportMode>() { 
            ImportMode.Dashboard,
            ImportMode.ServiceRecord, 
            ImportMode.RepairRecord, 
            ImportMode.GasRecord, 
            ImportMode.UpgradeRecord, 
            ImportMode.TaxRecord, 
            ImportMode.ReminderRecord, 
            ImportMode.NoteRecord};
        public ImportMode DefaultTab { get; set; } = ImportMode.Dashboard;
    }
}
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
    }
}
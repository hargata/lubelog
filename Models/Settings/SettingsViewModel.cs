namespace CarCareTracker.Models
{
    public class SettingsViewModel
    {
        public UserConfig UserConfig { get; set; }
        public List<string> UILanguages { get; set; }
        public Sponsors Sponsors { get; set; } = new Sponsors();
    }
}

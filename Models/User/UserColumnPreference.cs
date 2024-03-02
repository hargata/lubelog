namespace CarCareTracker.Models
{
    public class UserColumnPreference
    {
        public ImportMode Tab { get; set; }
        public List<string> VisibleColumns { get; set; } = new List<string>();
    }
}
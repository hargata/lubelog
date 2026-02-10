namespace CarCareTracker.Models
{
    public class APIKey
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public List<HouseholdPermission> Permissions { get; set; } = new List<HouseholdPermission>();
    }
}
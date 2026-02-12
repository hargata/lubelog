using System.Text.Json.Serialization;

namespace CarCareTracker.Models
{
    public class MethodParameter
    {
        public int Id { get; set; }
        [JsonConverter(typeof(FromDateOptional))]
        public string StartDate { get; set; } = string.Empty;
        [JsonConverter(typeof(FromDateOptional))]
        public string EndDate { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public bool UseMPG { get; set; }
        public bool UseUKMPG { get; set; }
    }
    public class ReminderMethodParameter
    {
        public int Id { get; set; }
        public List<ReminderUrgency> Urgencies { get; set; } = new List<ReminderUrgency> { ReminderUrgency.NotUrgent, ReminderUrgency.Urgent, ReminderUrgency.VeryUrgent, ReminderUrgency.PastDue };
        public string Tags { get; set; } = string.Empty;
    }
}

namespace CarCareTracker.Models
{
    public class NotificationConfig
    {
        public int HourToCheck { get; set; }
        public int MinuteToCheck { get; set; }
        public List<ReminderUrgency> UrgenciesTracked { get; set; } = new List<ReminderUrgency>();
        public int DaysToCache { get; set; }
        public List<AutomatedEvent> AutomatedEvents { get; set; } = new List<AutomatedEvent>();
        public List<NotificationServiceConfig> ServiceConfigs { get; set; } = new List<NotificationServiceConfig>();
    }
    public class NotificationServiceConfig
    {
        public string Url { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> PriorityMapping { get; set; } = new Dictionary<string, string>();
        public object? Body { get; set;  }
    }
}
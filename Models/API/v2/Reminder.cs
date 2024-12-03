namespace CarCareTracker.Models.API.v2
{
    public class ReminderApiModel
    {
        public string Description { get; set; }
        public string Urgency { get; set; }
        public string Metric { get; set; }
        public string Notes { get; set; }
        public string DueDate { get; set; }
        public int DueOdometer { get; set; }
    }
}

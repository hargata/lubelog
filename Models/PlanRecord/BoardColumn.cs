using System.Text.Json.Serialization;

namespace CarCareTracker.Models
{
    public class BoardColumn
    {
        public string Name { get; set; } = string.Empty;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PlanProgress Progress { get; set; }
        public bool Visible { get; set; }
        public int Order { get; set; }
    }
}

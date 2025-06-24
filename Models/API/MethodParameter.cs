using System.Text.Json.Serialization;

namespace CarCareTracker.Models
{
    public class MethodParameter
    {
        public int VehicleId { get; set; }
        public int Id { get; set; }
        [JsonConverter(typeof(FromDateOptional))]
        public string StartDate { get; set; }
        [JsonConverter(typeof(FromDateOptional))]
        public string EndDate { get; set; }
        public string Tags { get; set; }
        public bool UseMPG { get; set; }
        public bool UseUKMPG { get; set; }
    }
}

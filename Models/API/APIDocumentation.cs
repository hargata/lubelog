using System.Text.Json.Serialization;

namespace CarCareTracker.Models
{
    public class APIDocumentation
    {
        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;
        [JsonPropertyName("methods")]
        public List<APIMethod> Methods { get; set; } = new List<APIMethod>();
    }
    public class APIMethod
    {
        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("methodType")]
        public APIMethodType MethodType { get; set; }
        [JsonPropertyName("queryParams")]
        public List<APIQueryParam> QueryParams { get; set; } = new List<APIQueryParam>();
        [JsonPropertyName("hasBody")]
        public bool HasBody { get; set; }
        [JsonPropertyName("bodySample")]
        public object BodySample { get; set; } = new object();
        public string BodySampleString { get; set; } = string.Empty;
        [JsonPropertyName("bodyParamName")]
        public string BodyParamName { get; set; } = string.Empty;
        [JsonPropertyName("bodyIsFileUpload")]
        public bool BodyIsFileUpload { get; set; } = false;
    }
    public class APIQueryParam
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("required")]
        public bool Required { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}

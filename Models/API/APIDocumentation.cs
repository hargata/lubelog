using System.Text.Json.Serialization;

namespace CarCareTracker.Models
{
    public class APIDocumentation
    {
        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; }
        [JsonPropertyName("methods")]
        public List<APIMethod> Methods { get; set; } = new List<APIMethod>();
    }
    public class APIMethod
    {
        [JsonPropertyName("path")]
        public string Path { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("methodType")]
        public APIMethodType MethodType { get; set; }
        [JsonPropertyName("queryParams")]
        public List<APIQueryParam> QueryParams { get; set; } = new List<APIQueryParam>();
        [JsonPropertyName("hasBody")]
        public bool HasBody { get; set; }
        [JsonPropertyName("bodySample")]
        public object BodySample { get; set; } = new object();
        public string BodySampleString { get; set; }
        [JsonPropertyName("bodyParamName")]
        public string BodyParamName { get; set; }
        [JsonPropertyName("bodyIsFileUpload")]
        public bool BodyIsFileUpload { get; set; } = false;
    }
    public class APIQueryParam
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("required")]
        public bool Required { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
    }
}

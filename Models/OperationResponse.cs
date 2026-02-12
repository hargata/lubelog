using CarCareTracker.Helper;
using System.Text.Json.Serialization;

namespace CarCareTracker.Models
{
    public class OperationResponseBase
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? AdditionalData { get; set; }
    }
    public class OperationResponse: OperationResponseBase
    {
        public static OperationResponse Succeed(string message = "")
        {
            return new OperationResponse { Success = true, Message = message};
        }
        public static OperationResponse Succeed(string message, object additionalData)
        {
            return new OperationResponse { Success = true, Message = message, AdditionalData = additionalData };
        }
        public static OperationResponse Failed(string message = "")
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                message = StaticHelper.GenericErrorMessage;
            }
            return new OperationResponse { Success = false, Message = message};
        }
        public static OperationResponse Conditional(bool result, string successMessage = "", string errorMessage = "")
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                errorMessage = StaticHelper.GenericErrorMessage;
            }
            return new OperationResponse { Success = result, Message = result ? successMessage : errorMessage };
        }
    }    
}

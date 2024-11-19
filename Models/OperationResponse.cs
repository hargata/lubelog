using CarCareTracker.Helper;

namespace CarCareTracker.Models
{
    public class OperationResponseBase
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
    public class OperationResponse: OperationResponseBase
    {
        public static OperationResponse Succeed(string message)
        {
            return new OperationResponse { Success = true, Message = message };
        }
        public static OperationResponse Failed(string message = "")
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                message = StaticHelper.GenericErrorMessage;
            }
            return new OperationResponse { Success = false, Message = message};
        }
    }    
}

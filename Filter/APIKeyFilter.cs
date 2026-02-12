using CarCareTracker.Logic;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CarCareTracker.Filter
{
    public class APIKeyFilter: ActionFilterAttribute
    {
        private readonly IUserLogic _userLogic;
        private readonly HouseholdPermission _permission;
        public APIKeyFilter(IUserLogic userLogic, HouseholdPermission? permission = HouseholdPermission.View) {
            _userLogic = userLogic;
            _permission = permission ?? HouseholdPermission.View;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.IsInRole("APIKeyAuth"))
            {
                var apikey_header = filterContext.HttpContext.Request.Headers["x-api-key"];
                if (string.IsNullOrWhiteSpace(apikey_header))
                {
                    apikey_header = filterContext.HttpContext.Request.Query["apiKey"];
                }
                if (!string.IsNullOrWhiteSpace(apikey_header))
                {
                    var permissions = _userLogic.GetAPIKeyPermissions(apikey_header);
                    if (!permissions.Contains(_permission))
                    {
                        filterContext.Result = new JsonResult(OperationResponse.Failed("Access Denied"));
                        filterContext.HttpContext.Response.StatusCode = 401;
                    }
                }
            }
        }
    }
}

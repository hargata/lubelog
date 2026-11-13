using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CarCareTracker.Filter
{
    /// <summary>
    /// Refuses requests authenticated with an external identity provider's bearer JWT (APIAuthJWT) with 403.
    /// Those short-lived tokens are for everyday vehicle data only; put this on anything that manages the server,
    /// credentials, sessions or users, or exports the whole database (backups).
    /// </summary>
    public class DenyJWTFilter : ActionFilterAttribute
    {
        public const string JWTAuthRole = "JWTAuth";
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.IsInRole(JWTAuthRole))
            {
                filterContext.Result = new JsonResult(OperationResponse.Failed("Not available with an identity provider token"));
                filterContext.HttpContext.Response.StatusCode = 403;
            }
        }
    }
}

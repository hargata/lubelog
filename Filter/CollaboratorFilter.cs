using CarCareTracker.Logic;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace CarCareTracker.Filter
{
    public class CollaboratorFilter: ActionFilterAttribute
    {
        private readonly IUserLogic _userLogic;
        public CollaboratorFilter(IUserLogic userLogic) {
            _userLogic = userLogic;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.User.IsInRole(nameof(UserData.IsRootUser)))
            {
                var vehicleId = int.Parse(filterContext.ActionArguments["vehicleId"].ToString());
                if (vehicleId != default)
                {
                    var userId = int.Parse(filterContext.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                    if (!_userLogic.UserCanEditVehicle(userId, vehicleId))
                    {
                        filterContext.Result = new RedirectResult("/Error/Unauthorized");
                    }
                }
            }
        }
    }
}

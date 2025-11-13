using CarCareTracker.Helper;
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
        private readonly IConfigHelper _config;
        public CollaboratorFilter(IUserLogic userLogic, IConfigHelper config) {
            _userLogic = userLogic;
            _config = config;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.User.IsInRole(nameof(UserData.IsRootUser)))
            {
                if (filterContext.ActionArguments.ContainsKey("vehicleId"))
                {
                    var vehicleId = int.Parse(filterContext.ActionArguments["vehicleId"].ToString());
                    if (vehicleId != default)
                    {
                        var userId = int.Parse(filterContext.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                        if (!_userLogic.UserCanEditVehicle(userId, vehicleId, HouseholdPermission.View))
                        {
                            filterContext.Result = new RedirectResult("/Error/Unauthorized");
                        }
                    }
                    else
                    {
                        var shopSupplyEndpoints = new List<string> { "ImportToVehicleIdFromCsv", "GetSupplyRecordsByVehicleId", "ExportFromVehicleToCsv" };
                        if (shopSupplyEndpoints.Contains(filterContext.RouteData.Values["action"].ToString()) && !_config.GetServerEnableShopSupplies())
                        {
                            //user trying to access shop supplies but shop supplies is not enabled by root user.
                            filterContext.Result = new RedirectResult("/Error/Unauthorized");
                        }
                        else if (!shopSupplyEndpoints.Contains(filterContext.RouteData.Values["action"].ToString()))
                        {
                            //user trying to access any other endpoints using 0 as vehicle id.
                            filterContext.Result = new RedirectResult("/Error/Unauthorized");
                        }
                    }
                } else
                {
                    filterContext.Result = new RedirectResult("/Error/Unauthorized");
                }
            }
        }
    }
}

using CarCareTracker.Helper;
using CarCareTracker.Logic;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace CarCareTracker.Filter
{
    public class StrictCollaboratorFilter: ActionFilterAttribute
    {
        private readonly IUserLogic _userLogic;
        private readonly IConfigHelper _config;
        private readonly bool _multiple;
        private readonly bool _jsonResponse;
        public StrictCollaboratorFilter(IUserLogic userLogic, IConfigHelper config, bool? multiple = false, bool? jsonResponse = false) {
            _userLogic = userLogic;
            _config = config;
            _multiple = multiple ?? false;
            _jsonResponse = jsonResponse ?? false;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.User.IsInRole(nameof(UserData.IsRootUser)))
            {
                List<int> vehicleIds = new List<int>();
                if (!_multiple && filterContext.ActionArguments.ContainsKey("vehicleId"))
                {
                    vehicleIds.Add(int.Parse(filterContext.ActionArguments["vehicleId"].ToString()));
                }
                else if (_multiple && filterContext.ActionArguments.ContainsKey("vehicleIds"))
                {
                    vehicleIds.AddRange(filterContext.ActionArguments["vehicleIds"] as List<int>);
                }

                if (vehicleIds.Any())
                {
                    foreach (int vehicleId in vehicleIds)
                    {
                        if (vehicleId != default)
                        {
                            var userId = int.Parse(filterContext.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
                            if (!_userLogic.UserCanDirectlyEditVehicle(userId, vehicleId))
                            {
                                filterContext.Result = _jsonResponse ? new JsonResult(OperationResponse.Failed("Access Denied")) : new RedirectResult("/Error/Unauthorized");
                            }
                        }
                        else
                        {
                            if (StaticHelper.IsShopSupplyEndpoint(filterContext.RouteData.Values["action"].ToString()) && !_config.GetServerEnableShopSupplies())
                            {
                                //user trying to access shop supplies but shop supplies is not enabled by root user.
                                filterContext.Result = _jsonResponse ? new JsonResult(OperationResponse.Failed("Access Denied")) : new RedirectResult("/Error/Unauthorized");
                            }
                            else if (!StaticHelper.IsShopSupplyEndpoint(filterContext.RouteData.Values["action"].ToString()))
                            {
                                //user trying to access any other endpoints using 0 as vehicle id.
                                filterContext.Result = _jsonResponse ? new JsonResult(OperationResponse.Failed("Access Denied")) : new RedirectResult("/Error/Unauthorized");
                            }
                        }
                    }
                }
                else
                {
                    filterContext.Result = _jsonResponse ? new JsonResult(OperationResponse.Failed("Access Denied")) : new RedirectResult("/Error/Unauthorized");
                }
            }
        }
    }
}
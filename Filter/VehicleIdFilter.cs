using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
using System.Text.Json;

namespace CarCareTracker.Filter
{
    public class VehicleIdFilter: ActionFilterAttribute
    {
        private readonly string[] _queryParams;
        public VehicleIdFilter(string[] queryParams)
        {
            _queryParams = queryParams;
        }
        public override async void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (_queryParams.Any(x => !filterContext.ActionArguments.ContainsKey(x)))
            {
                filterContext.HttpContext.Request.Body.Position = 0;
                var reader = new StreamReader(filterContext.HttpContext.Request.Body, Encoding.UTF8);
                var rawMessage = await reader.ReadToEndAsync();
                Dictionary<string, object> dynamicDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(rawMessage) ?? new Dictionary<string, object>();
                foreach(string queryParam in _queryParams)
                {
                    if (!filterContext.ActionArguments.ContainsKey(queryParam) && dynamicDictionary.ContainsKey(queryParam))
                    {
                        filterContext.ActionArguments.Add(queryParam, dynamicDictionary[queryParam].ToString());
                    }
                }
            }
        }
    }
}
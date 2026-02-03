using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
using System.Text.Json;

namespace CarCareTracker.Filter
{
    public class QueryParamFilter: ActionFilterAttribute
    {
        private readonly string[] _queryParams;
        public QueryParamFilter(string[] queryParams)
        {
            _queryParams = queryParams;
        }
        public override async void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Dictionary<string, string> paramDictionary = new Dictionary<string, string>{
                { "vehicleId", "int"},
                { "autoIncludeEquipment", "bool" }
            };
            if (_queryParams.Any(x => !filterContext.ActionArguments.ContainsKey(x)))
            {
                filterContext.HttpContext.Request.Body.Position = 0;
                var reader = new StreamReader(filterContext.HttpContext.Request.Body, Encoding.UTF8);
                var rawMessage = await reader.ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(rawMessage))
                {
                    Dictionary<string, object> dynamicDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(rawMessage) ?? new Dictionary<string, object>();
                    foreach (string queryParam in _queryParams)
                    {
                        if (!filterContext.ActionArguments.ContainsKey(queryParam) && dynamicDictionary.ContainsKey(queryParam))
                        {
                            if (paramDictionary.TryGetValue(queryParam, out string queryParamType))
                            {
                                if (queryParamType == "int")
                                {
                                    filterContext.ActionArguments.Add(queryParam, int.Parse(dynamicDictionary[queryParam].ToString()));
                                }
                                else if (queryParamType == "bool")
                                {
                                    filterContext.ActionArguments.Add(queryParam, bool.Parse(dynamicDictionary[queryParam].ToString()));
                                }
                                else
                                {
                                    filterContext.ActionArguments.Add(queryParam, dynamicDictionary[queryParam].ToString());
                                }
                            }
                            else
                            {
                                filterContext.ActionArguments.Add(queryParam, dynamicDictionary[queryParam].ToString());
                            }
                        }
                    }
                }
            }
        }
    }
}
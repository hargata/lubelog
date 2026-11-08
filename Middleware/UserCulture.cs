using CarCareTracker.Helper;
using System.Collections.Concurrent;
using System.Globalization;

namespace CarCareTracker.Middleware
{
    /// <summary>
    /// Applies the user's preferred region format and decimal separator to the current request.
    /// API requests keep the server locale so existing integrations are not affected.
    /// </summary>
    public class UserCulture
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, CultureInfo> _cultureCache = new();
        public UserCulture(RequestDelegate next) =>
            _next = next;

        public async Task InvokeAsync(HttpContext context, IConfigHelper config)
        {
            if (!context.Request.Path.StartsWithSegments("/api") && context.User.Identity?.IsAuthenticated == true)
            {
                var userConfig = config.GetUserConfig(context.User);
                var culture = GetCulture(userConfig.UserLocale, userConfig.UserDecimalSeparator);
                if (culture != null)
                {
                    CultureInfo.CurrentCulture = culture;
                }
            }
            await _next(context);
        }
        public static CultureInfo? GetCulture(string locale, string decimalSeparator)
        {
            if (string.IsNullOrWhiteSpace(locale) && string.IsNullOrWhiteSpace(decimalSeparator))
            {
                return null;
            }
            return _cultureCache.GetOrAdd($"{locale}|{decimalSeparator}", _ =>
            {
                CultureInfo culture;
                try
                {
                    culture = string.IsNullOrWhiteSpace(locale) ? (CultureInfo)CultureInfo.CurrentCulture.Clone() : new CultureInfo(locale);
                }
                catch (CultureNotFoundException)
                {
                    culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
                }
                if (decimalSeparator == "." || decimalSeparator == ",")
                {
                    var groupSeparator = decimalSeparator == "." ? "," : ".";
                    var numberFormat = culture.NumberFormat;
                    numberFormat.NumberDecimalSeparator = decimalSeparator;
                    numberFormat.CurrencyDecimalSeparator = decimalSeparator;
                    numberFormat.PercentDecimalSeparator = decimalSeparator;
                    if (numberFormat.NumberGroupSeparator == decimalSeparator)
                    {
                        numberFormat.NumberGroupSeparator = groupSeparator;
                    }
                    if (numberFormat.CurrencyGroupSeparator == decimalSeparator)
                    {
                        numberFormat.CurrencyGroupSeparator = groupSeparator;
                    }
                    if (numberFormat.PercentGroupSeparator == decimalSeparator)
                    {
                        numberFormat.PercentGroupSeparator = groupSeparator;
                    }
                }
                return CultureInfo.ReadOnly(culture);
            });
        }
    }
}

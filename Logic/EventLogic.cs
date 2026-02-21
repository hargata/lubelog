using CarCareTracker.Helper;
using CarCareTracker.Models;

namespace CarCareTracker.Logic
{
    public interface IEventLogic
    {
        void PublishEvent(WebHookPayload webHookPayload);
    }
    public class EventLogic: IEventLogic
    {
        private readonly IConfigHelper _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EventLogic> _logger;
        private bool doDelay = false;
        private int delaySeconds = 2;
        private int maxDelay = 60;
        public EventLogic(IConfigHelper config, IHttpClientFactory httpClientFactory, ILogger<EventLogic> logger)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public async void PublishEvent(WebHookPayload webHookPayload)
        {
            string webhookURL = _config.GetWebHookUrl();
            int maxRetries = 5;
            if (doDelay)
            {
                _logger.LogWarning($"WebHook: Delaying for {delaySeconds} seconds before attempt");
                await Task.Delay(delaySeconds * 1000);
            }
            if (!string.IsNullOrWhiteSpace(webhookURL))
            {
                var _httpClient = _httpClientFactory.CreateClient();
                HttpResponseMessage result;
                int attempt = 1;
                bool succeed = false;
                int iterationDelay = 2;
                while (!succeed && attempt <= maxRetries)
                {
                    if (webhookURL.StartsWith("discord://"))
                    {
                        var cleanWebhookURL = webhookURL.Replace("discord://", "https://");
                        result = await _httpClient.PostAsJsonAsync(cleanWebhookURL, DiscordWebHook.FromWebHookPayload(webHookPayload));
                    }
                    else
                    {
                        result = await _httpClient.PostAsJsonAsync(webhookURL, webHookPayload);
                    }
                    if (result.IsSuccessStatusCode)
                    {
                        succeed = true;
                        doDelay = false;
                        delaySeconds = 2; //succeeded, reset delay back to 2
                    }
                    else if (result.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        doDelay = true;
                        //attempt to get retry-after from header
                        if (result.Headers.RetryAfter != null)
                        {
                            if (result.Headers.RetryAfter.Delta.HasValue)
                            {
                                iterationDelay = (int)result.Headers.RetryAfter.Delta.Value.TotalSeconds;
                            }
                        }
                        else
                        {
                            iterationDelay += (attempt * 2);
                            if (iterationDelay > maxDelay)
                            {
                                iterationDelay = maxDelay;
                            }
                        }
                        _logger.LogWarning($"WebHook: Too Many Requests, Delaying for {iterationDelay} seconds before attempt {attempt}/{maxRetries}");
                        await Task.Delay(iterationDelay * 1000);
                    } else
                    {
                        doDelay = true;
                        _logger.LogWarning($"WebHook Error: {result.StatusCode} Attempt {attempt}");
                    }
                    attempt++;
                }
                if (!succeed)
                {
                    _logger.LogWarning($"WebHook Error: Exhausted All Attempts");
                    delaySeconds += 2; //push back future webhook attempts;
                    if (delaySeconds > maxDelay)
                    {
                        delaySeconds = maxDelay; //cap max delay to 60 seconds.
                    }
                }
            }
        }
    }
}
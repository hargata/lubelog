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
        private List<int> delayDict = new List<int>() { 2, 4, 16, 32, 60 };
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
            if (!string.IsNullOrWhiteSpace(webhookURL))
            {
                var _httpClient = _httpClientFactory.CreateClient();
                HttpResponseMessage result;
                int attempt = 1;
                bool succeed = false;
                while (!succeed && attempt <= maxRetries)
                {
                    int iterationDelay = delayDict[attempt - 1];
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
                    }
                    else if (result.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        int randomModifier = new Random().Next(1, 20);
                        //attempt to get retry-after from header
                        if (result.Headers.RetryAfter != null)
                        {
                            if (result.Headers.RetryAfter.Delta.HasValue)
                            {
                                iterationDelay += (int)Math.Ceiling(result.Headers.RetryAfter.Delta.Value.TotalSeconds);
                            }
                        }
                        iterationDelay += randomModifier;
                        _logger.LogWarning($"WebHook: Too Many Requests, Delaying for {iterationDelay} seconds before attempt {attempt}/{maxRetries}");
                        await Task.Delay(iterationDelay * 1000);
                    } else
                    {
                        _logger.LogWarning($"WebHook Error: {result.StatusCode} Attempt {attempt}");
                    }
                    attempt++;
                }
                if (!succeed)
                {
                    _logger.LogWarning($"WebHook Error: Exhausted All Attempts");
                }
            }
        }
    }
}
using CarCareTracker.Helper;
using CarCareTracker.Models;

namespace CarCareTracker.Webhooks;

public class WebhookPublisher : IWebhookPublisher
{
    private readonly IConfigHelper _config;
    private readonly HttpClient _client;
    private readonly ILogger<WebhookPublisher> _logger;

    public WebhookPublisher(IConfigHelper config, ILogger<WebhookPublisher> logger)
    {
        _config = config;
        _logger = logger;
        _client = new HttpClient();
    }

    public async Task PublishAsync(WebHookPayloadBase payload, CancellationToken ct = default)
    {
        this._logger.LogInformation("Publishing webhook payload");

        await _client.PostAsJsonAsync(_config.GetWebHookUrl(), payload, ct);
    }
}
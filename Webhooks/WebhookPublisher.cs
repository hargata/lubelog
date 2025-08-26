using CarCareTracker.Helper;
using CarCareTracker.Models;

namespace CarCareTracker.Webhooks;

public class WebhookPublisher : IWebhookPublisher
{
    private readonly IConfigHelper _config;
    private readonly HttpClient _client;

    public WebhookPublisher(IConfigHelper config)
    {
        _config = config;
        _client = new HttpClient();
    }

    public async Task PublishAsync(WebHookPayloadBase payload, CancellationToken ct = default)
    {
        await _client.PostAsJsonAsync(_config.GetWebHookUrl(), payload, ct);
    }
}
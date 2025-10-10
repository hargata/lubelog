using CarCareTracker.Models;

namespace CarCareTracker.Webhooks;

public interface IWebhookPublisher
{
    Task PublishAsync(WebHookPayloadBase payload, CancellationToken ct = default);
}
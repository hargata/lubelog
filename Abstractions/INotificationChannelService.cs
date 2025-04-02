using CarCareTracker.Models;

namespace CarCareTracker.Abstractions;

public interface INotificationChannelService
{
    Task WriteAsync(WebHookPayload payload, CancellationToken ct = default);

    IAsyncEnumerable<WebHookPayload> ReadAllAsync(CancellationToken ct = default);
}
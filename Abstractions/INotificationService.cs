using CarCareTracker.Models;

namespace CarCareTracker.Abstractions;

public interface INotificationSenderService
{
    Task SendNotificationAsync(WebHookPayload webHookPayload, CancellationToken cancellationToken = default);
}
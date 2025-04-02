using CarCareTracker.Models;

namespace CarCareTracker.Abstractions;

public interface INotificationService
{
    Task NotifyAsync(WebHookPayload webHookPayload);
}
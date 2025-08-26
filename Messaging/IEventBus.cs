namespace CarCareTracker.Messaging;

public interface IEventBus
{
    Task Publish(object @event, CancellationToken ct = default);

    void Publish(object @event);

    IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler);
}
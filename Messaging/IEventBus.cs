namespace CarCareTracker.Messaging;

public interface IEventBus
{
    Task Publish(object @event, CancellationToken ct = default);

    void Publish(object @event);

    void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler);
}
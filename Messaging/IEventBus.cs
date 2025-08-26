namespace CarCareTracker.Messaging;

public interface IEventBus
{
    Task Publish<TEvent>(TEvent @event, CancellationToken ct = default);
    void Publish<TEvent>(TEvent @event);
    void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler);
}
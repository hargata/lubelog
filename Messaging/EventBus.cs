using System.Threading.Channels;

namespace CarCareTracker.Messaging;

// Assumption: Subscriptions are registered synchronously during startup (Program.cs)
// before any Publish is called. No runtime subscriptions occur.
public sealed class EventBus : IEventBus
{
    // Queue holds executable work items
    private readonly Channel<Func<CancellationToken, Task>> _queue;
    private readonly bool _throwOnSyncFull;

    // Handlers by exact event type; only written during startup
    private readonly Dictionary<Type, List<Func<object, CancellationToken, Task>>> _eventHandlers = new();

    internal ChannelReader<Func<CancellationToken, Task>> Reader => _queue.Reader;

    public EventBus(EventBusOptions? options = null)
    {
        var o = options ?? new EventBusOptions();
        _throwOnSyncFull = o.ThrowOnSyncPublishWhenFull;

        _queue = o.BoundedCapacity is int cap
            ? Channel.CreateBounded<Func<CancellationToken, Task>>(new BoundedChannelOptions(cap)
            {
                SingleReader = o.NumReaders == 1,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            })
            : Channel.CreateUnbounded<Func<CancellationToken, Task>>(new UnboundedChannelOptions
            {
                SingleReader = o.NumReaders == 1,
                SingleWriter = false
            });
    }

    public async Task Publish(object @event, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var type = @event.GetType();

        if (!_eventHandlers.TryGetValue(type, out var handlers))
        {
            // No-one has registered to listen for this event type, can return early
            return;
        }

        // one work item per handler; await to respect backpressure
        foreach (var h in handlers)
        {
            var h1 = h;
            Func<CancellationToken, Task> work = token => h1(@event, token);
            await _queue.Writer.WriteAsync(work, ct);
        }
    }

    public void Publish(object @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var type = @event.GetType();
        if (!_eventHandlers.TryGetValue(type, out var handlers))
        {
            // No-one has registered to listen for this event type, can return early
            return;
        }

        var anyFailed = false;
        foreach (var h in handlers)
        {
            var h1 = h;
            Func<CancellationToken, Task> work = token => h1(@event, token);
            if (!_queue.Writer.TryWrite(work)) anyFailed = true;
        }

        if (anyFailed && _throwOnSyncFull)
        {
            throw new InvalidOperationException("Event queue is full; use async Publish to respect backpressure.");
        }
    }

    // Called only during startup registration
    public void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));
        Task Wrapper(object e, CancellationToken ct) => handler((TEvent)e, ct);

        var key = typeof(TEvent);
        if (!_eventHandlers.TryGetValue(key, out var list))
        {
            list = new List<Func<object, CancellationToken, Task>>(1);
            _eventHandlers[key] = list;
        }
        list.Add(Wrapper);
    }
}

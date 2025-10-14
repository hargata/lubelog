using System.Collections.Immutable;
using System.Threading.Channels;

namespace CarCareTracker.Messaging;

// Assumption: Subscriptions are registered synchronously during startup (Program.cs)
// before any Publish is called. No runtime subscriptions occur.
public sealed class EventBus : IEventBus
{
    // tiny, allocation-lean unit of work
    internal readonly struct WorkItem
    {
        private readonly object _evt;
        private readonly Func<object, CancellationToken, Task> _handler;

        public WorkItem(object evt, Func<object, CancellationToken, Task> handler)
        {
            _evt = evt;
            _handler = handler;
        }

        public Task Run(CancellationToken ct) => _handler(_evt, ct);
    }

    private readonly Channel<WorkItem> _queue;
    private readonly bool _throwOnSyncFull;

    // handlers by exact event type; only mutated during startup
    private readonly Dictionary<Type, ImmutableArray<Func<object, CancellationToken, Task>>> _eventHandlers = new();

    // expose reader so background workers can drain and execute
    internal ChannelReader<WorkItem> Reader => _queue.Reader;

    public EventBus(EventBusOptions? options = null)
    {
        var o = options ?? new EventBusOptions();
        _throwOnSyncFull = o.ThrowOnSyncPublishWhenFull;

        _queue = o.BoundedCapacity is int cap
            ? Channel.CreateBounded<WorkItem>(new BoundedChannelOptions(cap)
            {
                SingleReader = o.NumReaders == 1,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            })
            : Channel.CreateUnbounded<WorkItem>(new UnboundedChannelOptions
            {
                SingleReader = o.NumReaders == 1,
                SingleWriter = false
            });
    }

    public Task Publish<TEvent>(TEvent @event, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var key = typeof(TEvent);
        if (!_eventHandlers.TryGetValue(key, out var handlers))
        {
            // No ones subscribed to this event, can return early
            return Task.CompletedTask;
        }

        return EnqueueAll(@event, handlers, ct);
    }

    public void Publish<TEvent>(TEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var key = typeof(TEvent);
        if (!_eventHandlers.TryGetValue(key, out var handlers))
        {
            // No ones subscribed to this event, can return early
            return;
        }

        var anyFailed = false;
        foreach (var h in handlers)
        {
            var work = new WorkItem(@event, h);
            if (!_queue.Writer.TryWrite(work)) anyFailed = true;
        }

        if (anyFailed && _throwOnSyncFull)
        {
            throw new InvalidOperationException("Event queue is full; use async Publish to respect backpressure.");
        }
    }

    public void Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));

        // wrap once at registration time
        Task Wrapper(object e, CancellationToken ct) => handler((TEvent)e, ct);

        var key = typeof(TEvent);
        if (!_eventHandlers.TryGetValue(key, out var list))
        {
            list = ImmutableArray<Func<object, CancellationToken, Task>>.Empty;
        }

        list = list.Add(Wrapper);
        _eventHandlers[key] = list;
    }

    private async Task EnqueueAll(object evt, ImmutableArray<Func<object, CancellationToken, Task>> handlers, CancellationToken ct)
    {
        foreach (var h in handlers)
        {
            var work = new WorkItem(evt, h);
            await _queue.Writer.WriteAsync(work, ct);
        }
    }
}
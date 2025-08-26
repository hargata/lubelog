using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading.Channels;

namespace CarCareTracker.Messaging;

public sealed class EventBus : IEventBus
{
    private readonly Channel<object> _channel;
    private readonly EventBusOptions _options;

    // Exact-type subscriptions; simple and fast
    private readonly Dictionary<Type, List<Func<object, CancellationToken, Task>>> _handlers = new();
    private readonly object _gate = new();

    internal ChannelReader<object> Reader => _channel.Reader;

    public EventBus(EventBusOptions? options = null)
    {
        _options = options ?? new EventBusOptions();

        _channel = _options.BoundedCapacity is int cap
            ? Channel.CreateBounded<object>(new BoundedChannelOptions(cap)
              {
                  SingleReader = _options.SingleReader,
                  SingleWriter = false,
                  FullMode = BoundedChannelFullMode.Wait
              })
            : Channel.CreateUnbounded<object>(new UnboundedChannelOptions
              {
                  SingleReader = _options.SingleReader,
                  SingleWriter = false
              });
    }

    public Task Publish(object @event, CancellationToken ct = default)
    {
        if (@event is null) throw new ArgumentNullException(nameof(@event));
        return _channel.Writer.WriteAsync(@event, ct).AsTask();
    }

    public void Publish(object @event)
    {
        if (@event is null) throw new ArgumentNullException(nameof(@event));
        var wrote = _channel.Writer.TryWrite(@event);
        if (!wrote && _options.ThrowOnSyncPublishWhenFull)
        {
            throw new InvalidOperationException("Event channel is full; use async Publish to respect backpressure.");
        }
        // else: best-effort drop (bounded + full) or always success (unbounded)
    }

    public IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));

        Task Wrapper(object e, CancellationToken ct) => handler((TEvent)e, ct);
        var type = typeof(TEvent);

        lock (_gate)
        {
            if (!_handlers.TryGetValue(type, out var list))
            {
                list = new List<Func<object, CancellationToken, Task>>();
                _handlers[type] = list;
            }
            list.Add(Wrapper);
        }

        return new Subscription(this, type, Wrapper);
    }

    private sealed class Subscription : IDisposable
    {
        private readonly EventBus _owner;
        private readonly Type _type;
        private readonly Func<object, CancellationToken, Task> _wrapper;

        public Subscription(EventBus owner, Type type, Func<object, CancellationToken, Task> wrapper)
            => (_owner, _type, _wrapper) = (owner, type, wrapper);

        public void Dispose()
        {
            lock (_owner._gate)
            {
                if (!_owner._handlers.TryGetValue(_type, out var list)) return;
                list.Remove(_wrapper);
                if (list.Count == 0) _owner._handlers.Remove(_type);
            }
        }
    }

    // Internal: drained by the background dispatcher
    internal async Task Dispatch(object @event, CancellationToken ct)
    {
        var evtType = @event.GetType();

        Func<object, CancellationToken, Task>[] snapshot;
        lock (_gate)
        {
            if (!_handlers.TryGetValue(evtType, out var list) || list.Count == 0) return;
            snapshot = list.ToArray(); // safe copy
        }

        // Process all the handlers associated with this event concurrently

        var tasks = new Task[snapshot.Length];
        for (var i = 0; i < snapshot.Length; i++)
        {
            var h = snapshot[i];
            tasks[i] = h(@event, ct);
        }

        await Task.WhenAll(tasks);
    }
}
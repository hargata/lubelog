using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Threading.Channels;

namespace CarCareTracker.Messaging;

public sealed class EventBus : IEventBus
{
    private readonly Channel<object> _channel;
    private readonly ILogger<EventBus>? _logger;
    private readonly EventBusOptions _options;

    private ImmutableDictionary<Type, ImmutableArray<Func<object, CancellationToken, Task>>> _handlers
        = ImmutableDictionary<Type, ImmutableArray<Func<object, CancellationToken, Task>>>.Empty;

    // For removals we need to know which wrapper we added.
    private readonly ConcurrentDictionary<IDisposable, (Type type, Func<object, CancellationToken, Task> wrapper)> _subscriptions = new();

    public EventBus(EventBusOptions? options = null, ILogger<EventBus>? logger = null)
    {
        _options = options ?? new EventBusOptions();
        _logger = logger;

        _channel = _options.BoundedCapacity is { } cap
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

    internal ChannelReader<object> Reader => _channel.Reader;

    public Task Publish(object @event, CancellationToken ct = default)
    {
        if (@event is null) throw new ArgumentNullException(nameof(@event));
        return _channel.Writer.WriteAsync(@event, ct).AsTask();
    }

    public void Publish(object @event)
    {
        if (@event is null) throw new ArgumentNullException(nameof(@event));
        var written = _channel.Writer.TryWrite(@event);
        if (!written && _options.ThrowOnSyncPublishWhenFull)
        {
            throw new InvalidOperationException("Event channel is full; use the async Publish overload to respect backpressure.");
        }
        // If not thrown and not written, the event is dropped by design.
    }

    public IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler)
    {
        if (handler is null) throw new ArgumentNullException(nameof(handler));

        // Wrap strongly-typed handler into an object-based one, with a fast cast.
        Task Wrapper(object e, CancellationToken ct) => handler((TEvent)e, ct);

        var type = typeof(TEvent);

        ImmutableArray<Func<object, CancellationToken, Task>> original, updated;
        do
        {
            original = _handlers.TryGetValue(type, out var arr) ? arr : ImmutableArray<Func<object, CancellationToken, Task>>.Empty;
            updated = original.Add(Wrapper);
        }
        while (Interlocked.CompareExchange(
                   ref _handlers,
                   _handlers.SetItem(type, updated),
                   _handlers) != _handlers);

        var disposable = new Removal(this);
        _subscriptions[disposable] = (type, Wrapper);
        return disposable;
    }

    private sealed class Removal : IDisposable
    {
        private EventBus? _owner;
        public Removal(EventBus owner) => _owner = owner;
        public void Dispose()
        {
            var owner = Interlocked.Exchange(ref _owner, null);
            if (owner is null) return;

            if (!owner._subscriptions.TryRemove(this, out var sub)) return;

            var (type, wrapper) = sub;
            ImmutableArray<Func<object, CancellationToken, Task>> updated;
            do
            {
                if (!owner._handlers.TryGetValue(type, out var original))
                    return;

                var idx = original.IndexOf(wrapper);
                if (idx < 0) return;

                updated = original.RemoveAt(idx);
            }
            while (Interlocked.CompareExchange(
                       ref owner._handlers,
                       updated.Length == 0 ? owner._handlers.Remove(type) : owner._handlers.SetItem(type, updated),
                       owner._handlers) != owner._handlers);
        }
    }

    /// <summary>
    /// Dispatch one event to all compatible handlers.
    /// </summary>
    internal async Task Dispatch(object @event, CancellationToken ct)
    {
        try
        {
            var evtType = @event.GetType();

            // Collect handlers where subscribed type is assignable from the event type
            // (i.e., subscribing to a base class or interface receives derived events).
            var toInvoke = Array.Empty<Func<object, CancellationToken, Task>>();

            foreach (var kvp in _handlers)
            {
                if (kvp.Key.IsAssignableFrom(evtType))
                {
                    // concatenate without allocations when possible
                    var src = kvp.Value;
                    if (toInvoke.Length == 0)
                    {
                        toInvoke = src.ToArray();
                    }
                    else
                    {
                        var tmp = new Func<object, CancellationToken, Task>[toInvoke.Length + src.Length];
                        Array.Copy(toInvoke, 0, tmp, 0, toInvoke.Length);
                        src.AsSpan().CopyTo(tmp.AsSpan(toInvoke.Length));
                        toInvoke = tmp;
                    }
                }
            }

            if (toInvoke.Length == 0) return;

            if (_options.PreserveHandlerOrder)
            {
                foreach (var h in toInvoke)
                {
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        await h(@event, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Event handler failed for {EventType}", evtType.FullName);
                    }
                }
            }
            else
            {
                var tasks = new Task[toInvoke.Length];
                for (var i = 0; i < toInvoke.Length; i++)
                {
                    var h = toInvoke[i];
                    tasks[i] = InvokeSafe(h, @event, ct);
                }
                await Task.WhenAll(tasks);
            }
        }
        catch (OperationCanceledException)
        {
            // normal during shutdown
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unhandled exception while dispatching event {Event}", @event);
        }
    }

    private async Task InvokeSafe(Func<object, CancellationToken, Task> h, object e, CancellationToken ct)
    {
        try { await h(e, ct); }
        catch (Exception ex) { _logger?.LogError(ex, "Event handler failed for {EventType}", e.GetType().FullName); }
    }
}
namespace CarCareTracker.Messaging;

public static class EventBusServiceCollectionExtensions
{
    public static IEventBus AddEventBus(
        this IServiceCollection services,
        Action<EventBusOptions>? configure = null)
    {
        var opts = new EventBusOptions();
        configure?.Invoke(opts);

        services.AddSingleton(sp =>
        {
            var logger = sp.GetService<ILogger<EventBus>>();
            return new EventBus(opts, logger);
        });

        services.AddHostedService(sp =>
        {
            var bus = (EventBus)sp.GetRequiredService<EventBus>();
            var logger = sp.GetService<ILogger<EventBusBackgroundService>>();
            return new EventBusBackgroundService(bus, logger);
        });

        services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<EventBus>());

        // ⚠️ Important: we can't return the actual instance yet, because DI container is not built.
        // Instead, return a proxy that resolves lazily.
        return new DeferredEventBus(services);
    }

    private sealed class DeferredEventBus : IEventBus
    {
        private readonly IServiceCollection _services;
        private IServiceProvider? _provider;
        private IEventBus Bus => (_provider ??= _services.BuildServiceProvider()).GetRequiredService<IEventBus>();

        public DeferredEventBus(IServiceCollection services) => _services = services;

        public Task Publish(object @event, CancellationToken ct = default) => Bus.Publish(@event, ct);

        public void Publish(object @event) => Bus.Publish(@event);

        public IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) => Bus.Subscribe(handler);
    }
}
using CarCareTracker.Messaging;
using CarCareTracker.Models;

namespace CarCareTracker.Webhooks;

public static class WebhookExtensions
{
    // Register mappings (no resolution here)
    public static IServiceCollection AddWebhooks(this IServiceCollection services, Action<WebhookMap> configure)
    {
        if (configure is null) throw new ArgumentNullException(nameof(configure));

        // single registry instance that stores the map actions
        var registry = new WebhookRegistry();
        configure(new WebhookMap(registry));

        services.AddSingleton(registry);
        return services;
    }

    public static WebApplication UseWebhooks(this WebApplication app)
    {
        var sp = app.Services;
        var bus = sp.GetRequiredService<IEventBus>();
        var registry = sp.GetRequiredService<WebhookRegistry>();

        var subs = new List<IDisposable>();
        foreach (var r in registry.Items)
        {
            // returns the IDisposable subscription
            var sub = r(sp, bus);
            subs.Add(sub);
        }

        return app;
    }
}

// Simple in-memory registry of “how to subscribe”
public sealed class WebhookRegistry
{
    private readonly List<Func<IServiceProvider, IEventBus, IDisposable>> _items = new();
    internal IEnumerable<Func<IServiceProvider, IEventBus, IDisposable>> Items => _items;

    internal void Add(Func<IServiceProvider, IEventBus, IDisposable> reg) => _items.Add(reg);
}

// Fluent map that records Register<T> calls
public sealed class WebhookMap
{
    private readonly WebhookRegistry _registry;
    public WebhookMap(WebhookRegistry registry) => _registry = registry;

    public WebhookMap Register<TEvent>(Func<TEvent, WebHookPayload> toPayload)
    {
        if (toPayload is null) throw new ArgumentNullException(nameof(toPayload));

        _registry.Add((sp, bus) =>
        {
            // Subscribe now (at UseWebhooks time). Per event we resolve IWebhookPublisher from a fresh scope.
            return bus.Subscribe<TEvent>(async (e, ct) =>
            {
                using var scope = sp.CreateScope();
                var publisher = scope.ServiceProvider.GetRequiredService<IWebhookPublisher>();

                var payload = toPayload(e);
                await publisher.PublishAsync(payload, ct);
            });
        });

        return this;
    }
}
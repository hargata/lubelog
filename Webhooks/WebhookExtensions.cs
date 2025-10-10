using CarCareTracker.Messaging;
using CarCareTracker.Models;

namespace CarCareTracker.Webhooks;

public static class WebhookExtensions
{
    public static IServiceCollection AddWebhooks(this IServiceCollection services, Action<WebhookMap> configure)
    {
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

        foreach (var reg in registry.Items)
            reg(sp, bus); // just subscribe

        return app;
    }
}

public sealed class WebhookRegistry
{
    private readonly List<Action<IServiceProvider, IEventBus>> _items = new();
    internal IEnumerable<Action<IServiceProvider, IEventBus>> Items => _items;
    internal void Add(Action<IServiceProvider, IEventBus> reg) => _items.Add(reg);
}

public sealed class WebhookMap
{
    private readonly WebhookRegistry _registry;
    public WebhookMap(WebhookRegistry registry) => _registry = registry;

    public WebhookMap Register<TEvent>(Func<TEvent, WebHookPayload> toPayload)
    {
        _registry.Add((sp, bus) =>
        {
            bus.Subscribe<TEvent>(async (e, ct) =>
            {
                using var scope = sp.CreateScope();
                var publisher = scope.ServiceProvider.GetRequiredService<IWebhookPublisher>();
                await publisher.PublishAsync(toPayload(e), ct);
            });
        });
        return this;
    }
}
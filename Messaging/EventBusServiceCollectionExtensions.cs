namespace CarCareTracker.Messaging;

public static class EventBusServiceCollectionExtensions
{
    public static void AddEventBus(
        this IServiceCollection services,
        Action<EventBusOptions>? configure = null)
    {
        var opts = new EventBusOptions();
        configure?.Invoke(opts);

        services.AddSingleton<EventBus>(sp =>
        {
            var logger = sp.GetService<ILogger<EventBus>>();
            return new EventBus(opts, logger);
        });

        services.AddHostedService(sp =>
        {
            var bus = sp.GetRequiredService<EventBus>();
            var logger = sp.GetService<ILogger<EventBusBackgroundService>>();
            return new EventBusBackgroundService(bus, logger);
        });

        services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<EventBus>());
    }
}
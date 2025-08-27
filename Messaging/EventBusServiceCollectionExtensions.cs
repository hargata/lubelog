namespace CarCareTracker.Messaging;

public static class EventBusServiceCollectionExtensions
{
    public static IServiceCollection AddEventBus(this IServiceCollection services, Action<EventBusOptions>? configure = null)
    {
        var opts = new EventBusOptions();
        configure?.Invoke(opts);

        services.AddSingleton(new EventBus(opts));
        services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<EventBus>());

        for (var i = 0; i < Math.Max(1, opts.NumReaders); i++)
            services.AddHostedService<EventBusBackgroundService>();

        return services;
    }
}
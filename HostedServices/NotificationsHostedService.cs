using CarCareTracker.Abstractions;

namespace CarCareTracker.HostedServices;

internal class NotificationsHostedService : BackgroundService
{
    private readonly INotificationChannelService _notificationChannelService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationsHostedService> _logger;

    public NotificationsHostedService(INotificationChannelService notificationChannelService,
                                      IServiceScopeFactory scopeFactory,
                                      ILogger<NotificationsHostedService> logger)
    {
        _notificationChannelService = notificationChannelService;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var notificationSenderService = scope.ServiceProvider.GetRequiredService<INotificationSenderService>();

        await foreach(var payload in _notificationChannelService.ReadAllAsync(stoppingToken))
        {
            try
            {
                await notificationSenderService.SendNotificationAsync(payload, stoppingToken);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to process notification");
            }
        }
    }
}
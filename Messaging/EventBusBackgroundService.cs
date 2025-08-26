namespace CarCareTracker.Messaging;

public sealed class EventBusBackgroundService : BackgroundService
{
    private readonly EventBus _bus;
    private readonly ILogger<EventBusBackgroundService>? _logger;

    public EventBusBackgroundService(EventBus bus, ILogger<EventBusBackgroundService>? logger = null)
    {
        _bus = bus;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger?.LogInformation("EventBus background dispatcher starting");
       
        var reader = _bus.Reader;

        while (!stoppingToken.IsCancellationRequested)
        {
            object next;
            try
            {
                next = await reader.ReadAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await _bus.Dispatch(next, stoppingToken);
        }

        _logger?.LogInformation("EventBus background dispatcher stopping");
    }
}
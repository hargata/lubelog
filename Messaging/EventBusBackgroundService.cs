namespace CarCareTracker.Messaging;

public sealed class EventBusBackgroundService : BackgroundService
{
    private readonly EventBus _bus;

    public EventBusBackgroundService(EventBus bus) => _bus = bus;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await foreach (var work in _bus.Reader.ReadAllAsync(ct))
        {
            try { await work(ct); } catch { /* MVP: ignore handler failures */ }
        }
    }
}
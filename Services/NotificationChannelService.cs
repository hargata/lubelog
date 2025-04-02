using System.Threading.Channels;
using CarCareTracker.Abstractions;
using CarCareTracker.Models;

namespace CarCareTracker.Services;

internal class NotificationChannelService : INotificationChannelService
{
    private readonly Channel<WebHookPayload> _channel;
    private readonly ILogger<NotificationChannelService> _logger;

    public NotificationChannelService(ILogger<NotificationChannelService> logger)
    {
        _logger = logger;

        var options = new BoundedChannelOptions(100)
        {
            SingleWriter = false,
            SingleReader = true,
        };

        _channel = Channel.CreateBounded<WebHookPayload>(options);
    }

    public async Task WriteAsync(WebHookPayload payload, CancellationToken ct = default)
    {
        await _channel.Writer.WriteAsync(payload, ct);
    }

    public IAsyncEnumerable<WebHookPayload> ReadAllAsync(CancellationToken ct = default)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }
}
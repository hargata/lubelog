using CarCareTracker.Abstractions;
using CarCareTracker.Helper;
using CarCareTracker.Models;

namespace CarCareTracker.Services;

internal class NotificationSenderService : INotificationSenderService
{
    private readonly HttpClient _client;
    private readonly IConfigHelper _config;
    
    private const string DiscordSchemaPrefix = "discord://";

    public NotificationSenderService(HttpClient httpClient, IConfigHelper config)
    {
        _config = config;
        _client = httpClient;
    }

    public async Task SendNotificationAsync(WebHookPayload webHookPayload, CancellationToken cancellationToken = default)
    {
        var webhookUrl = _config.GetWebHookUrl();
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            return;
        }

        if (webhookUrl.StartsWith(DiscordSchemaPrefix))
        {
            webhookUrl = webhookUrl.Replace(DiscordSchemaPrefix, "https://"); //cleanurl
            // format payload to discord format
            await _client.PostAsJsonAsync(webhookUrl, DiscordWebHook.FromWebHookPayload(webHookPayload), cancellationToken: cancellationToken);
        }
        else
        {
            await _client.PostAsJsonAsync(webhookUrl, webHookPayload, cancellationToken: cancellationToken);
        }
    }
}
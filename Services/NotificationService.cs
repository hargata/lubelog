using CarCareTracker.Abstractions;
using CarCareTracker.Helper;
using CarCareTracker.Models;

namespace CarCareTracker.Services;

internal class NotificationService : INotificationService
{
    private readonly HttpClient _client;
    private readonly IConfigHelper _config;

    public NotificationService(HttpClient httpClient, IConfigHelper config)
    {
        _config = config;
        _client = httpClient;
    }

    public async Task NotifyAsync(WebHookPayload webHookPayload)
    {
        var webhookUrl = _config.GetWebHookUrl();
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            return;
        }

        if (webhookUrl.StartsWith("discord://"))
        {
            webhookUrl = webhookUrl.Replace("discord://", "https://"); //cleanurl
            //format to discord
            await _client.PostAsJsonAsync(webhookUrl, DiscordWebHook.FromWebHookPayload(webHookPayload));
        }
        else
        {
            await _client.PostAsJsonAsync(webhookUrl, webHookPayload);
        }
    }
}
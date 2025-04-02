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

    public Task NotifyAsync(WebHookPayload webHookPayload)
    {
        var webhookUrl = _config.GetWebHookUrl();
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            return Task.CompletedTask;
        }

        if (webhookUrl.StartsWith("discord://"))
        {
            webhookUrl = webhookUrl.Replace("discord://", "https://"); //cleanurl
            //format to discord
            _client.PostAsJsonAsync(webhookUrl, DiscordWebHook.FromWebHookPayload(webHookPayload));
        }
        else
        {
            _client.PostAsJsonAsync(webhookUrl, webHookPayload);
        }

        return Task.CompletedTask;
    }
}
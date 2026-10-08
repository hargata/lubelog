using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CarCareTracker.Logic
{
    public interface INotificationLogic
    {
        Task RunAutomatedEvents();
        Task CheckReminderStateChanged();
        Task<OperationResponse> SendNotificationToExternalServices(NotificationServiceConfig serviceConfig, List<ReminderRecordViewModel> remindersToSend, Vehicle vehicle);
    }
    public class NotificationLogic: INotificationLogic
    {
        private List<CachedReminderRecord> _cachedReminders { get; set; } = new List<CachedReminderRecord>();
        private readonly IConfigHelper _config;
        private readonly IFileHelper _fileHelper;
        private readonly IMailHelper _mailHelper;
        private readonly ITranslationHelper _translator;
        private readonly IVehicleDataAccess _dataAccess;
        private readonly IVehicleLogic _vehicleLogic;
        private readonly IReminderRecordDataAccess _reminderRecordDataAccess;
        private readonly IUserAccessDataAccess _userAccessDataAccess;
        private readonly IUserRecordDataAccess _userRecordDataAccess;
        private readonly IReminderHelper _reminderHelper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<NotificationLogic> _logger;
        public NotificationLogic(IConfigHelper config, 
            IFileHelper fileHelper, 
            IMailHelper mailHelper,
            ITranslationHelper translationHelper,
            IReminderHelper reminderHelper,
            IVehicleDataAccess dataAccess,
            IUserAccessDataAccess userAccessDataAccess,
            IUserRecordDataAccess userRecordDataAccess,
            IReminderRecordDataAccess reminderRecordDataAccess,
            IVehicleLogic vehicleLogic,
            IHttpClientFactory httpClientFactory,
            ILogger<NotificationLogic> logger
            )
        {
            _config = config;
            _fileHelper = fileHelper;
            _mailHelper = mailHelper;
            _reminderHelper = reminderHelper;
            _translator = translationHelper;
            _dataAccess = dataAccess;
            _reminderRecordDataAccess = reminderRecordDataAccess;
            _userRecordDataAccess = userRecordDataAccess;
            _userAccessDataAccess = userAccessDataAccess;
            _vehicleLogic = vehicleLogic;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public async Task RunAutomatedEvents()
        {
            if (_config.GetAutomatedEventsEnabled())
            {
                List<Vehicle> vehicles = new List<Vehicle>();
                var notificationConfig = _config.GetNotificationConfig();
                var automatedEvents = notificationConfig.AutomatedEvents;
                var defaultEmailAddress = _config.GetDefaultReminderEmail();
                if (automatedEvents.Any())
                {
                    vehicles = _dataAccess.GetVehicles();
                }
                if (automatedEvents.Contains(AutomatedEvent.CleanTempFile) || automatedEvents.Contains(AutomatedEvent.DeepClean))
                {
                    try
                    {
                        //Clear out temp folder
                        var tempFilesDeleted = _fileHelper.ClearTempFolder();
                        _logger.LogInformation($"Temp Files Deleted: {tempFilesDeleted.ToString()}");
                        if (automatedEvents.Contains(AutomatedEvent.DeepClean))
                        {
                            //clear out unused vehicle thumbnails
                            var vehicleImages = _vehicleLogic.GetVehicleThumbnails(vehicles);
                            if (vehicleImages.Any())
                            {
                                var thumbnailsDeleted = _fileHelper.ClearUnlinkedThumbnails(vehicleImages);
                                _logger.LogInformation($"Unlinked Thumbnails Deleted: {thumbnailsDeleted.ToString()}");
                            }
                            var vehicleDocuments = new List<string>();
                            vehicleDocuments.AddRange(_vehicleLogic.GetVehicleDocuments(vehicles));
                            //shop supplies
                            vehicleDocuments.AddRange(_vehicleLogic.GetStoreSupplyDocuments());
                            if (vehicleDocuments.Any())
                            {
                                var documentsDeleted = _fileHelper.ClearUnlinkedDocuments(vehicleDocuments);
                                _logger.LogInformation($"Unlinked Documents Deleted: {documentsDeleted.ToString()}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Unable To Perform Cleanup Due To Error: {ex.Message}");
                    }
                }
                if (automatedEvents.Contains(AutomatedEvent.BackupEmail))
                {
                    if (!string.IsNullOrWhiteSpace(defaultEmailAddress))
                    {
                        try {
                            string backupFileLocation = _fileHelper.MakeBackup();
                            string fullExportFilePath = _fileHelper.GetFullFilePath(backupFileLocation, false);
                            var fileContents = await _fileHelper.GetFileBytesAsync(fullExportFilePath);
                            var result = await _mailHelper.SendBackupEmail(Path.GetFileName(backupFileLocation), fileContents, defaultEmailAddress);
                            if (result.Success)
                            {
                                _logger.LogInformation(result.Message);
                            }
                            else
                            {
                                _logger.LogError(result.Message);
                            }
                        } 
                        catch (Exception ex)
                        {
                            _logger.LogError($"Unable to Send Backup Email Due To Error: {ex.Message}");
                        }
                    } else
                    {
                        _logger.LogError("No Default Email Configured");
                    }
                }
                if (automatedEvents.Contains(AutomatedEvent.UpdateRecurringTax))
                {
                    try
                    {
                        int vehiclesUpdated = 0;
                        foreach (Vehicle vehicle in vehicles)
                        {
                            var updateResult = _vehicleLogic.UpdateRecurringTaxes(vehicle.Id);
                            if (updateResult)
                            {
                                vehiclesUpdated++;
                            }
                        }
                        if (vehiclesUpdated != default)
                        {
                            _logger.LogInformation($"Recurring Taxes for {vehiclesUpdated} Vehicles Updated!");
                        }
                        else
                        {
                            _logger.LogInformation("No Recurring Taxes Updated");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"No Recurring Taxes Updated Due To Error: {ex.Message}");
                    }
                }
                if (automatedEvents.Contains(AutomatedEvent.AllReminder))
                {
                    try
                    {
                        List<OperationResponse> operationResponses = new List<OperationResponse>();
                        List<OperationResponse> serviceResponses = new List<OperationResponse>();
                        var digestServiceConfigs = notificationConfig.ServiceConfigs.Where(x => x.IncludeAllReminders).ToList();
                        //skip emails when SMTP is not set up and reminders are being delivered via external services instead
                        bool sendEmails = !digestServiceConfigs.Any() || !string.IsNullOrWhiteSpace(_config.GetMailConfig().EmailServer);
                        foreach (Vehicle vehicle in vehicles)
                        {
                            var vehicleId = vehicle.Id;
                            //get reminders
                            var currentMileage = _vehicleLogic.GetMaxMileage(vehicleId);
                            var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
                            var results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, DateTime.Now).OrderByDescending(x => x.Urgency).ToList();
                            results.RemoveAll(x => !notificationConfig.UrgenciesTracked.Contains(x.Urgency));
                            if (!results.Any())
                            {
                                continue;
                            }
                            foreach (NotificationServiceConfig serviceConfig in digestServiceConfigs)
                            {
                                serviceResponses.Add(await SendNotificationToExternalServices(serviceConfig, results, vehicle));
                            }
                            if (!sendEmails)
                            {
                                continue;
                            }
                            //get list of recipients.
                            var userIds = _userAccessDataAccess.GetUserAccessByVehicleId(vehicleId).Select(x => x.Id.UserId);
                            List<string> emailRecipients = new List<string>();
                            if (!string.IsNullOrWhiteSpace(defaultEmailAddress))
                            {
                                emailRecipients.Add(defaultEmailAddress);
                            }
                            foreach (int userId in userIds)
                            {
                                var userData = _userRecordDataAccess.GetUserRecordById(userId);
                                emailRecipients.Add(userData.EmailAddress);
                            }
                            if (!emailRecipients.Any())
                            {
                                continue;
                            }
                            var result = await _mailHelper.NotifyUserForReminders(vehicle, emailRecipients, results);
                            operationResponses.Add(result);
                        }
                        if (serviceResponses.Any())
                        {
                            if (serviceResponses.All(x => x.Success))
                            {
                                _logger.LogInformation($"External Notifications Sent({serviceResponses.Count()})");
                            }
                            else
                            {
                                _logger.LogError($"External Notifications Sent({serviceResponses.Count(x => x.Success)}), External Notifications Failed({serviceResponses.Count(x => !x.Success)})");
                            }
                        }
                        if (!sendEmails)
                        {
                            _logger.LogInformation("SMTP Server Not Setup, Skipped Reminder Emails");
                        }
                        else if (!operationResponses.Any())
                        {
                            _logger.LogWarning("No Emails Sent, No Vehicles Available or No Recipients Configured");
                        }
                        else if (operationResponses.All(x => x.Success))
                        {
                            _logger.LogInformation($"Emails Sent({operationResponses.Count()})");
                        }
                        else if (operationResponses.All(x => !x.Success))
                        {
                            _logger.LogError($"All Emails Failed({operationResponses.Count()}), Check SMTP Settings");
                        }
                        else
                        {
                            _logger.LogWarning($"Emails Sent({operationResponses.Count(x => x.Success)}), Emails Failed({operationResponses.Count(x => !x.Success)}), Check Recipient Settings");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Unable To Send Reminders Email Due To Error: {ex.Message}");
                    }
                }
                if (automatedEvents.Contains(AutomatedEvent.ReminderStateChanged))
                {
                    await CheckReminderStateChanged();
                }
            }
        }
        public async Task CheckReminderStateChanged()
        {
            try
            {
                List<Vehicle> vehicles = new List<Vehicle>();
                var notificationConfig = _config.GetNotificationConfig();
                var automatedEvents = notificationConfig.AutomatedEvents;
                var defaultEmailAddress = _config.GetDefaultReminderEmail();
                var serverLanguage = _config.GetServerLanguage();
                var serverDomain = _config.GetServerDomain();
                if (!automatedEvents.Contains(AutomatedEvent.ReminderStateChanged))
                {
                    //return early because this event is not configured
                    return;
                }
                if (!notificationConfig.UseEmailNotification && !notificationConfig.ServiceConfigs.Any())
                {
                    //return early because there is no service to send to
                    return;
                }
                if (!notificationConfig.UrgenciesTracked.Any())
                {
                    //return early because no urgencies tracked
                    return;
                }
                vehicles = _dataAccess.GetVehicles();
                if (!vehicles.Any())
                {
                    //return early because there are no vehicles
                    return;
                }
                int _daysToCache = notificationConfig.DaysToCache * -1;
                //clear out expired reminders
                int expiredReminders = _cachedReminders.RemoveAll(x => x.DateAdded < DateTime.Now.AddDays(_daysToCache));
                if (expiredReminders != default)
                {
                    _logger.LogInformation($"Cleared {expiredReminders} Expired Reminders");
                }
                List<ReminderRecordViewModel> remindersToSend = new List<ReminderRecordViewModel>();
                foreach (Vehicle vehicle in vehicles)
                {
                    var vehicleId = vehicle.Id;
                    //get reminders
                    var currentMileage = _vehicleLogic.GetMaxMileage(vehicleId);
                    var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
                    var results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, DateTime.Now).OrderByDescending(x => x.Urgency).ToList();
                    //filter reminders with untracked urgencies
                    results.RemoveAll(x => !notificationConfig.UrgenciesTracked.Contains(x.Urgency));
                    //filter reminders that have already been cached
                    results.RemoveAll(x => _cachedReminders.Any(y => y.Id == x.Id && y.Urgency == x.Urgency));
                    if (!results.Any())
                    {
                        continue;
                    }
                    remindersToSend.AddRange(results);
                }
                if (remindersToSend.Any())
                {
                    var groupedNotifications = remindersToSend.GroupBy(x => x.VehicleId);
                    foreach (var groupedNotification in groupedNotifications)
                    {
                        var vehicle = vehicles.FirstOrDefault(x => x.Id == groupedNotification.Key) ?? new Vehicle();
                        if (vehicle.Id == default)
                        {
                            continue;
                        }
                        if (notificationConfig.UseEmailNotification)
                        {
                            var userIds = _userAccessDataAccess.GetUserAccessByVehicleId(vehicle.Id).Select(x => x.Id.UserId);
                            List<string> emailRecipients = new List<string>();
                            if (!string.IsNullOrWhiteSpace(defaultEmailAddress))
                            {
                                emailRecipients.Add(defaultEmailAddress);
                            }
                            foreach (int userId in userIds)
                            {
                                var userData = _userRecordDataAccess.GetUserRecordById(userId);
                                emailRecipients.Add(userData.EmailAddress);
                            }
                            if (emailRecipients.Any())
                            {
                                var result = await _mailHelper.NotifyUserForReminders(vehicle, emailRecipients, groupedNotification.ToList());
                                if (result.Success)
                                {
                                    _logger.LogInformation("Email Sent!");
                                }
                                else
                                {
                                    _logger.LogError($"Email Failed: {result.Message}");
                                }
                            }
                        }
                        foreach (NotificationServiceConfig serviceConfig in notificationConfig.ServiceConfigs)
                        {
                            await SendNotificationToExternalServices(serviceConfig, groupedNotification.ToList(), vehicle);
                        }
                        _cachedReminders.AddRange(groupedNotification.Select(x => new CachedReminderRecord { Id = x.Id, Urgency = x.Urgency, DateAdded = DateTime.Now }));
                    }
                }
                else
                {
                    _logger.LogInformation($"No Reminder State Changed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable To Detect Reminder State Change Due To Error {ex.Message}");
            }
        }
        public async Task<OperationResponse> SendNotificationToExternalServices(NotificationServiceConfig serviceConfig, List<ReminderRecordViewModel> remindersToSend, Vehicle vehicle)
        {
            if (string.IsNullOrWhiteSpace(serviceConfig.Url))
            {
                return OperationResponse.Failed("Notification Service URL Not Configured");
            }
            if (!remindersToSend.Any())
            {
                return OperationResponse.Failed("No reminders could be found");
            }
            var serverDomain = _config.GetServerDomain();
            string notificationTitle = $"{vehicle.Year} {vehicle.Make} {vehicle.Model} ({StaticHelper.GetVehicleIdentifier(vehicle)})";
            string linkToClick = string.Empty;
            if (!string.IsNullOrWhiteSpace(serverDomain))
            {
                string cleanedURL = serverDomain.EndsWith('/') ? serverDomain.TrimEnd('/') : serverDomain;
                linkToClick = $"{cleanedURL}/Vehicle/Index?vehicleId={vehicle.Id}&tab=reminder";
            }
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                List<HttpResponseMessage> responses = serviceConfig.ServiceType == NotificationServiceType.Discord
                    ? await SendDiscordNotification(httpClient, serviceConfig, remindersToSend, notificationTitle, linkToClick)
                    : await SendGenericNotification(httpClient, serviceConfig, remindersToSend, vehicle, notificationTitle, linkToClick, serverDomain);
                var failedResponse = responses.FirstOrDefault(x => !x.IsSuccessStatusCode);
                if (failedResponse != null)
                {
                    string errorBody = await failedResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"{serviceConfig.ServiceType} Notification Failed: {(int)failedResponse.StatusCode} {failedResponse.StatusCode} {errorBody}");
                    return OperationResponse.Failed($"Notification Failed: {(int)failedResponse.StatusCode} {failedResponse.StatusCode}");
                }
                return OperationResponse.Succeed("Notification Sent!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{serviceConfig.ServiceType} Notification Failed Due To Error: {ex.Message}");
                return OperationResponse.Failed(ex.Message);
            }
        }
        private async Task<List<HttpResponseMessage>> SendGenericNotification(HttpClient httpClient, NotificationServiceConfig serviceConfig, List<ReminderRecordViewModel> remindersToSend, Vehicle vehicle, string notificationTitle, string linkToClick, string serverDomain)
        {
            var serverLanguage = _config.GetServerLanguage();
            string vehicleId = vehicle.Id.ToString();
            string contentType = string.IsNullOrWhiteSpace(serviceConfig.ContentType) ? "text/plain" : serviceConfig.ContentType;
            //values substituted into a JSON body have to be escaped or quotes in descriptions will produce invalid JSON
            bool escapeForJson = contentType.Contains("json", StringComparison.OrdinalIgnoreCase);
            List<HttpResponseMessage> responses = new List<HttpResponseMessage>();
            //generic services receive one notification per reminder
            foreach (ReminderRecordViewModel reminderToSend in remindersToSend)
            {
                string notificationBody = $"{_translator.Translate(serverLanguage, StaticHelper.GetTitleCaseReminderUrgency(reminderToSend.Urgency))} - {reminderToSend.Description}";
                string priority = string.Empty;
                if (serviceConfig.PriorityMapping.TryGetValue(reminderToSend.Urgency.ToString(), out string? mappedPriority))
                {
                    priority = mappedPriority ?? string.Empty;
                }
                string messageBody = notificationBody;
                if (!string.IsNullOrWhiteSpace(serviceConfig.Body))
                {
                    messageBody = RenderNotificationBody(serviceConfig.Body, vehicleId, notificationTitle, notificationBody, priority, linkToClick, serverDomain, escapeForJson);
                }
                string cleanedUrl = RenderNotificationBody(serviceConfig.Url, vehicleId, notificationTitle, notificationBody, priority, linkToClick, serverDomain);
                var response = await SendWithRetry(httpClient, () =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, cleanedUrl);
                    foreach (var header in serviceConfig.Headers)
                    {
                        request.Headers.Add(header.Key, RenderNotificationBody(header.Value, vehicleId, notificationTitle, notificationBody, priority, linkToClick, serverDomain));
                    }
                    if (!string.IsNullOrWhiteSpace(messageBody))
                    {
                        request.Content = new StringContent(messageBody, Encoding.UTF8, contentType);
                    }
                    return request;
                });
                responses.Add(response);
            }
            return responses;
        }
        private async Task<List<HttpResponseMessage>> SendDiscordNotification(HttpClient httpClient, NotificationServiceConfig serviceConfig, List<ReminderRecordViewModel> remindersToSend, string notificationTitle, string linkToClick)
        {
            var serverLanguage = _config.GetServerLanguage();
            string webhookUrl = serviceConfig.Url.Trim().Replace("discord://", "https://");
            var embeds = remindersToSend.Select(reminder =>
            {
                string urgency = _translator.Translate(serverLanguage, StaticHelper.GetTitleCaseReminderUrgency(reminder.Urgency));
                string dueOn = reminder.Metric == ReminderMetric.Both ? $"{reminder.Date.ToShortDateString()} or {reminder.Mileage}" : reminder.Metric == ReminderMetric.Date ? $"{reminder.Date.ToShortDateString()}" : $"{reminder.Mileage}";
                return new DiscordEmbed
                {
                    Title = Truncate(reminder.Description, 256),
                    Description = $"**{urgency}** · {_translator.Translate(serverLanguage, "Due")}: {dueOn}",
                    Color = GetDiscordColor(reminder.Urgency),
                    Url = string.IsNullOrWhiteSpace(linkToClick) ? null : linkToClick
                };
            }).ToList();
            List<HttpResponseMessage> responses = new List<HttpResponseMessage>();
            //discord allows at most 10 embeds per message
            foreach (var embedChunk in embeds.Chunk(10))
            {
                var payload = new DiscordWebHook
                {
                    Content = $"**{notificationTitle}**",
                    Embeds = embedChunk.ToList()
                };
                string serializedPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                var response = await SendWithRetry(httpClient, () => new HttpRequestMessage(HttpMethod.Post, webhookUrl) { Content = new StringContent(serializedPayload, Encoding.UTF8, "application/json") });
                responses.Add(response);
                if (!response.IsSuccessStatusCode)
                {
                    break;
                }
            }
            return responses;
        }
        private async Task<HttpResponseMessage> SendWithRetry(HttpClient httpClient, Func<HttpRequestMessage> createRequest)
        {
            int maxAttempts = 3;
            var response = await httpClient.SendAsync(createRequest());
            for (int attempt = 1; attempt < maxAttempts && response.StatusCode == System.Net.HttpStatusCode.TooManyRequests; attempt++)
            {
                var retryDelay = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(Math.Pow(2, attempt));
                _logger.LogWarning($"Notification Service: Too Many Requests, Delaying for {retryDelay.TotalSeconds} seconds before attempt {attempt + 1}/{maxAttempts}");
                response.Dispose();
                await Task.Delay(retryDelay);
                response = await httpClient.SendAsync(createRequest());
            }
            return response;
        }
        private static int GetDiscordColor(ReminderUrgency urgency)
        {
            //matches the bootstrap badge colors used for reminders in the UI
            switch (urgency)
            {
                case ReminderUrgency.PastDue:
                    return 0x6C757D;
                case ReminderUrgency.VeryUrgent:
                    return 0xDC3545;
                case ReminderUrgency.Urgent:
                    return 0xFFC107;
                default:
                    return 0x198754;
            }
        }
        private static string Truncate(string input, int maxLength)
        {
            return input.Length <= maxLength ? input : $"{input.Substring(0, maxLength - 3)}...";
        }
        private string RenderNotificationBody(string inputString, string vehicleId, string title, string message, string priority, string linkToClick, string domain, bool escapeForJson = false)
        {
            Func<string, string> encode = escapeForJson ? EscapeJsonString : (value => value);
            return inputString.Replace("{vehicleId}", encode(vehicleId)).Replace("{title}", encode(title)).Replace("{message}", encode(message)).Replace("{priority}", encode(priority)).Replace("{link}", encode(linkToClick)).Replace("{domain}", encode(domain));
        }
        private static string EscapeJsonString(string value)
        {
            //serialize as a JSON string and strip the surrounding quotes so the value can be placed inside a quoted template field
            var serialized = JsonSerializer.Serialize(value, new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            return serialized.Substring(1, serialized.Length - 2);
        }
    }
}
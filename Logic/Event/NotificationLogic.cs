using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using System.Text;
using System.Text.Json;

namespace CarCareTracker.Logic
{
    public interface INotificationLogic
    {
        Task RunAutomatedEvents();
        Task CheckReminderStateChanged();
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
                        _logger.LogInformation($"temp_files_deleted: {tempFilesDeleted.ToString()}");
                        if (automatedEvents.Contains(AutomatedEvent.DeepClean))
                        {
                            //clear out unused vehicle thumbnails
                            var vehicleImages = _vehicleLogic.GetVehicleThumbnails(vehicles);
                            if (vehicleImages.Any())
                            {
                                var thumbnailsDeleted = _fileHelper.ClearUnlinkedThumbnails(vehicleImages);
                                _logger.LogInformation($"unlinked_thumbnails_deleted: {thumbnailsDeleted.ToString()}");
                            }
                            var vehicleDocuments = new List<string>();
                            vehicleDocuments.AddRange(_vehicleLogic.GetVehicleDocuments(vehicles));
                            //shop supplies
                            vehicleDocuments.AddRange(_vehicleLogic.GetStoreSupplyDocuments());
                            if (vehicleDocuments.Any())
                            {
                                var documentsDeleted = _fileHelper.ClearUnlinkedDocuments(vehicleDocuments);
                                _logger.LogInformation($"unlinked_documents_deleted: {documentsDeleted.ToString()}");
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
                            var fileContents = _fileHelper.GetFileBytes(fullExportFilePath);
                            var result = _mailHelper.SendBackupEmail(Path.GetFileName(backupFileLocation), fileContents, defaultEmailAddress);
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
                            var result = _mailHelper.NotifyUserForReminders(vehicle, emailRecipients, results);
                            operationResponses.Add(result);
                        }
                        if (!operationResponses.Any())
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
                                var result = _mailHelper.NotifyUserForReminders(vehicle, emailRecipients, groupedNotification.ToList());
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
                        if (notificationConfig.ServiceConfigs.Any())
                        {
                            string notificationTitle = $"{vehicle.Year} {vehicle.Make} {vehicle.Model} ({StaticHelper.GetVehicleIdentifier(vehicle)})";
                            string vehicleId = vehicle.Id.ToString();
                            string linkToClick = string.Empty;
                            if (!string.IsNullOrWhiteSpace(serverDomain))
                            {
                                string cleanedURL = serverDomain.EndsWith('/') ? serverDomain.TrimEnd('/') : serverDomain;
                                linkToClick = $"{cleanedURL}/Vehicle/Index?vehicleId={vehicleId}&tab=reminder";
                            }
                            var httpClient = _httpClientFactory.CreateClient();
                            foreach (ReminderRecordViewModel reminderToSend in groupedNotification)
                            {
                                string notificationBody = $"{_translator.Translate(serverLanguage, StaticHelper.GetTitleCaseReminderUrgency(reminderToSend.Urgency))} - {reminderToSend.Description}";
                                foreach (NotificationServiceConfig serviceConfig in notificationConfig.ServiceConfigs)
                                {
                                    //loop through all configured service
                                    string priority = string.Empty;
                                    if (serviceConfig.PriorityMapping.TryGetValue(reminderToSend.Urgency.ToString().ToLower(), out string? mappedPriority))
                                    {
                                        priority = mappedPriority ?? string.Empty;
                                    }
                                    string messageBody = notificationBody;
                                    if (serviceConfig.Body != null)
                                    {
                                        string templateBody = JsonSerializer.Serialize(serviceConfig.Body).Trim('"');
                                        messageBody = RenderNotificationBody(templateBody, vehicleId, notificationTitle, notificationBody, priority, linkToClick);
                                    }
                                    string cleanedUrl = RenderNotificationBody(serviceConfig.Url, vehicleId, notificationTitle, notificationBody, priority, linkToClick);
                                    var request = new HttpRequestMessage(HttpMethod.Post, cleanedUrl);
                                    if (serviceConfig.Headers.Any())
                                    {
                                        foreach (var header in serviceConfig.Headers)
                                        {
                                            request.Headers.Add(header.Key, RenderNotificationBody(header.Value, vehicleId, notificationTitle, notificationBody, priority, linkToClick));
                                        }
                                    }
                                    if (!string.IsNullOrWhiteSpace(messageBody))
                                    {
                                        request.Content = new StringContent(messageBody, Encoding.UTF8, serviceConfig.ContentType);
                                    }
                                    await httpClient.SendAsync(request);
                                }
                            }
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
        private string RenderNotificationBody(string inputString, string vehicleId, string title, string message, string priority, string linkToClick)
        {
            return inputString.Replace("{vehicleId}", vehicleId).Replace("{title}", title).Replace("{message}", message).Replace("{priority}", priority).Replace("{link}", linkToClick);
        }
    }
}
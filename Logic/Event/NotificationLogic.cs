using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;

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
        private readonly IVehicleDataAccess _dataAccess;
        private readonly IVehicleLogic _vehicleLogic;
        private readonly IReminderRecordDataAccess _reminderRecordDataAccess;
        private readonly IUserAccessDataAccess _userAccessDataAccess;
        private readonly IUserRecordDataAccess _userRecordDataAccess;
        private readonly IReminderHelper _reminderHelper;
        private readonly ILogger<NotificationLogic> _logger;
        public NotificationLogic(IConfigHelper config, 
            IFileHelper fileHelper, 
            IMailHelper mailHelper,
            IReminderHelper reminderHelper,
            IVehicleDataAccess dataAccess,
            IUserAccessDataAccess userAccessDataAccess,
            IUserRecordDataAccess userRecordDataAccess,
            IReminderRecordDataAccess reminderRecordDataAccess,
            IVehicleLogic vehicleLogic,
            ILogger<NotificationLogic> logger
            )
        {
            _config = config;
            _fileHelper = fileHelper;
            _mailHelper = mailHelper;
            _reminderHelper = reminderHelper;
            _dataAccess = dataAccess;
            _reminderRecordDataAccess = reminderRecordDataAccess;
            _userRecordDataAccess = userRecordDataAccess;
            _userAccessDataAccess = userAccessDataAccess;
            _vehicleLogic = vehicleLogic;
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
                if (automatedEvents.Contains(AutomatedEvent.BackupEmail))
                {
                    if (!string.IsNullOrWhiteSpace(defaultEmailAddress))
                    {
                        string backupFileLocation = _fileHelper.MakeBackup();
                        var fileContents = _fileHelper.GetFileBytes(backupFileLocation);
                        var result = _mailHelper.SendBackupEmail(Path.GetFileName(backupFileLocation), fileContents, defaultEmailAddress);
                        if (result.Success)
                        {
                            _logger.LogInformation(result.Message);
                        } else
                        {
                            _logger.LogError(result.Message);
                        }
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
                if (automatedEvents.Contains(AutomatedEvent.ReminderStateChanged))
                {
                    await CheckReminderStateChanged();
                }
            }
        }
        public async Task CheckReminderStateChanged()
        {
            List<Vehicle> vehicles = new List<Vehicle>();
            var notificationConfig = _config.GetNotificationConfig();
            var automatedEvents = notificationConfig.AutomatedEvents;
            var defaultEmailAddress = _config.GetDefaultReminderEmail();
            if (automatedEvents.Any())
            {
                vehicles = _dataAccess.GetVehicles();
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
                foreach(var groupedNotification in groupedNotifications)
                {
                    var vehicle = vehicles.FirstOrDefault(x=>x.Id == groupedNotification.Key) ?? new Vehicle();
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
                        foreach (NotificationServiceConfig serviceConfig in notificationConfig.ServiceConfigs)
                        {
                            //loop through all configured service
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
    }
}
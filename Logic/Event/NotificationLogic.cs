using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;

namespace CarCareTracker.Logic
{
    public interface INotificationLogic
    {
        Task RunAutomatedEvents();
    }
    public class NotificationLogic: INotificationLogic
    {
        private List<ReminderRecord> _cachedReminders { get; set; } = new List<ReminderRecord>();
        private readonly IConfigHelper _config;
        private readonly IFileHelper _fileHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IVehicleDataAccess _dataAccess;
        private readonly INoteDataAccess _noteDataAccess;
        private readonly IServiceRecordDataAccess _serviceRecordDataAccess;
        private readonly IGasRecordDataAccess _gasRecordDataAccess;
        private readonly ICollisionRecordDataAccess _collisionRecordDataAccess;
        private readonly ITaxRecordDataAccess _taxRecordDataAccess;
        private readonly IReminderRecordDataAccess _reminderRecordDataAccess;
        private readonly IUpgradeRecordDataAccess _upgradeRecordDataAccess;
        private readonly IOdometerRecordDataAccess _odometerRecordDataAccess;
        private readonly ISupplyRecordDataAccess _supplyRecordDataAccess;
        private readonly IPlanRecordDataAccess _planRecordDataAccess;
        private readonly IPlanRecordTemplateDataAccess _planRecordTemplateDataAccess;
        private readonly IInspectionRecordDataAccess _inspectionRecordDataAccess;
        private readonly IInspectionRecordTemplateDataAccess _inspectionRecordTemplateDataAccess;
        private readonly IEquipmentRecordDataAccess _equipmentRecordDataAccess;
        private readonly IVehicleLogic _vehicleLogic;
        private readonly ILogger<NotificationLogic> _logger;
        public NotificationLogic(IConfigHelper config, 
            IFileHelper fileHelper, 
            IMailHelper mailHelper,
            IVehicleDataAccess dataAccess,
            INoteDataAccess noteDataAccess,
            IServiceRecordDataAccess serviceRecordDataAccess,
            IGasRecordDataAccess gasRecordDataAccess,
            ICollisionRecordDataAccess collisionRecordDataAccess,
            ITaxRecordDataAccess taxRecordDataAccess,
            IReminderRecordDataAccess reminderRecordDataAccess,
            IUpgradeRecordDataAccess upgradeRecordDataAccess,
            IOdometerRecordDataAccess odometerRecordDataAccess,
            ISupplyRecordDataAccess supplyRecordDataAccess,
            IPlanRecordDataAccess planRecordDataAccess,
            IPlanRecordTemplateDataAccess planRecordTemplateDataAccess,
            IInspectionRecordDataAccess inspectionRecordDataAccess,
            IInspectionRecordTemplateDataAccess inspectionRecordTemplateDataAccess,
            IEquipmentRecordDataAccess equipmentRecordDataAccess,
            IVehicleLogic vehicleLogic,
            ILogger<NotificationLogic> logger
            )
        {
            _config = config;
            _fileHelper = fileHelper;
            _mailHelper = mailHelper;
            _dataAccess = dataAccess;
            _noteDataAccess = noteDataAccess;
            _serviceRecordDataAccess = serviceRecordDataAccess;
            _gasRecordDataAccess = gasRecordDataAccess;
            _collisionRecordDataAccess = collisionRecordDataAccess;
            _taxRecordDataAccess = taxRecordDataAccess;
            _reminderRecordDataAccess = reminderRecordDataAccess;
            _upgradeRecordDataAccess = upgradeRecordDataAccess;
            _odometerRecordDataAccess = odometerRecordDataAccess;
            _supplyRecordDataAccess = supplyRecordDataAccess;
            _planRecordDataAccess = planRecordDataAccess;
            _planRecordTemplateDataAccess = planRecordTemplateDataAccess;
            _inspectionRecordDataAccess = inspectionRecordDataAccess;
            _inspectionRecordTemplateDataAccess = inspectionRecordTemplateDataAccess;
            _equipmentRecordDataAccess = equipmentRecordDataAccess;
            _vehicleLogic = vehicleLogic;
            _logger = logger;
        }
        public async Task RunAutomatedEvents()
        {
            if (_config.GetAutomatedEventsEnabled())
            {
                var automatedEvents = _config.GetNotificationConfig().AutomatedEvents;
                if (automatedEvents.Contains(AutomatedEvent.CleanTempFile) || automatedEvents.Contains(AutomatedEvent.DeepClean))
                {
                    //Clear out temp folder
                    var tempFilesDeleted = _fileHelper.ClearTempFolder();
                    _logger.LogInformation($"temp_files_deleted: {tempFilesDeleted.ToString()}");
                    if (automatedEvents.Contains(AutomatedEvent.DeepClean))
                    {
                        //clear out unused vehicle thumbnails
                        var vehicles = _dataAccess.GetVehicles();
                        var vehicleImages = vehicles.Select(x => x.ImageLocation).Where(x => x.StartsWith("/images/")).Select(x => Path.GetFileName(x)).ToList();
                        if (vehicleImages.Any())
                        {
                            var thumbnailsDeleted = _fileHelper.ClearUnlinkedThumbnails(vehicleImages);
                            _logger.LogInformation($"unlinked_thumbnails_deleted: {thumbnailsDeleted.ToString()}");
                        }
                        var vehicleDocuments = new List<string>();
                        foreach (Vehicle vehicle in vehicles)
                        {
                            if (!string.IsNullOrWhiteSpace(vehicle.MapLocation))
                            {
                                vehicleDocuments.Add(Path.GetFileName(vehicle.MapLocation));
                            }
                            vehicleDocuments.AddRange(_serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                            vehicleDocuments.AddRange(_collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                            vehicleDocuments.AddRange(_upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                            vehicleDocuments.AddRange(_taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                            vehicleDocuments.AddRange(_gasRecordDataAccess.GetGasRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                            vehicleDocuments.AddRange(_noteDataAccess.GetNotesByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                            vehicleDocuments.AddRange(_odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                            vehicleDocuments.AddRange(_supplyRecordDataAccess.GetSupplyRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                            vehicleDocuments.AddRange(_planRecordDataAccess.GetPlanRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                            vehicleDocuments.AddRange(_planRecordTemplateDataAccess.GetPlanRecordTemplatesByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                            vehicleDocuments.AddRange(_inspectionRecordDataAccess.GetInspectionRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                            vehicleDocuments.AddRange(_inspectionRecordTemplateDataAccess.GetInspectionRecordTemplatesByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                            vehicleDocuments.AddRange(_equipmentRecordDataAccess.GetEquipmentRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                        }
                        //shop supplies
                        vehicleDocuments.AddRange(_supplyRecordDataAccess.GetSupplyRecordsByVehicleId(0).SelectMany(x => x.Files).Select(y => Path.GetFileName(y.Location)));
                        if (vehicleDocuments.Any())
                        {
                            var documentsDeleted = _fileHelper.ClearUnlinkedDocuments(vehicleDocuments);
                            _logger.LogInformation($"unlinked_documents_deleted: {documentsDeleted.ToString()}");
                        }
                    }
                }
                if (automatedEvents.Contains(AutomatedEvent.BackupEmail))
                {
                    var defaultEmailAddress = _config.GetDefaultReminderEmail();
                    if (!string.IsNullOrWhiteSpace(defaultEmailAddress))
                    {
                        string backupFileLocation = _fileHelper.MakeBackup();
                        var fileContents = _fileHelper.GetFileBytes(backupFileLocation);
                        var result = _mailHelper.SendBackupEmail(Path.GetFileName(backupFileLocation), fileContents, defaultEmailAddress);
                        _logger.LogInformation(result.Message);
                    }
                }
                if (automatedEvents.Contains(AutomatedEvent.UpdateRecurringTax))
                {
                    List<Vehicle> vehicles = _dataAccess.GetVehicles();
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
                        _logger.LogInformation($"No Recurring Taxes Updated Due To Error: {ex.Message}");
                    }
                }
            }
        }
        public async Task CheckReminderStateChanged()
        {

        }
    }
}
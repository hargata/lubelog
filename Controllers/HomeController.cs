using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CarCareTracker.Helper;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CarCareTracker.Logic;
using System.Globalization;

namespace CarCareTracker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IVehicleDataAccess _dataAccess;
        private readonly IUserLogic _userLogic;
        private readonly ILoginLogic _loginLogic;
        private readonly IVehicleLogic _vehicleLogic;
        private readonly IFileHelper _fileHelper;
        private readonly IConfigHelper _config;
        private readonly IExtraFieldDataAccess _extraFieldDataAccess;
        private readonly IReminderRecordDataAccess _reminderRecordDataAccess;
        private readonly IReminderHelper _reminderHelper;
        private readonly ITranslationHelper _translationHelper;
        private readonly IMailHelper _mailHelper;
        public HomeController(ILogger<HomeController> logger,
            IVehicleDataAccess dataAccess,
            IUserLogic userLogic,
            ILoginLogic loginLogic,
            IVehicleLogic vehicleLogic,
            IConfigHelper configuration,
            IFileHelper fileHelper,
            IExtraFieldDataAccess extraFieldDataAccess,
            IReminderRecordDataAccess reminderRecordDataAccess,
            IReminderHelper reminderHelper,
            ITranslationHelper translationHelper,
            IMailHelper mailHelper)
        {
            _logger = logger;
            _dataAccess = dataAccess;
            _config = configuration;
            _userLogic = userLogic;
            _fileHelper = fileHelper;
            _extraFieldDataAccess = extraFieldDataAccess;
            _reminderRecordDataAccess = reminderRecordDataAccess;
            _reminderHelper = reminderHelper;
            _loginLogic = loginLogic;
            _vehicleLogic = vehicleLogic;
            _translationHelper = translationHelper;
            _mailHelper = mailHelper;
        }
        private int GetUserID()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
        public IActionResult Index(string tab = "garage")
        {
            return View(model: tab);
        }
        [Route("/kiosk")]
        public IActionResult Kiosk(string exclusions, KioskMode kioskMode = KioskMode.Vehicle)
        { 
            try {
                var viewModel = new KioskViewModel
                {
                    Exclusions = string.IsNullOrWhiteSpace(exclusions) ? new List<int>() : exclusions.Split(',').Select(x => int.Parse(x)).ToList(),
                    KioskMode = kioskMode
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return View(new KioskViewModel());
            }
        }
        [HttpPost]
        public IActionResult KioskContent(KioskViewModel kioskParameters)
        {
            var vehiclesStored = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehiclesStored = _userLogic.FilterUserVehicles(vehiclesStored, GetUserID());
            }
            vehiclesStored.RemoveAll(x => kioskParameters.Exclusions.Contains(x.Id));
            var userConfig = _config.GetUserConfig(User);
            if (userConfig.HideSoldVehicles)
            {
                vehiclesStored.RemoveAll(x => !string.IsNullOrWhiteSpace(x.SoldDate));
            }
            switch (kioskParameters.KioskMode)
            {
                case KioskMode.Vehicle:
                    {
                        var kioskResult = _vehicleLogic.GetVehicleInfo(vehiclesStored);
                        return PartialView("_Kiosk", kioskResult);
                    }
                case KioskMode.Plan:
                    {
                        var kioskResult = _vehicleLogic.GetPlans(vehiclesStored, true);
                        return PartialView("_KioskPlan", kioskResult);
                    }
                case KioskMode.Reminder:
                    {
                        var kioskResult = _vehicleLogic.GetReminders(vehiclesStored, false);
                        return PartialView("_KioskReminder", kioskResult);
                    }
            }
            var result = _vehicleLogic.GetVehicleInfo(vehiclesStored);
            return PartialView("_Kiosk", result);
        }
        public IActionResult Garage()
        {
            var vehiclesStored = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehiclesStored = _userLogic.FilterUserVehicles(vehiclesStored, GetUserID());
            }
            var vehicleViewModels = vehiclesStored.Select(x =>
            {
                var vehicleVM = new VehicleViewModel
                {
                    Id = x.Id,
                    ImageLocation = x.ImageLocation,
                    Year = x.Year,
                    Make = x.Make,
                    Model = x.Model,
                    LicensePlate = x.LicensePlate,
                    SoldDate = x.SoldDate,
                    IsElectric = x.IsElectric,
                    IsDiesel = x.IsDiesel,
                    UseHours = x.UseHours,
                    OdometerOptional = x.OdometerOptional,
                    ExtraFields = x.ExtraFields,
                    Tags = x.Tags,
                    DashboardMetrics = x.DashboardMetrics,
                    VehicleIdentifier = x.VehicleIdentifier
                };
                //dashboard metrics
                if (x.DashboardMetrics.Any())
                {
                    var vehicleRecords = _vehicleLogic.GetVehicleRecords(x.Id);
                    var userConfig = _config.GetUserConfig(User);
                    var distanceUnit = x.UseHours ? "h" : userConfig.UseMPG ? "mi." : "km";
                    if (vehicleVM.DashboardMetrics.Contains(DashboardMetric.Default))
                    {
                        vehicleVM.LastReportedMileage = _vehicleLogic.GetMaxMileage(vehicleRecords);
                        vehicleVM.HasReminders = _vehicleLogic.GetVehicleHasUrgentOrPastDueReminders(x.Id, vehicleVM.LastReportedMileage);
                    }
                    if (vehicleVM.DashboardMetrics.Contains(DashboardMetric.CostPerMile))
                    {
                        var vehicleTotalCost = _vehicleLogic.GetVehicleTotalCost(vehicleRecords);
                        var maxMileage = _vehicleLogic.GetMaxMileage(vehicleRecords);
                        var minMileage = _vehicleLogic.GetMinMileage(vehicleRecords);
                        var totalDistance = maxMileage - minMileage;
                        vehicleVM.CostPerMile = totalDistance != default ? vehicleTotalCost / totalDistance : 0.00M;
                        vehicleVM.DistanceUnit = distanceUnit;
                    }
                    if (vehicleVM.DashboardMetrics.Contains(DashboardMetric.TotalCost))
                    {
                        vehicleVM.TotalCost = _vehicleLogic.GetVehicleTotalCost(vehicleRecords);
                    }
                }
                return vehicleVM;
            }).ToList();
            return PartialView("_GarageDisplay", vehicleViewModels);
        }
        public IActionResult Calendar()
        {
            var vehiclesStored = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehiclesStored = _userLogic.FilterUserVehicles(vehiclesStored, GetUserID());
            }
            var reminders = _vehicleLogic.GetReminders(vehiclesStored, true);
            return PartialView("_Calendar", reminders);
        }
        public IActionResult ViewCalendarReminder(int reminderId)
        {
            var reminder = _reminderRecordDataAccess.GetReminderRecordById(reminderId);
            var reminderUrgency = _reminderHelper.GetReminderRecordViewModels(new List<ReminderRecord> { reminder }, 0, DateTime.Now).FirstOrDefault();
            return PartialView("_ReminderRecordCalendarModal", reminderUrgency);
        }
        public async Task<IActionResult> Settings()
        {
            var userConfig = _config.GetUserConfig(User);
            var languages = _fileHelper.GetLanguages();
            var viewModel = new SettingsViewModel
            {
                UserConfig = userConfig,
                UILanguages = languages
            };
            return PartialView("_Settings", viewModel);
        }
        public async Task<IActionResult> Sponsors()
        {
            try
            {
                var httpClient = new HttpClient();
                var sponsorsData = await httpClient.GetFromJsonAsync<Sponsors>(StaticHelper.SponsorsPath) ?? new Sponsors();
                return PartialView("_Sponsors", sponsorsData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to retrieve sponsors: {ex.Message}");
                return PartialView("_Sponsors", new Sponsors());
            }
        }
        [HttpPost]
        public IActionResult WriteToSettings(UserConfig userConfig)
        {
            //retrieve existing userConfig.
            var existingConfig = _config.GetUserConfig(User);
            //copy over stuff that persists
            userConfig.UserColumnPreferences = existingConfig.UserColumnPreferences;
            var result = _config.SaveUserConfig(User, userConfig);
            return Json(result);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        public IActionResult GetExtraFieldsModal(int importMode = 0)
        {
            var recordExtraFields = _extraFieldDataAccess.GetExtraFieldsById(importMode);
            if (recordExtraFields.Id != importMode)
            {
                recordExtraFields.Id = importMode;
            }
            return PartialView("_ExtraFields", recordExtraFields);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        public IActionResult UpdateExtraFields(RecordExtraField record)
        {
            try
            {
                var result = _extraFieldDataAccess.SaveExtraFields(record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            var recordExtraFields = _extraFieldDataAccess.GetExtraFieldsById(record.Id);
            return PartialView("_ExtraFields", recordExtraFields);
        }
        [HttpPost]
        public IActionResult GenerateTokenForUser()
        {
            try
            {
                //get current user email address.
                var emailAddress = User.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrWhiteSpace(emailAddress))
                {
                    var result = _loginLogic.GenerateTokenForEmailAddress(emailAddress, false);
                    return Json(result);
                }
                return Json(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(false);
            }
        }
        [HttpPost]
        public IActionResult UpdateUserAccount(LoginModel userAccount)
        {
            try
            {
                var userId = GetUserID();
                if (userId > 0)
                {
                    var result = _loginLogic.UpdateUserDetails(userId, userAccount);
                    return Json(result);
                }
                return Json(OperationResponse.Failed());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(OperationResponse.Failed());
            }
        }
        [HttpGet]
        public IActionResult GetUserAccountInformationModal()
        {
            var emailAddress = User.FindFirstValue(ClaimTypes.Email);
            var userName = User.Identity.Name;
            return PartialView("_AccountModal", new UserData() { EmailAddress = emailAddress, UserName = userName });
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        public IActionResult GetRootAccountInformationModal()
        {
            var userName = User.Identity.Name;
            return PartialView("_RootAccountModal", new UserData() { UserName = userName });
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        public IActionResult GetTranslatorEditor(string userLanguage)
        {
            var translationData = _translationHelper.GetTranslations(userLanguage);
            return PartialView("_TranslationEditor", translationData);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpPost]
        public IActionResult SaveTranslation(string userLanguage, Dictionary<string, string> translationData)
        {
            var result = _translationHelper.SaveTranslation(userLanguage, translationData);
            return Json(result);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpPost]
        public IActionResult ExportTranslation(Dictionary<string, string> translationData)
        {
            var result = _translationHelper.ExportTranslation(translationData);
            return Json(result);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        public async Task<IActionResult> GetAvailableTranslations()
        {
            try
            {
                var httpClient = new HttpClient();
                var translations = await httpClient.GetFromJsonAsync<Translations>(StaticHelper.TranslationDirectoryPath) ?? new Translations();
                return PartialView("_Translations", translations);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to retrieve translations: {ex.Message}");
                return PartialView("_Translations", new Translations());
            }
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        public async Task<IActionResult> DownloadTranslation(string continent, string name)
        {
            try
            {
                var httpClient = new HttpClient();
                var translationData = await httpClient.GetFromJsonAsync<Dictionary<string, string>>(StaticHelper.GetTranslationDownloadPath(continent, name)) ?? new Dictionary<string, string>();
                if (translationData.Any())
                {
                    var result = _translationHelper.SaveTranslation(name, translationData);
                    if (!result.Success)
                    {
                        return Json(false);
                    }
                }
                else
                {
                    _logger.LogError($"Unable to download translation: {name}");
                    return Json(false);
                }
                return Json(true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to download translation: {ex.Message}");
                return Json(false);
            }
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        public async Task<IActionResult> DownloadAllTranslations()
        {
            try
            {
                var httpClient = new HttpClient();
                var translations = await httpClient.GetFromJsonAsync<Translations>(StaticHelper.TranslationDirectoryPath) ?? new Translations();
                int translationsDownloaded = 0;
                foreach (string translation in translations.Asia)
                {
                    try
                    {
                        var translationData = await httpClient.GetFromJsonAsync<Dictionary<string, string>>(StaticHelper.GetTranslationDownloadPath("Asia", translation)) ?? new Dictionary<string, string>();
                        if (translationData.Any())
                        {
                            var result = _translationHelper.SaveTranslation(translation, translationData);
                            if (result.Success) 
                            {
                                translationsDownloaded++;
                            };
                        }
                    } 
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error Downloading Translation {translation}: {ex.Message} ");
                    }
                }
                foreach (string translation in translations.Africa)
                {
                    try
                    {
                        var translationData = await httpClient.GetFromJsonAsync<Dictionary<string, string>>(StaticHelper.GetTranslationDownloadPath("Africa", translation)) ?? new Dictionary<string, string>();
                        if (translationData.Any())
                        {
                            var result = _translationHelper.SaveTranslation(translation, translationData);
                            if (result.Success)
                            {
                                translationsDownloaded++;
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error Downloading Translation {translation}: {ex.Message} ");
                    }
                }
                foreach (string translation in translations.Europe)
                {
                    try
                    {
                        var translationData = await httpClient.GetFromJsonAsync<Dictionary<string, string>>(StaticHelper.GetTranslationDownloadPath("Europe", translation)) ?? new Dictionary<string, string>();
                        if (translationData.Any())
                        {
                            var result = _translationHelper.SaveTranslation(translation, translationData);
                            if (result.Success)
                            {
                                translationsDownloaded++;
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error Downloading Translation {translation}: {ex.Message} ");
                    }
                }
                foreach (string translation in translations.NorthAmerica)
                {
                    try
                    {
                        var translationData = await httpClient.GetFromJsonAsync<Dictionary<string, string>>(StaticHelper.GetTranslationDownloadPath("NorthAmerica", translation)) ?? new Dictionary<string, string>();
                        if (translationData.Any())
                        {
                            var result = _translationHelper.SaveTranslation(translation, translationData);
                            if (result.Success)
                            {
                                translationsDownloaded++;
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error Downloading Translation {translation}: {ex.Message} ");
                    }
                }
                foreach (string translation in translations.SouthAmerica)
                {
                    try
                    {
                        var translationData = await httpClient.GetFromJsonAsync<Dictionary<string, string>>(StaticHelper.GetTranslationDownloadPath("SouthAmerica", translation)) ?? new Dictionary<string, string>();
                        if (translationData.Any())
                        {
                            var result = _translationHelper.SaveTranslation(translation, translationData);
                            if (result.Success)
                            {
                                translationsDownloaded++;
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error Downloading Translation {translation}: {ex.Message} ");
                    }
                }
                foreach (string translation in translations.Oceania)
                {
                    try
                    {
                        var translationData = await httpClient.GetFromJsonAsync<Dictionary<string, string>>(StaticHelper.GetTranslationDownloadPath("Oceania", translation)) ?? new Dictionary<string, string>();
                        if (translationData.Any())
                        {
                            var result = _translationHelper.SaveTranslation(translation, translationData);
                            if (result.Success)
                            {
                                translationsDownloaded++;
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error Downloading Translation {translation}: {ex.Message} ");
                    }
                }
                if (translationsDownloaded > 0)
                {
                    return Json(OperationResponse.Succeed($"{translationsDownloaded} Translations Downloaded"));
                } else
                {
                    return Json(OperationResponse.Failed("No Translations Downloaded"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unable to retrieve translations: {ex.Message}");
                return Json(OperationResponse.Failed());
            }
        }
        public ActionResult GetVehicleSelector(int vehicleId)
        {
            var vehiclesStored = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehiclesStored = _userLogic.FilterUserVehicles(vehiclesStored, GetUserID());
            }
            if (vehicleId != default)
            {
                vehiclesStored.RemoveAll(x => x.Id == vehicleId);
            }
            var userConfig = _config.GetUserConfig(User);
            if (userConfig.HideSoldVehicles)
            {
                vehiclesStored.RemoveAll(x => !string.IsNullOrWhiteSpace(x.SoldDate));
            }
            return PartialView("_VehicleSelector", vehiclesStored);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        public IActionResult GetCustomWidgetEditor()
        {
            if (_config.GetCustomWidgetsEnabled())
            {
                var customWidgetData = _fileHelper.GetWidgets();
                return PartialView("_WidgetEditor", customWidgetData);
            }
            return Json(string.Empty);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpPost]
        public IActionResult SaveCustomWidgets(string widgetsData)
        {
            if (_config.GetCustomWidgetsEnabled())
            {
                var saveResult = _fileHelper.SaveWidgets(widgetsData);
                return Json(saveResult);
            }
            return Json(false);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpPost]
        public IActionResult DeleteCustomWidgets()
        {
            if (_config.GetCustomWidgetsEnabled())
            {
                var deleteResult = _fileHelper.DeleteWidgets();
                return Json(deleteResult);
            }
            return Json(false);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        public IActionResult GetLocaleSample(string locale)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(locale);
            var viewModel = new LocaleSample
            {
                ShortDateSample = DateTime.Now.ToString(cultureInfo.DateTimeFormat.ShortDatePattern),
                CurrencySample = 13.45M.ToString("C", cultureInfo),
                NumberSample = 123456.ToString("N", cultureInfo),
                DecimalSample = 123456.78M.ToString("N2", cultureInfo)
            };
            return PartialView("_LocaleSample", viewModel);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [Route("/setup")]
        public IActionResult Setup()
        {
            var installedLocales = CultureInfo.GetCultures(CultureTypes.AllCultures).Select(x=>x.Name).ToList();
            installedLocales.RemoveAll(x => string.IsNullOrWhiteSpace(x));
            installedLocales.Insert(0, "");
            var viewModel = new ServerSettingsViewModel
            {
                LocaleOverride = _config.GetLocaleOverride(),
                AvailableLocales = installedLocales,
                PostgresConnection = _config.GetServerPostgresConnection(),
                AllowedFileExtensions = _config.GetAllowedFileUploadExtensions(),
                CustomLogoURL = _config.GetLogoUrl(),
                CustomSmallLogoURL = _config.GetSmallLogoUrl(),
                MessageOfTheDay = _config.GetMOTD(),
                WebHookURL = _config.GetWebHookUrl(),
                CustomWidgetsEnabled = _config.GetCustomWidgetsEnabled(),
                InvariantAPIEnabled = _config.GetInvariantApi(),
                SMTPConfig = _config.GetMailConfig(),
                Domain = _config.GetServerDomain(),
                OIDCConfig = _config.GetOpenIDConfig(),
                OpenRegistration = _config.GetServerOpenRegistration(),
                DisableRegistration = _config.GetServerDisabledRegistration(),
                ReminderUrgencyConfig = _config.GetReminderUrgencyConfig(),
                EnableAuth = _config.GetServerAuthEnabled(),
                DefaultReminderEmail = _config.GetDefaultReminderEmail(),
                EnableRootUserOIDC = _config.GetEnableRootUserOIDC()
            };
            return View(viewModel);
        }
        [HttpPost]
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        public IActionResult WriteServerConfiguration(ServerConfig serverConfig)
        {
            var result = _config.SaveServerConfig(serverConfig);
            return Json(result);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        public IActionResult SendTestEmail(string emailAddress, MailConfig mailConfig)
        {
            var result = _mailHelper.SendTestEmail(emailAddress, mailConfig);
            return Json(result);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

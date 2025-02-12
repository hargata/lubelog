﻿using CarCareTracker.Models;
using CsvHelper;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace CarCareTracker.Helper
{
    /// <summary>
    /// helper method for static vars
    /// </summary>
    public static class StaticHelper
    {
        public const string VersionNumber = "1.4.5";
        public const string DbName = "data/cartracker.db";
        public const string UserConfigPath = "data/config/userConfig.json";
        public const string LegacyUserConfigPath = "config/userConfig.json";
        public const string AdditionalWidgetsPath = "data/widgets.html";
        public const string GenericErrorMessage = "An error occurred, please try again later";
        public const string ReminderEmailTemplate = "defaults/reminderemailtemplate.txt";
        public const string DefaultAllowedFileExtensions = ".png,.jpg,.jpeg,.pdf,.xls,.xlsx,.docx";
        public const string SponsorsPath = "https://hargata.github.io/hargata/sponsors.json";
        public const string TranslationPath = "https://hargata.github.io/lubelog_translations";
        public const string TranslationDirectoryPath = $"{TranslationPath}/directory.json";
        public const string ReportNote = "Report generated by LubeLogger, a Free and Open Source Vehicle Maintenance Tracker - LubeLogger.com";
        public static string GetTitleCaseReminderUrgency(ReminderUrgency input)
        {
            switch (input)
            {
                case ReminderUrgency.NotUrgent:
                    return "Not Urgent";
                case ReminderUrgency.VeryUrgent:
                    return "Very Urgent";
                case ReminderUrgency.PastDue:
                    return "Past Due";
                default:
                    return input.ToString();
            }
        }
        public static string GetTitleCaseReminderUrgency(string input)
        {
            switch (input)
            {
                case "NotUrgent":
                    return "Not Urgent";
                case "VeryUrgent":
                    return "Very Urgent";
                case "PastDue":
                    return "Past Due";
                default:
                    return input;
            }
        }
        public static string GetReminderUrgencyColor(ReminderUrgency input)
        {
            switch (input)
            {
                case ReminderUrgency.NotUrgent:
                    return "text-bg-success";
                case ReminderUrgency.VeryUrgent:
                    return "text-bg-danger";
                case ReminderUrgency.PastDue:
                    return "text-bg-secondary";
                default:
                    return "text-bg-warning";
            }
        }

        public static string GetPlanRecordColor(PlanPriority input)
        {
            switch (input)
            {
                case PlanPriority.Critical:
                    return "text-bg-danger";
                case PlanPriority.Normal:
                    return "text-bg-primary";
                case PlanPriority.Low:
                    return "text-bg-info";
                default:
                    return "text-bg-primary";
            }
        }

        public static string GetPlanRecordProgress(PlanProgress input)
        {
            switch (input)
            {
                case PlanProgress.Backlog:
                    return "Planned";
                case PlanProgress.InProgress:
                    return "Doing";
                case PlanProgress.Testing:
                    return "Testing";
                case PlanProgress.Done:
                    return "Done";
                default:
                    return input.ToString();
            }
        }

        public static string TruncateStrings(string input, int maxLength = 25)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }
            if (input.Length > maxLength)
            {
                return (input.Substring(0, maxLength) + "...");
            }
            else
            {
                return input;
            }
        }
        public static string DefaultActiveTab(UserConfig userConfig, ImportMode tab)
        {
            var defaultTab = userConfig.DefaultTab;
            var visibleTabs = userConfig.VisibleTabs;
            if (visibleTabs.Contains(tab) && tab == defaultTab)
            {
                return "active";
            }
            else if (!visibleTabs.Contains(tab))
            {
                return "d-none";
            }
            return "";
        }
        public static string DefaultActiveTabContent(UserConfig userConfig, ImportMode tab)
        {
            var defaultTab = userConfig.DefaultTab;
            if (tab == defaultTab)
            {
                return "show active";
            }
            return "";
        }
        public static string DefaultTabSelected(UserConfig userConfig, ImportMode tab)
        {
            var defaultTab = userConfig.DefaultTab;
            var visibleTabs = userConfig.VisibleTabs;
            if (!visibleTabs.Contains(tab))
            {
                return "disabled";
            }
            else if (tab == defaultTab)
            {
                return "selected";
            }
            return "";
        }
        public static List<CostForVehicleByMonth> GetBaseLineCosts()
        {
            return new List<CostForVehicleByMonth>()
            {
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(1), MonthId = 1, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(2), MonthId = 2, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(3), MonthId = 3, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(4), MonthId = 4, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(5), MonthId = 5, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(6), MonthId = 6, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(7), MonthId = 7, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(8), MonthId = 8, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(9), MonthId = 9, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(10), MonthId = 10, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(11), MonthId = 11, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(12), MonthId = 12, Cost = 0M}
            };
        }
        public static List<CostForVehicleByMonth> GetBaseLineCostsNoMonthName()
        {
            return new List<CostForVehicleByMonth>()
            {
                new CostForVehicleByMonth { MonthId = 1, Cost = 0M},
                new CostForVehicleByMonth {MonthId = 2, Cost = 0M},
                new CostForVehicleByMonth {MonthId = 3, Cost = 0M},
                new CostForVehicleByMonth {MonthId = 4, Cost = 0M},
                new CostForVehicleByMonth {MonthId = 5, Cost = 0M},
                new CostForVehicleByMonth {MonthId = 6, Cost = 0M},
                new CostForVehicleByMonth {MonthId = 7, Cost = 0M},
                new CostForVehicleByMonth {MonthId = 8, Cost = 0M},
                new CostForVehicleByMonth {MonthId = 9, Cost = 0M},
                new CostForVehicleByMonth { MonthId = 10, Cost = 0M},
                new CostForVehicleByMonth { MonthId = 11, Cost = 0M},
                new CostForVehicleByMonth { MonthId = 12, Cost = 0M}
            };
        }
        public static List<string> GetBarChartColors()
        {
            return new List<string> { "#00876c", "#43956e", "#67a371", "#89b177", "#a9be80", "#c8cb8b", "#e6d79b", "#e4c281", "#e3ab6b", "#e2925b", "#e07952", "#db5d4f" };
        }

        public static ServiceRecord GenericToServiceRecord(GenericRecord input)
        {
            return new ServiceRecord
            {
                VehicleId = input.VehicleId,
                Date = input.Date,
                Description = input.Description,
                Cost = input.Cost,
                Mileage = input.Mileage,
                Files = input.Files,
                Notes = input.Notes,
                Tags = input.Tags,
                ExtraFields = input.ExtraFields,
                RequisitionHistory = input.RequisitionHistory
            };
        }
        public static CollisionRecord GenericToRepairRecord(GenericRecord input)
        {
            return new CollisionRecord
            {
                VehicleId = input.VehicleId,
                Date = input.Date,
                Description = input.Description,
                Cost = input.Cost,
                Mileage = input.Mileage,
                Files = input.Files,
                Notes = input.Notes,
                Tags = input.Tags,
                ExtraFields = input.ExtraFields,
                RequisitionHistory = input.RequisitionHistory
            };
        }
        public static UpgradeRecord GenericToUpgradeRecord(GenericRecord input)
        {
            return new UpgradeRecord
            {
                VehicleId = input.VehicleId,
                Date = input.Date,
                Description = input.Description,
                Cost = input.Cost,
                Mileage = input.Mileage,
                Files = input.Files,
                Notes = input.Notes,
                Tags = input.Tags,
                ExtraFields = input.ExtraFields,
                RequisitionHistory = input.RequisitionHistory
            };
        }

        public static List<ExtraField> AddExtraFields(List<ExtraField> recordExtraFields, List<ExtraField> templateExtraFields)
        {
            if (!templateExtraFields.Any())
            {
                return new List<ExtraField>();
            }
            if (!recordExtraFields.Any())
            {
                return templateExtraFields;
            }
            var fieldNames = templateExtraFields.Select(x => x.Name);
            //remove fields that are no longer present in template.
            recordExtraFields.RemoveAll(x => !fieldNames.Contains(x.Name));
            if (!recordExtraFields.Any())
            {
                return templateExtraFields;
            }
            var recordFieldNames = recordExtraFields.Select(x => x.Name);
            //update isrequired setting
            foreach (ExtraField extraField in recordExtraFields)
            {
                extraField.IsRequired = templateExtraFields.Where(x => x.Name == extraField.Name).First().IsRequired;
            }
            //append extra fields
            foreach (ExtraField extraField in templateExtraFields)
            {
                if (!recordFieldNames.Contains(extraField.Name))
                {
                    recordExtraFields.Add(extraField);
                }
            }
            //re-order extra fields
            recordExtraFields = recordExtraFields.OrderBy(x => templateExtraFields.FindIndex(y => y.Name == x.Name)).ToList();
            return recordExtraFields;
        }

        public static string GetFuelEconomyUnit(bool useKwh, bool useHours, bool useMPG, bool useUKMPG)
        {
            string fuelEconomyUnit;
            if (useKwh)
            {
                var distanceUnit = useHours ? "h" : (useMPG ? "mi." : "km");
                fuelEconomyUnit = useMPG ? $"{distanceUnit}/kWh" : $"kWh/100{distanceUnit}";
            }
            else if (useMPG && useUKMPG)
            {
                fuelEconomyUnit = useHours ? "h/g" : "mpg";
            }
            else if (useUKMPG)
            {
                fuelEconomyUnit = useHours ? "l/100h" : "l/100mi.";
            }
            else
            {
                fuelEconomyUnit = useHours ? (useMPG ? "h/g" : "l/100h") : (useMPG ? "mpg" : "l/100km");
            }
            return fuelEconomyUnit;
        }
        public static long GetEpochFromDateTime(DateTime date)
        {
            return new DateTimeOffset(date).ToUnixTimeMilliseconds();
        }
        public static long GetEpochFromDateTimeSeconds(DateTime date)
        {
            return new DateTimeOffset(date).ToUnixTimeSeconds();
        }
        public static void InitMessage(IConfiguration config)
        {
            Console.WriteLine($"LubeLogger {VersionNumber}");
            Console.WriteLine("Website: https://lubelogger.com");
            Console.WriteLine("Documentation: https://docs.lubelogger.com");
            Console.WriteLine("GitHub: https://github.com/hargata/lubelog");
            var mailConfig = config.GetSection("MailConfig").Get<MailConfig>();
            if (mailConfig != null && !string.IsNullOrWhiteSpace(mailConfig.EmailServer))
            {
                Console.WriteLine($"SMTP Configured for {mailConfig.EmailServer}");
            }
            else
            {
                Console.WriteLine("SMTP Not Configured");
            }
            var motd = config["LUBELOGGER_MOTD"] ?? "Not Configured";
            Console.WriteLine($"Message Of The Day: {motd}");
            if (string.IsNullOrWhiteSpace(CultureInfo.CurrentCulture.Name))
            {
                Console.WriteLine("WARNING: No Locale or Culture Configured for LubeLogger, Check Environment Variables");
            }
            //Create folders if they don't exist.
            if (!Directory.Exists("data"))
            {
                Directory.CreateDirectory("data");
                Console.WriteLine("Created data directory");
            }
            if (!Directory.Exists("data/images"))
            {
                Console.WriteLine("Created images directory");
                Directory.CreateDirectory("data/images");
            }
            if (!Directory.Exists("data/documents"))
            {
                Directory.CreateDirectory("data/documents");
                Console.WriteLine("Created documents directory");
            }
            if (!Directory.Exists("data/translations"))
            {
                Directory.CreateDirectory("data/translations");
                Console.WriteLine("Created translations directory");
            }
            if (!Directory.Exists("data/temp"))
            {
                Directory.CreateDirectory("data/temp");
                Console.WriteLine("Created translations directory");
            }
            if (!Directory.Exists("data/config"))
            {
                Directory.CreateDirectory("data/config");
                Console.WriteLine("Created config directory");
            }
        }
        public static void CheckMigration(string webRootPath, string webContentPath)
        {
            //check if current working directory differs from content root.
            if (Directory.GetCurrentDirectory() != webContentPath)
            {
                Console.WriteLine("WARNING: The Working Directory differs from the Web Content Path");
                Console.WriteLine($"Working Directory: {Directory.GetCurrentDirectory()}");
                Console.WriteLine($"Web Content Path: {webContentPath}");
            }
            //migrates all user-uploaded files from webroot to new data folder
            //images
            var imagePath = Path.Combine(webRootPath, "images");
            var docsPath = Path.Combine(webRootPath, "documents");
            var translationPath = Path.Combine(webRootPath, "translations");
            var tempPath = Path.Combine(webRootPath, "temp");
            if (File.Exists(LegacyUserConfigPath))
            {
                File.Move(LegacyUserConfigPath, UserConfigPath, true);
            }
            if (Directory.Exists(imagePath))
            {
                foreach (string fileToMove in Directory.GetFiles(imagePath))
                {
                    var newFilePath = $"data/images/{Path.GetFileName(fileToMove)}";
                    File.Move(fileToMove, newFilePath, true);
                    Console.WriteLine($"Migrated Image: {Path.GetFileName(fileToMove)}");
                }
            }
            if (Directory.Exists(docsPath))
            {
                foreach (string fileToMove in Directory.GetFiles(docsPath))
                {
                    var newFilePath = $"data/documents/{Path.GetFileName(fileToMove)}";
                    File.Move(fileToMove, newFilePath, true);
                    Console.WriteLine($"Migrated Document: {Path.GetFileName(fileToMove)}");
                }
            }
            if (Directory.Exists(translationPath))
            {
                foreach (string fileToMove in Directory.GetFiles(translationPath))
                {
                    var newFilePath = $"data/translations/{Path.GetFileName(fileToMove)}";
                    File.Move(fileToMove, newFilePath, true);
                    Console.WriteLine($"Migrated Translation: {Path.GetFileName(fileToMove)}");
                }
            }
            if (Directory.Exists(tempPath))
            {
                foreach (string fileToMove in Directory.GetFiles(tempPath))
                {
                    var newFilePath = $"data/temp/{Path.GetFileName(fileToMove)}";
                    File.Move(fileToMove, newFilePath, true);
                    Console.WriteLine($"Migrated Temp File: {Path.GetFileName(fileToMove)}");
                }
            }
        }
        public static async void NotifyAsync(string webhookURL, WebHookPayload webHookPayload)
        {
            if (string.IsNullOrWhiteSpace(webhookURL))
            {
                return;
            }
            var httpClient = new HttpClient();
            if (webhookURL.StartsWith("discord://"))
            {
                webhookURL = webhookURL.Replace("discord://", "https://"); //cleanurl
                //format to discord
                httpClient.PostAsJsonAsync(webhookURL, DiscordWebHook.FromWebHookPayload(webHookPayload));
            }
            else
            {
                httpClient.PostAsJsonAsync(webhookURL, webHookPayload);
            }
        }
        public static string GetImportModeIcon(ImportMode importMode)
        {
            switch (importMode)
            {
                case ImportMode.ServiceRecord:
                    return "bi-card-checklist";
                case ImportMode.RepairRecord:
                    return "bi-exclamation-octagon";
                case ImportMode.UpgradeRecord:
                    return "bi-wrench-adjustable";
                case ImportMode.TaxRecord:
                    return "bi-currency-dollar";
                case ImportMode.SupplyRecord:
                    return "bi-shop";
                case ImportMode.PlanRecord:
                    return "bi-bar-chart-steps";
                case ImportMode.OdometerRecord:
                    return "bi-speedometer";
                case ImportMode.GasRecord:
                    return "bi-fuel-pump";
                case ImportMode.NoteRecord:
                    return "bi-journal-bookmark";
                case ImportMode.ReminderRecord:
                    return "bi-bell";
                default:
                    return "bi-file-bar-graph";
            }
        }
        public static string GetVehicleIdentifier(Vehicle vehicle)
        {
            if (vehicle.VehicleIdentifier == "LicensePlate")
            {
                return vehicle.LicensePlate;
            }
            else
            {
                if (vehicle.ExtraFields.Any(x => x.Name == vehicle.VehicleIdentifier))
                {
                    return vehicle.ExtraFields?.FirstOrDefault(x => x.Name == vehicle.VehicleIdentifier)?.Value;
                }
                else
                {
                    return "N/A";
                }
            }
        }
        public static string GetVehicleIdentifier(VehicleViewModel vehicle)
        {
            if (vehicle.VehicleIdentifier == "LicensePlate")
            {
                return vehicle.LicensePlate;
            }
            else
            {
                if (vehicle.ExtraFields.Any(x => x.Name == vehicle.VehicleIdentifier))
                {
                    return vehicle.ExtraFields?.FirstOrDefault(x => x.Name == vehicle.VehicleIdentifier)?.Value;
                }
                else
                {
                    return "N/A";
                }
            }
        }
        //Translations
        public static string GetTranslationDownloadPath(string continent, string name)
        {
            if (string.IsNullOrWhiteSpace(continent) || string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }
            else
            {
                switch (continent)
                {
                    case "NorthAmerica":
                        continent = "North America";
                        break;
                    case "SouthAmerica":
                        continent = "South America";
                        break;
                }
                return $"{TranslationPath}/{continent}/{name}.json";
            }
        }
        public static string GetTranslationName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }
            else
            {
                try
                {
                    string cleanedName = name.Contains("_") ? name.Replace("_", "-") : name;
                    string displayName = CultureInfo.GetCultureInfo(cleanedName).DisplayName;
                    if (string.IsNullOrWhiteSpace(displayName))
                    {
                        return name;
                    }
                    else
                    {
                        return displayName;
                    }
                }
                catch (Exception ex)
                {
                    return name;
                }
            }
        }
        //CSV Write Methods
        public static void WriteGenericRecordExportModel(CsvWriter _csv, IEnumerable<GenericRecordExportModel> genericRecords)
        {
            var extraHeaders = genericRecords.SelectMany(x => x.ExtraFields).Select(y => y.Name).Distinct();
            //write headers
            _csv.WriteField(nameof(GenericRecordExportModel.Date));
            _csv.WriteField(nameof(GenericRecordExportModel.Description));
            _csv.WriteField(nameof(GenericRecordExportModel.Cost));
            _csv.WriteField(nameof(GenericRecordExportModel.Notes));
            _csv.WriteField(nameof(GenericRecordExportModel.Odometer));
            _csv.WriteField(nameof(GenericRecordExportModel.Tags));
            foreach (string extraHeader in extraHeaders)
            {
                _csv.WriteField($"extrafield_{extraHeader}");
            }
            _csv.NextRecord();
            foreach (GenericRecordExportModel genericRecord in genericRecords)
            {
                _csv.WriteField(genericRecord.Date);
                _csv.WriteField(genericRecord.Description);
                _csv.WriteField(genericRecord.Cost);
                _csv.WriteField(genericRecord.Notes);
                _csv.WriteField(genericRecord.Odometer);
                _csv.WriteField(genericRecord.Tags);
                foreach (string extraHeader in extraHeaders)
                {
                    var extraField = genericRecord.ExtraFields.Where(x => x.Name == extraHeader).FirstOrDefault();
                    _csv.WriteField(extraField != null ? extraField.Value : string.Empty);
                }
                _csv.NextRecord();
            }
        }
        public static void WriteOdometerRecordExportModel(CsvWriter _csv, IEnumerable<OdometerRecordExportModel> genericRecords)
        {
            var extraHeaders = genericRecords.SelectMany(x => x.ExtraFields).Select(y => y.Name).Distinct();
            //write headers
            _csv.WriteField(nameof(OdometerRecordExportModel.Date));
            _csv.WriteField(nameof(OdometerRecordExportModel.InitialOdometer));
            _csv.WriteField(nameof(OdometerRecordExportModel.Odometer));
            _csv.WriteField(nameof(OdometerRecordExportModel.Notes));
            _csv.WriteField(nameof(OdometerRecordExportModel.Tags));
            foreach (string extraHeader in extraHeaders)
            {
                _csv.WriteField($"extrafield_{extraHeader}");
            }
            _csv.NextRecord();
            foreach (OdometerRecordExportModel genericRecord in genericRecords)
            {
                _csv.WriteField(genericRecord.Date);
                _csv.WriteField(genericRecord.InitialOdometer);
                _csv.WriteField(genericRecord.Odometer);
                _csv.WriteField(genericRecord.Notes);
                _csv.WriteField(genericRecord.Tags);
                foreach (string extraHeader in extraHeaders)
                {
                    var extraField = genericRecord.ExtraFields.Where(x => x.Name == extraHeader).FirstOrDefault();
                    _csv.WriteField(extraField != null ? extraField.Value : string.Empty);
                }
                _csv.NextRecord();
            }
        }
        public static void WriteTaxRecordExportModel(CsvWriter _csv, IEnumerable<TaxRecordExportModel> genericRecords)
        {
            var extraHeaders = genericRecords.SelectMany(x => x.ExtraFields).Select(y => y.Name).Distinct();
            //write headers
            _csv.WriteField(nameof(TaxRecordExportModel.Date));
            _csv.WriteField(nameof(TaxRecordExportModel.Description));
            _csv.WriteField(nameof(TaxRecordExportModel.Cost));
            _csv.WriteField(nameof(TaxRecordExportModel.Notes));
            _csv.WriteField(nameof(TaxRecordExportModel.Tags));
            foreach (string extraHeader in extraHeaders)
            {
                _csv.WriteField($"extrafield_{extraHeader}");
            }
            _csv.NextRecord();
            foreach (TaxRecordExportModel genericRecord in genericRecords)
            {
                _csv.WriteField(genericRecord.Date);
                _csv.WriteField(genericRecord.Description);
                _csv.WriteField(genericRecord.Cost);
                _csv.WriteField(genericRecord.Notes);
                _csv.WriteField(genericRecord.Tags);
                foreach (string extraHeader in extraHeaders)
                {
                    var extraField = genericRecord.ExtraFields.Where(x => x.Name == extraHeader).FirstOrDefault();
                    _csv.WriteField(extraField != null ? extraField.Value : string.Empty);
                }
                _csv.NextRecord();
            }
        }
        public static void WriteSupplyRecordExportModel(CsvWriter _csv, IEnumerable<SupplyRecordExportModel> genericRecords)
        {
            var extraHeaders = genericRecords.SelectMany(x => x.ExtraFields).Select(y => y.Name).Distinct();
            //write headers
            _csv.WriteField(nameof(SupplyRecordExportModel.Date));
            _csv.WriteField(nameof(SupplyRecordExportModel.PartNumber));
            _csv.WriteField(nameof(SupplyRecordExportModel.PartSupplier));
            _csv.WriteField(nameof(SupplyRecordExportModel.PartQuantity));
            _csv.WriteField(nameof(SupplyRecordExportModel.Description));
            _csv.WriteField(nameof(SupplyRecordExportModel.Notes));
            _csv.WriteField(nameof(SupplyRecordExportModel.Cost));
            _csv.WriteField(nameof(SupplyRecordExportModel.Tags));
            foreach (string extraHeader in extraHeaders)
            {
                _csv.WriteField($"extrafield_{extraHeader}");
            }
            _csv.NextRecord();
            foreach (SupplyRecordExportModel genericRecord in genericRecords)
            {
                _csv.WriteField(genericRecord.Date);
                _csv.WriteField(genericRecord.PartNumber);
                _csv.WriteField(genericRecord.PartSupplier);
                _csv.WriteField(genericRecord.PartQuantity);
                _csv.WriteField(genericRecord.Description);
                _csv.WriteField(genericRecord.Notes);
                _csv.WriteField(genericRecord.Cost);
                _csv.WriteField(genericRecord.Tags);
                foreach (string extraHeader in extraHeaders)
                {
                    var extraField = genericRecord.ExtraFields.Where(x => x.Name == extraHeader).FirstOrDefault();
                    _csv.WriteField(extraField != null ? extraField.Value : string.Empty);
                }
                _csv.NextRecord();
            }
        }
        public static void WritePlanRecordExportModel(CsvWriter _csv, IEnumerable<PlanRecordExportModel> genericRecords)
        {
            var extraHeaders = genericRecords.SelectMany(x => x.ExtraFields).Select(y => y.Name).Distinct();
            //write headers
            _csv.WriteField(nameof(PlanRecordExportModel.DateCreated));
            _csv.WriteField(nameof(PlanRecordExportModel.DateModified));
            _csv.WriteField(nameof(PlanRecordExportModel.Description));
            _csv.WriteField(nameof(PlanRecordExportModel.Notes));
            _csv.WriteField(nameof(PlanRecordExportModel.Type));
            _csv.WriteField(nameof(PlanRecordExportModel.Priority));
            _csv.WriteField(nameof(PlanRecordExportModel.Progress));
            _csv.WriteField(nameof(PlanRecordExportModel.Cost));
            foreach (string extraHeader in extraHeaders)
            {
                _csv.WriteField($"extrafield_{extraHeader}");
            }
            _csv.NextRecord();
            foreach (PlanRecordExportModel genericRecord in genericRecords)
            {
                _csv.WriteField(genericRecord.DateCreated);
                _csv.WriteField(genericRecord.DateModified);
                _csv.WriteField(genericRecord.Description);
                _csv.WriteField(genericRecord.Notes);
                _csv.WriteField(genericRecord.Type);
                _csv.WriteField(genericRecord.Priority);
                _csv.WriteField(genericRecord.Progress);
                _csv.WriteField(genericRecord.Cost);
                foreach (string extraHeader in extraHeaders)
                {
                    var extraField = genericRecord.ExtraFields.Where(x => x.Name == extraHeader).FirstOrDefault();
                    _csv.WriteField(extraField != null ? extraField.Value : string.Empty);
                }
                _csv.NextRecord();
            }
        }
        public static string HideZeroCost(string input, bool hideZero, string decorations = "")
        {
            if (input == 0M.ToString("C2") && hideZero)
            {
                return "---";
            }
            else
            {
                return string.IsNullOrWhiteSpace(decorations) ? input : $"{input}{decorations}";
            }
        }
        public static string HideZeroCost(decimal input, bool hideZero, string decorations = "")
        {
            if (input == default && hideZero)
            {
                return "---";
            }
            else
            {
                return string.IsNullOrWhiteSpace(decorations) ? input.ToString("C2") : $"{input.ToString("C2")}{decorations}";
            }
        }
        public static JsonSerializerOptions GetInvariantOption()
        {
            var serializerOption = new JsonSerializerOptions();
            serializerOption.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            serializerOption.Converters.Add(new InvariantConverter());
            return serializerOption;
        }
        public static void WriteGasRecordExportModel(CsvWriter _csv, IEnumerable<GasRecordExportModel> genericRecords)
        {
            var extraHeaders = genericRecords.SelectMany(x => x.ExtraFields).Select(y => y.Name).Distinct();
            //write headers
            _csv.WriteField(nameof(GasRecordExportModel.Date));
            _csv.WriteField(nameof(GasRecordExportModel.Odometer));
            _csv.WriteField(nameof(GasRecordExportModel.FuelConsumed));
            _csv.WriteField(nameof(GasRecordExportModel.Cost));
            _csv.WriteField(nameof(GasRecordExportModel.FuelEconomy));
            _csv.WriteField(nameof(GasRecordExportModel.IsFillToFull));
            _csv.WriteField(nameof(GasRecordExportModel.MissedFuelUp));
            _csv.WriteField(nameof(GasRecordExportModel.Notes));
            _csv.WriteField(nameof(GasRecordExportModel.Tags));
            foreach (string extraHeader in extraHeaders)
            {
                _csv.WriteField($"extrafield_{extraHeader}");
            }
            _csv.NextRecord();
            foreach (GasRecordExportModel genericRecord in genericRecords)
            {
                _csv.WriteField(genericRecord.Date);
                _csv.WriteField(genericRecord.Odometer);
                _csv.WriteField(genericRecord.FuelConsumed);
                _csv.WriteField(genericRecord.Cost);
                _csv.WriteField(genericRecord.FuelEconomy);
                _csv.WriteField(genericRecord.IsFillToFull);
                _csv.WriteField(genericRecord.MissedFuelUp);
                _csv.WriteField(genericRecord.Notes);
                _csv.WriteField(genericRecord.Tags);
                foreach (string extraHeader in extraHeaders)
                {
                    var extraField = genericRecord.ExtraFields.Where(x => x.Name == extraHeader).FirstOrDefault();
                    _csv.WriteField(extraField != null ? extraField.Value : string.Empty);
                }
                _csv.NextRecord();
            }
        }
        public static byte[] RemindersToCalendar(List<ReminderRecordViewModel> reminders)
        {
            //converts reminders to iCal file
            StringBuilder sb = new StringBuilder();
            //start the calendar item
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:lubelogger.com");
            sb.AppendLine("CALSCALE:GREGORIAN");
            sb.AppendLine("METHOD:PUBLISH");

            //create events.
            foreach(ReminderRecordViewModel reminder in reminders)
            {
                var dtStart = reminder.Date.Date.ToString("yyyyMMddTHHmm00");
                var dtEnd = reminder.Date.Date.AddDays(1).AddMilliseconds(-1).ToString("yyyyMMddTHHmm00");
                var calendarUID = new Guid(MD5.HashData(Encoding.UTF8.GetBytes($"{dtStart}_{reminder.Description}")));
                sb.AppendLine("BEGIN:VEVENT");
                sb.AppendLine("DTSTAMP:" + DateTime.Now.ToString("yyyyMMddTHHmm00"));
                sb.AppendLine("UID:" + calendarUID);
                sb.AppendLine("DTSTART:" + dtStart);
                sb.AppendLine("DTEND:" + dtEnd);
                sb.AppendLine($"SUMMARY:{reminder.Description}");
                sb.AppendLine($"DESCRIPTION:{reminder.Description}");
                switch (reminder.Urgency)
                {
                    case ReminderUrgency.NotUrgent:
                        sb.AppendLine("PRIORITY:3");
                        break;
                    case ReminderUrgency.Urgent:
                        sb.AppendLine("PRIORITY:2");
                        break;
                    case ReminderUrgency.VeryUrgent:
                        sb.AppendLine("PRIORITY:1");
                        break;
                    case ReminderUrgency.PastDue:
                        sb.AppendLine("PRIORITY:1");
                        break;
                }
                sb.AppendLine("END:VEVENT");
            }

            //end calendar item
            sb.AppendLine("END:VCALENDAR");
            string calendarContent = sb.ToString();
            return Encoding.UTF8.GetBytes(calendarContent);
        }
    }
}

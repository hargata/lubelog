using CarCareTracker.External.Interfaces;
using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Logic;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace CarCareTracker.Controllers
{
    [Authorize]
    public partial class APIController : Controller
    {
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
        private readonly IUserAccessDataAccess _userAccessDataAccess;
        private readonly IUserRecordDataAccess _userRecordDataAccess;
        private readonly IExtraFieldDataAccess _extraFieldDataAccess;
        private readonly IReminderHelper _reminderHelper;
        private readonly IEquipmentHelper _equipmentHelper;
        private readonly IGasHelper _gasHelper;
        private readonly IUserLogic _userLogic;
        private readonly IVehicleLogic _vehicleLogic;
        private readonly IOdometerLogic _odometerLogic;
        private readonly IFileHelper _fileHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IConfigHelper _config;
        private readonly IWebHostEnvironment _webEnv;
        private readonly IHttpClientFactory _httpClientFactory;
        public APIController(IVehicleDataAccess dataAccess,
            IGasHelper gasHelper,
            IEquipmentHelper equipmentHelper,
            IReminderHelper reminderHelper,
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
            IUserAccessDataAccess userAccessDataAccess,
            IUserRecordDataAccess userRecordDataAccess,
            IExtraFieldDataAccess extraFieldDataAccess,
            IMailHelper mailHelper,
            IFileHelper fileHelper,
            IConfigHelper config,
            IUserLogic userLogic,
            IVehicleLogic vehicleLogic,
            IOdometerLogic odometerLogic,
            IWebHostEnvironment webEnv,
            IHttpClientFactory httpClientFactory)
        {
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
            _userAccessDataAccess = userAccessDataAccess;
            _userRecordDataAccess = userRecordDataAccess;
            _extraFieldDataAccess = extraFieldDataAccess;
            _mailHelper = mailHelper;
            _gasHelper = gasHelper;
            _equipmentHelper = equipmentHelper;
            _reminderHelper = reminderHelper;
            _userLogic = userLogic;
            _odometerLogic = odometerLogic;
            _vehicleLogic = vehicleLogic;
            _fileHelper = fileHelper;
            _config = config;
            _webEnv = webEnv;
            _httpClientFactory = httpClientFactory;
        }
        public IActionResult Index()
        {
            //load up documentation
            var apiDocFilePath = _fileHelper.GetFullFilePath("/defaults/api.json");
            var apiDocText = _fileHelper.GetFileText(apiDocFilePath);
            var apiDocData = JsonSerializer.Deserialize<List<APIDocumentation>>(apiDocText) ?? new List<APIDocumentation>();
            var apiSerializeOptions = StaticHelper.GetNoEncodingOption();
            apiSerializeOptions.WriteIndented = true;
            foreach(APIDocumentation apiDocumentation in apiDocData)
            {
                foreach(APIMethod apiMethod in apiDocumentation.Methods)
                {
                    if (apiMethod.HasBody)
                    {
                        apiMethod.BodySampleString = JsonSerializer.Serialize(apiMethod.BodySample, apiSerializeOptions);
                    }
                }
            }
            return View(apiDocData);
        }
        private int GetUserID()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
        [HttpGet]
        [Route("/api/whoami")]
        public IActionResult WhoAmI()
        {
            var result = new UserExportModel
            {
                Username = User.FindFirstValue(ClaimTypes.Name),
                EmailAddress = User.IsInRole(nameof(UserData.IsRootUser)) ? _config.GetDefaultReminderEmail() : User.FindFirstValue(ClaimTypes.Email),
                IsAdmin = User.IsInRole(nameof(UserData.IsAdmin)).ToString(),
                IsRoot = User.IsInRole(nameof(UserData.IsRootUser)).ToString()
            };
            if (_config.GetInvariantApi() || Request.Headers.ContainsKey("culture-invariant"))
            {
                return Json(result, StaticHelper.GetInvariantOption());
            }
            else
            {
                return Json(result);
            }
        }
        [HttpGet]
        [Route("/api/version")]
        public async Task<IActionResult> ServerVersion(bool checkForUpdate = false)
        {
            var viewModel = new ReleaseVersion
            {
                CurrentVersion = StaticHelper.VersionNumber,
                LatestVersion = StaticHelper.VersionNumber
            };
            if (checkForUpdate)
            {
                try
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
                    var releaseResponse = await httpClient.GetFromJsonAsync<ReleaseResponse>(StaticHelper.ReleasePath) ?? new ReleaseResponse();
                    if (!string.IsNullOrWhiteSpace(releaseResponse.tag_name))
                    {
                        viewModel.LatestVersion = releaseResponse.tag_name;
                    }
                }
                catch (Exception ex)
                {
                    return Json(OperationResponse.Failed($"Unable to retrieve latest version from GitHub API: {ex.Message}"));
                }
            }
            return Json(viewModel);
        }
        [HttpGet]
        [Route("/api/vehicles")]
        public IActionResult Vehicles()
        {
            var result = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                result = _userLogic.FilterUserVehicles(result, GetUserID());
            }
            if (_config.GetInvariantApi() || Request.Headers.ContainsKey("culture-invariant"))
            {
                return Json(result, StaticHelper.GetInvariantOption());
            }
            else
            {
                return Json(result);
            }
        }

        [HttpGet]
        [Route("/api/vehicle/info")]
        public IActionResult VehicleInfo(int vehicleId)
        {
            //stats for a specific or all vehicles
            List<Vehicle> vehicles = new List<Vehicle>();
            if (vehicleId != default)
            {
                if (_userLogic.UserCanEditVehicle(GetUserID(), vehicleId, HouseholdPermission.View))
                {
                    vehicles.Add(_dataAccess.GetVehicleById(vehicleId));
                } else
                {
                    return new RedirectResult("/Error/Unauthorized");
                }
            } else
            {
                var result = _dataAccess.GetVehicles();
                if (!User.IsInRole(nameof(UserData.IsRootUser)))
                {
                    result = _userLogic.FilterUserVehicles(result, GetUserID());
                }
                vehicles.AddRange(result);
            }

            var apiResult = _vehicleLogic.GetVehicleInfo(vehicles);
            if (_config.GetInvariantApi() || Request.Headers.ContainsKey("culture-invariant"))
            {
                return Json(apiResult, StaticHelper.GetInvariantOption());
            }
            else
            {
                return Json(apiResult);
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/adjustedodometer")]
        public IActionResult AdjustedOdometer(int vehicleId, int odometer)
        {
            var vehicle = _dataAccess.GetVehicleById(vehicleId);
            if (vehicle == null || !vehicle.HasOdometerAdjustment)
            {
                return Json(odometer);
            } else
            {
                var convertedOdometer = (odometer + int.Parse(vehicle.OdometerDifference)) * decimal.Parse(vehicle.OdometerMultiplier);
                return Json(convertedOdometer);
            }
        }
        [HttpPost]
        [Route("/api/vehicles/add")]
        [Consumes("application/json")]
        public IActionResult AddVehicleJson([FromBody] VehicleImportModel input) => AddVehicle(input);
        [HttpPost]
        [Route("/api/vehicles/add")]
        public IActionResult AddVehicle(VehicleImportModel input)
        {
            //validation
            if (string.IsNullOrWhiteSpace(input.Year) ||
                string.IsNullOrWhiteSpace(input.Make) ||
                string.IsNullOrWhiteSpace(input.Model) ||
                string.IsNullOrWhiteSpace(input.Identifier) ||
                string.IsNullOrWhiteSpace(input.FuelType))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Year, Make, Model, Identifier, and FuelType cannot be empty."));
            }
            if (input.Identifier == "LicensePlate" && string.IsNullOrWhiteSpace(input.LicensePlate))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, LicensePlate cannot be empty."));
            }
            if (input.ExtraFields == null)
            {
                input.ExtraFields = new List<ExtraField>();
            }
            if (input.Identifier != "LicensePlate" && string.IsNullOrWhiteSpace(input.ExtraFields.FirstOrDefault(x=>x.Name == input.Identifier)?.Value))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed($"Input object invalid, Identifier {input.Identifier} is specified but the value is not found in extra fields."));
            }
            var validFuelTypes = new List<string> { "Gasoline", "Diesel", "Electric" };
            if (!validFuelTypes.Contains(input.FuelType))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Fuel Type must be either Gasoline, Diesel, or Eletric"));
            }
            try
            {
                var vehicle = new Vehicle()
                {
                    Year = int.Parse(input.Year),
                    Make = input.Make,
                    Model = input.Model,
                    LicensePlate = input.LicensePlate,
                    VehicleIdentifier = input.Identifier,
                    UseHours = string.IsNullOrWhiteSpace(input.UseEngineHours) ? false : bool.Parse(input.UseEngineHours),
                    OdometerOptional = string.IsNullOrWhiteSpace(input.OdometerOptional) ? false : bool.Parse(input.OdometerOptional),
                    ExtraFields = input.ExtraFields,
                    Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList()
                };
                switch (input.FuelType)
                {
                    case "Diesel":
                        vehicle.IsDiesel = true;
                        break;
                    case "Electric":
                        vehicle.IsElectric = true;
                        break;
                }
                _dataAccess.SaveVehicle(vehicle);
                if (vehicle.Id != default)
                {
                    _userLogic.AddUserAccessToVehicle(GetUserID(), vehicle.Id);
                }
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.Generic($"Created Vehicle {vehicle.Year} {vehicle.Make} {vehicle.Model}({StaticHelper.GetVehicleIdentifier(vehicle)}) via API", "vehicle.add.api", User.Identity?.Name ?? string.Empty, vehicle.Id.ToString()));
                return Json(OperationResponse.Succeed("Vehicle Added", new { vehicleId = vehicle.Id }));
            } catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicles/update")]
        [Consumes("application/json")]
        public IActionResult UpdateVehicleJson([FromBody] VehicleImportModel input) => UpdateVehicle(input);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicles/update")]
        public IActionResult UpdateVehicle(VehicleImportModel input)
        {
            //validation
            if (string.IsNullOrWhiteSpace(input.Id) ||
                string.IsNullOrWhiteSpace(input.Year) ||
                string.IsNullOrWhiteSpace(input.Make) ||
                string.IsNullOrWhiteSpace(input.Model) ||
                string.IsNullOrWhiteSpace(input.Identifier) ||
                string.IsNullOrWhiteSpace(input.FuelType))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Id, Year, Make, Model, Identifier, and FuelType cannot be empty."));
            }
            if (input.Identifier == "LicensePlate" && string.IsNullOrWhiteSpace(input.LicensePlate))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, LicensePlate cannot be empty."));
            }
            if (input.ExtraFields == null)
            {
                input.ExtraFields = new List<ExtraField>();
            }
            if (input.Identifier != "LicensePlate" && string.IsNullOrWhiteSpace(input.ExtraFields.FirstOrDefault(x => x.Name == input.Identifier)?.Value))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed($"Input object invalid, Identifier {input.Identifier} is specified but the value is not found in extra fields."));
            }
            var validFuelTypes = new List<string> { "Gasoline", "Diesel", "Electric" };
            if (!validFuelTypes.Contains(input.FuelType))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Fuel Type must be either Gasoline, Diesel, or Eletric"));
            }
            try
            {
                var existingVehicle = _dataAccess.GetVehicleById(int.Parse(input.Id));
                if (existingVehicle != null && existingVehicle.Id == int.Parse(input.Id))
                {
                    if (!_userLogic.UserCanEditVehicle(GetUserID(), existingVehicle.Id, HouseholdPermission.Edit))
                    {
                        Response.StatusCode = 401;
                        return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
                    }
                    existingVehicle.Year = int.Parse(input.Year);
                    existingVehicle.Make = input.Make;
                    existingVehicle.Model = input.Model;
                    existingVehicle.LicensePlate = input.LicensePlate;
                    existingVehicle.VehicleIdentifier = input.Identifier;
                    existingVehicle.UseHours = string.IsNullOrWhiteSpace(input.UseEngineHours) ? false : bool.Parse(input.UseEngineHours);
                    existingVehicle.OdometerOptional = string.IsNullOrWhiteSpace(input.OdometerOptional) ? false : bool.Parse(input.OdometerOptional);
                    existingVehicle.ExtraFields = input.ExtraFields;
                    existingVehicle.Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList();
                    switch (input.FuelType)
                    {
                        case "Diesel":
                            existingVehicle.IsDiesel = true;
                            break;
                        case "Electric":
                            existingVehicle.IsElectric = true;
                            break;
                    }
                    _dataAccess.SaveVehicle(existingVehicle);
                    StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.Generic($"Updated Vehicle {existingVehicle.Year} {existingVehicle.Make} {existingVehicle.Model}({StaticHelper.GetVehicleIdentifier(existingVehicle)}) via API", "vehicle.update.api", User.Identity?.Name ?? string.Empty, existingVehicle.Id.ToString()));
                    return Json(OperationResponse.Succeed("Vehicle Updated"));
                }
                else
                {
                    Response.StatusCode = 400;
                    return Json(OperationResponse.Failed("Invalid Vehicle Id"));
                }
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [HttpPost]
        [Route("/api/documents/upload")]
        public IActionResult UploadDocument(List<IFormFile> documents)
        {
            if (documents.Any())
            {
                List<UploadedFiles> uploadedFiles = new List<UploadedFiles>();
                string uploadDirectory = "documents/";
                string uploadPath = Path.Combine(_webEnv.ContentRootPath, "data", uploadDirectory);
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);
                foreach (IFormFile document in documents)
                {
                    string fileName = Guid.NewGuid() + Path.GetExtension(document.FileName);
                    string filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        document.CopyTo(stream);
                    }
                    uploadedFiles.Add(new UploadedFiles
                    {
                        Location = Path.Combine("/", uploadDirectory, fileName),
                        Name = Path.GetFileName(document.FileName)
                    });
                }
                return Json(uploadedFiles);
            } else
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("No files to upload"));
            }
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        [Route("/api/vehicle/reminders/send")]
        public IActionResult SendReminders(ReminderMethodParameter parameters)
        {
            if (parameters.Urgencies == null || !parameters.Urgencies.Any())
            {
                //if no urgencies parameter, we will default to all urgencies.
                parameters.Urgencies = new List<ReminderUrgency> { ReminderUrgency.NotUrgent, ReminderUrgency.Urgent, ReminderUrgency.VeryUrgent, ReminderUrgency.PastDue };
            }
            List<OperationResponse> operationResponses = new List<OperationResponse>();
            var defaultEmailAddress = _config.GetDefaultReminderEmail();
            List<string> tagsFilter = !string.IsNullOrWhiteSpace(parameters.Tags) ? parameters.Tags.Split(' ').Distinct().ToList() : new List<string>();
            if (parameters.Id != default)
            {
                //if reminderId is provided, then we only send email out for that specific reminder
                var reminder = _reminderRecordDataAccess.GetReminderRecordById(parameters.Id);
                if (reminder == null || reminder.Id != parameters.Id)
                {
                    return Json(OperationResponse.Failed("No Emails Sent, No Reminders Matching Parameters"));
                }
                var vehicleId = reminder.VehicleId;
                var vehicle = _dataAccess.GetVehicleById(vehicleId);
                var currentMileage = _vehicleLogic.GetMaxMileage(vehicleId);
                var reminders = new List<ReminderRecord> { reminder }; //convert  to list
                var results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, DateTime.Now).OrderByDescending(x => x.Urgency).ToList();
                results.RemoveAll(x => !parameters.Urgencies.Contains(x.Urgency));
                if (tagsFilter.Any())
                {
                    results.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
                }
                if (!results.Any())
                {
                    return Json(OperationResponse.Failed("No Emails Sent, No Reminders Matching Parameters"));
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
                    return Json(OperationResponse.Failed("No Emails Sent, No Recipients Configured"));
                }
                var result = _mailHelper.NotifyUserForReminders(vehicle, emailRecipients, results);
                operationResponses.Add(result);
            } 
            else
            {
                var vehicles = _dataAccess.GetVehicles();
                foreach (Vehicle vehicle in vehicles)
                {
                    var vehicleId = vehicle.Id;
                    //get reminders
                    var currentMileage = _vehicleLogic.GetMaxMileage(vehicleId);
                    var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
                    var results = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, DateTime.Now).OrderByDescending(x => x.Urgency).ToList();
                    results.RemoveAll(x => !parameters.Urgencies.Contains(x.Urgency));
                    if (tagsFilter.Any())
                    {
                        results.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
                    }
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
            }
            if (!operationResponses.Any())
            {
                return Json(OperationResponse.Failed("No Emails Sent, No Vehicles Available or No Recipients Configured"));
            }
            else if (operationResponses.All(x => x.Success))
            {
                return Json(OperationResponse.Succeed($"Emails Sent({operationResponses.Count()})"));
            } else if (operationResponses.All(x => !x.Success))
            {
                return Json(OperationResponse.Failed($"All Emails Failed({operationResponses.Count()}), Check SMTP Settings"));
            } else
            {
                return Json(OperationResponse.Succeed($"Emails Sent({operationResponses.Count(x => x.Success)}), Emails Failed({operationResponses.Count(x => !x.Success)}), Check Recipient Settings"));
            }
        }
        [HttpGet]
        [Route("/api/extrafields")]
        public IActionResult GetExtraFields()
        {
            try
            {
                List<RecordExtraFieldExportModel> result = new List<RecordExtraFieldExportModel>();
                var extraFields = _extraFieldDataAccess.GetExtraFields();
                if (extraFields.Any())
                {
                    foreach(RecordExtraField extraField in extraFields)
                    {
                        if (extraField.ExtraFields.Any())
                        {
                            result.Add(new RecordExtraFieldExportModel
                            {
                                RecordType = ((ImportMode)extraField.Id).ToString(),
                                ExtraFields = extraField.ExtraFields.Select(x => new ExtraFieldExportModel { Name = x.Name, IsRequired = x.IsRequired.ToString(), FieldType = x.FieldType.ToString() }).ToList()
                            });
                        }
                    }
                }
                if (_config.GetInvariantApi() || Request.Headers.ContainsKey("culture-invariant"))
                {
                    return Json(result, StaticHelper.GetInvariantOption());
                }
                else
                {
                    return Json(result);
                }
            } catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        [Route("/api/makebackup")]
        public IActionResult MakeBackup()
        {
            var result = _fileHelper.MakeBackup();
            return Json(result);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        [Route("/api/tempfiles")]
        public IActionResult GetTempFiles()
        {
            var tempFiles = _fileHelper.GetTempFiles();
            return Json(tempFiles);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        [Route("/api/cleanup")]
        public IActionResult CleanUp(bool deepClean = false)
        {
            var jsonResponse = new Dictionary<string, string>();
            //Clear out temp folder
            var tempFilesDeleted = _fileHelper.ClearTempFolder();
            jsonResponse.Add("temp_files_deleted", tempFilesDeleted.ToString());
            if (deepClean)
            {
                //clear out unused vehicle thumbnails
                var vehicles = _dataAccess.GetVehicles();
                var vehicleImages = vehicles.Select(x => x.ImageLocation).Where(x => x.StartsWith("/images/")).Select(x=>Path.GetFileName(x)).ToList();
                if (vehicleImages.Any())
                {
                    var thumbnailsDeleted = _fileHelper.ClearUnlinkedThumbnails(vehicleImages);
                    jsonResponse.Add("unlinked_thumbnails_deleted", thumbnailsDeleted.ToString());
                }
                var vehicleDocuments = new List<string>();
                foreach(Vehicle vehicle in vehicles)
                {
                    if (!string.IsNullOrWhiteSpace(vehicle.MapLocation))
                    {
                        vehicleDocuments.Add(Path.GetFileName(vehicle.MapLocation));
                    }
                    vehicleDocuments.AddRange(_serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicle.Id).SelectMany(x => x.Files).Select(y=>Path.GetFileName(y.Location)));
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
                    jsonResponse.Add("unlinked_documents_deleted", documentsDeleted.ToString());
                }
            }
            return Json(jsonResponse);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        [Route("/api/demo/restore")]
        public IActionResult RestoreDemo()
        {
            var result = _fileHelper.RestoreBackup("/defaults/demo_default.zip", true);
            return Json(result);
        }
    }
}

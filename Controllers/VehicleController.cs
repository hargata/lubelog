using CarCareTracker.External.Interfaces;
using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Logic;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace CarCareTracker.Controllers
{
    [Authorize]
    public partial class VehicleController : Controller
    {
        private readonly ILogger<VehicleController> _logger;
        private readonly IVehicleDataAccess _dataAccess;
        private readonly INoteDataAccess _noteDataAccess;
        private readonly IServiceRecordDataAccess _serviceRecordDataAccess;
        private readonly IGasRecordDataAccess _gasRecordDataAccess;
        private readonly ICollisionRecordDataAccess _collisionRecordDataAccess;
        private readonly ITaxRecordDataAccess _taxRecordDataAccess;
        private readonly IReminderRecordDataAccess _reminderRecordDataAccess;
        private readonly IUpgradeRecordDataAccess _upgradeRecordDataAccess;
        private readonly ISupplyRecordDataAccess _supplyRecordDataAccess;
        private readonly IPlanRecordDataAccess _planRecordDataAccess;
        private readonly IPlanRecordTemplateDataAccess _planRecordTemplateDataAccess;
        private readonly IOdometerRecordDataAccess _odometerRecordDataAccess;
        private readonly IInspectionRecordDataAccess _inspectionRecordDataAccess;
        private readonly IInspectionRecordTemplateDataAccess _inspectionRecordTemplateDataAccess;
        private readonly IWebHostEnvironment _webEnv;
        private readonly IConfigHelper _config;
        private readonly IFileHelper _fileHelper;
        private readonly IGasHelper _gasHelper;
        private readonly IReminderHelper _reminderHelper;
        private readonly IReportHelper _reportHelper;
        private readonly IUserLogic _userLogic;
        private readonly IOdometerLogic _odometerLogic;
        private readonly IVehicleLogic _vehicleLogic;
        private readonly IExtraFieldDataAccess _extraFieldDataAccess;

        public VehicleController(ILogger<VehicleController> logger,
            IFileHelper fileHelper,
            IGasHelper gasHelper,
            IReminderHelper reminderHelper,
            IReportHelper reportHelper,
            IVehicleDataAccess dataAccess,
            INoteDataAccess noteDataAccess,
            IServiceRecordDataAccess serviceRecordDataAccess,
            IGasRecordDataAccess gasRecordDataAccess,
            ICollisionRecordDataAccess collisionRecordDataAccess,
            ITaxRecordDataAccess taxRecordDataAccess,
            IReminderRecordDataAccess reminderRecordDataAccess,
            IUpgradeRecordDataAccess upgradeRecordDataAccess,
            ISupplyRecordDataAccess supplyRecordDataAccess,
            IPlanRecordDataAccess planRecordDataAccess,
            IPlanRecordTemplateDataAccess planRecordTemplateDataAccess,
            IOdometerRecordDataAccess odometerRecordDataAccess,
            IExtraFieldDataAccess extraFieldDataAccess,
            IUserLogic userLogic,
            IOdometerLogic odometerLogic,
            IVehicleLogic vehicleLogic,
            IWebHostEnvironment webEnv,
            IConfigHelper config,
            IInspectionRecordDataAccess inspectionRecordDataAccess,
            IInspectionRecordTemplateDataAccess inspectionRecordTemplateDataAccess)
        {
            _logger = logger;
            _dataAccess = dataAccess;
            _noteDataAccess = noteDataAccess;
            _fileHelper = fileHelper;
            _gasHelper = gasHelper;
            _reminderHelper = reminderHelper;
            _reportHelper = reportHelper;
            _serviceRecordDataAccess = serviceRecordDataAccess;
            _gasRecordDataAccess = gasRecordDataAccess;
            _collisionRecordDataAccess = collisionRecordDataAccess;
            _taxRecordDataAccess = taxRecordDataAccess;
            _reminderRecordDataAccess = reminderRecordDataAccess;
            _upgradeRecordDataAccess = upgradeRecordDataAccess;
            _supplyRecordDataAccess = supplyRecordDataAccess;
            _planRecordDataAccess = planRecordDataAccess;
            _planRecordTemplateDataAccess = planRecordTemplateDataAccess;
            _inspectionRecordDataAccess = inspectionRecordDataAccess;
            _inspectionRecordTemplateDataAccess = inspectionRecordTemplateDataAccess;
            _odometerRecordDataAccess = odometerRecordDataAccess;
            _extraFieldDataAccess = extraFieldDataAccess;
            _userLogic = userLogic;
            _odometerLogic = odometerLogic;
            _vehicleLogic = vehicleLogic;
            _webEnv = webEnv;
            _config = config;
        }
        private int GetUserID()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult Index(int vehicleId)
        {
            var data = _dataAccess.GetVehicleById(vehicleId);
            return View(data);
        }
        [HttpGet]
        public IActionResult AddVehiclePartialView()
        {
            return PartialView("_VehicleModal", new Vehicle() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.VehicleRecord).ExtraFields });
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetEditVehiclePartialViewById(int vehicleId)
        {
            var data = _dataAccess.GetVehicleById(vehicleId);
            data.ExtraFields = StaticHelper.AddExtraFields(data.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.VehicleRecord).ExtraFields);
            return PartialView("_VehicleModal", data);
        }
        [HttpPost]
        public IActionResult SaveVehicle(Vehicle vehicleInput)
        {
            try
            {
                bool isNewAddition = vehicleInput.Id == default;
                if (!isNewAddition)
                {
                    if (!_userLogic.UserCanEditVehicle(GetUserID(), vehicleInput.Id))
                    {
                        return View("401");
                    }
                }
                //move image from temp folder to images folder.
                vehicleInput.ImageLocation = _fileHelper.MoveFileFromTemp(vehicleInput.ImageLocation, "images/");
                vehicleInput.MapLocation = _fileHelper.MoveFileFromTemp(vehicleInput.MapLocation, "documents/");
                //save vehicle.
                var result = _dataAccess.SaveVehicle(vehicleInput);
                if (isNewAddition)
                {
                    _userLogic.AddUserAccessToVehicle(GetUserID(), vehicleInput.Id);
                    StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.Generic($"Created Vehicle {vehicleInput.Year} {vehicleInput.Make} {vehicleInput.Model}({StaticHelper.GetVehicleIdentifier(vehicleInput)})", "vehicle.add", User.Identity.Name, vehicleInput.Id.ToString()));
                }
                else
                {
                    StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.Generic($"Updated Vehicle {vehicleInput.Year} {vehicleInput.Make} {vehicleInput.Model}({StaticHelper.GetVehicleIdentifier(vehicleInput)})", "vehicle.update", User.Identity.Name, vehicleInput.Id.ToString()));
                }
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Saving Vehicle");
                return Json(false);
            }
        }
        [HttpPost]
        [TypeFilter(typeof(StrictCollaboratorFilter), Arguments = new object[] { false, true })]
        public IActionResult DeleteVehicle(int vehicleId)
        {
            //Delete all service records, gas records, notes, etc.
            var result = _gasRecordDataAccess.DeleteAllGasRecordsByVehicleId(vehicleId) &&
                _serviceRecordDataAccess.DeleteAllServiceRecordsByVehicleId(vehicleId) &&
                _collisionRecordDataAccess.DeleteAllCollisionRecordsByVehicleId(vehicleId) &&
                _taxRecordDataAccess.DeleteAllTaxRecordsByVehicleId(vehicleId) &&
                _noteDataAccess.DeleteAllNotesByVehicleId(vehicleId) &&
                _reminderRecordDataAccess.DeleteAllReminderRecordsByVehicleId(vehicleId) &&
                _upgradeRecordDataAccess.DeleteAllUpgradeRecordsByVehicleId(vehicleId) &&
                _planRecordDataAccess.DeleteAllPlanRecordsByVehicleId(vehicleId) &&
                _planRecordTemplateDataAccess.DeleteAllPlanRecordTemplatesByVehicleId(vehicleId) &&
                _inspectionRecordDataAccess.DeleteAllInspectionRecordsByVehicleId(vehicleId) &&
                _inspectionRecordTemplateDataAccess.DeleteAllInspectionReportTemplatesByVehicleId(vehicleId) &&
                _supplyRecordDataAccess.DeleteAllSupplyRecordsByVehicleId(vehicleId) &&
                _odometerRecordDataAccess.DeleteAllOdometerRecordsByVehicleId(vehicleId) &&
                _userLogic.DeleteAllAccessToVehicle(vehicleId) &&
                _dataAccess.DeleteVehicle(vehicleId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.Generic(string.Empty, "vehicle.delete", User.Identity.Name, vehicleId.ToString()));
            }
            return Json(OperationResponse.Succeed());
        }
        [HttpPost]
        [TypeFilter(typeof(StrictCollaboratorFilter), Arguments = new object[] { true, true })]
        public IActionResult DeleteVehicles(List<int> vehicleIds)
        {
            List<bool> results = new List<bool>();
            foreach (int vehicleId in vehicleIds)
            {
                //Delete all service records, gas records, notes, etc.
                var result = _gasRecordDataAccess.DeleteAllGasRecordsByVehicleId(vehicleId) &&
                    _serviceRecordDataAccess.DeleteAllServiceRecordsByVehicleId(vehicleId) &&
                    _collisionRecordDataAccess.DeleteAllCollisionRecordsByVehicleId(vehicleId) &&
                    _taxRecordDataAccess.DeleteAllTaxRecordsByVehicleId(vehicleId) &&
                    _noteDataAccess.DeleteAllNotesByVehicleId(vehicleId) &&
                    _reminderRecordDataAccess.DeleteAllReminderRecordsByVehicleId(vehicleId) &&
                    _upgradeRecordDataAccess.DeleteAllUpgradeRecordsByVehicleId(vehicleId) &&
                    _planRecordDataAccess.DeleteAllPlanRecordsByVehicleId(vehicleId) &&
                    _planRecordTemplateDataAccess.DeleteAllPlanRecordTemplatesByVehicleId(vehicleId) &&
                    _inspectionRecordDataAccess.DeleteAllInspectionRecordsByVehicleId(vehicleId) &&
                    _inspectionRecordTemplateDataAccess.DeleteAllInspectionReportTemplatesByVehicleId(vehicleId) &&
                    _supplyRecordDataAccess.DeleteAllSupplyRecordsByVehicleId(vehicleId) &&
                    _odometerRecordDataAccess.DeleteAllOdometerRecordsByVehicleId(vehicleId) &&
                    _userLogic.DeleteAllAccessToVehicle(vehicleId) &&
                    _dataAccess.DeleteVehicle(vehicleId);
                if (result)
                {
                    StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.Generic(string.Empty, "vehicle.delete", User.Identity.Name, vehicleId.ToString()));
                }
                results.Add(result);
            }
            return Json(OperationResponse.Conditional(results.Any() && results.All(x => x), "", StaticHelper.GenericErrorMessage));
        }
        [HttpPost]
        [TypeFilter(typeof(StrictCollaboratorFilter), Arguments = new object[] { true, true })]
        public IActionResult GetVehiclesCollaborators(List<int> vehicleIds)
        {
            var viewModel = new UserCollaboratorViewModel();
            if (vehicleIds.Count() == 1)
            {
                //only one vehicle to manage
                if (_userLogic.UserCanEditVehicle(GetUserID(), vehicleIds.First()))
                {
                    viewModel.CommonCollaborators = _userLogic.GetCollaboratorsForVehicle(vehicleIds.First()).Select(x => x.UserName).ToList();
                    viewModel.VehicleIds.Add(vehicleIds.First());
                }
            }
            else
            {
                List<UserCollaborator> allCollaborators = new List<UserCollaborator>();
                foreach (int vehicleId in vehicleIds)
                {
                    if (_userLogic.UserCanEditVehicle(GetUserID(), vehicleId))
                    {
                        var vehicleCollaborators = _userLogic.GetCollaboratorsForVehicle(vehicleId);
                        allCollaborators.AddRange(vehicleCollaborators);
                        viewModel.VehicleIds.Add(vehicleId);
                    }
                }
                var groupedCollaborations = allCollaborators.GroupBy(x => x.UserName);
                viewModel.CommonCollaborators = groupedCollaborations.Where(x => x.Count() == vehicleIds.Count()).Select(y => y.Key).ToList();
                viewModel.PartialCollaborators = groupedCollaborations.Where(x => x.Count() != vehicleIds.Count()).Select(y => y.Key).ToList();
            }
            return PartialView("_UserCollaborators", viewModel);
        }
        [HttpPost]
        [TypeFilter(typeof(StrictCollaboratorFilter), Arguments = new object[] { true, true })]
        public IActionResult AddCollaboratorsToVehicles(List<string> usernames, List<int> vehicleIds)
        {
            List<OperationResponse> results = new List<OperationResponse>();
            foreach (string username in usernames)
            {
                foreach (int vehicleId in vehicleIds)
                {
                    var result = _userLogic.AddCollaboratorToVehicle(vehicleId, username);
                    results.Add(result);
                }
            }
            var allFailed = results.All(x => !x.Success);
            if (allFailed && results.Any())
            {
                return Json(OperationResponse.Failed(results.FirstOrDefault(x => !x.Success).Message));
            }
            return Json(OperationResponse.Succeed());
        }
        [HttpPost]
        [TypeFilter(typeof(StrictCollaboratorFilter), Arguments = new object[] { true, true })]
        public IActionResult RemoveCollaboratorsFromVehicles(List<string> usernames, List<int> vehicleIds)
        {
            List<OperationResponse> results = new List<OperationResponse>();
            foreach (string username in usernames)
            {
                foreach (int vehicleId in vehicleIds)
                {
                    var result = _userLogic.DeleteCollaboratorFromVehicle(vehicleId, username);
                    results.Add(result);
                }
            }
            var allFailed = results.All(x => !x.Success);
            if (allFailed && results.Any())
            {
                return Json(OperationResponse.Failed(results.FirstOrDefault(x => !x.Success).Message));
            }
            return Json(OperationResponse.Succeed());
        }

        #region "Shared Methods"
        [HttpPost]
        public IActionResult GetFilesPendingUpload(List<UploadedFiles> uploadedFiles)
        {
            var filesPendingUpload = uploadedFiles.Where(x => x.IsPending).ToList();
            return PartialView("_FilesToUpload", filesPendingUpload);
        }
        [HttpPost]
        [TypeFilter(typeof(CollaboratorFilter))]
        public IActionResult SearchRecords(int vehicleId, string searchQuery, bool caseSensitive)
        {
            List<SearchResult> searchResults = new List<SearchResult>();
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return Json(searchResults);
            }
            if (!caseSensitive)
            {
                searchQuery = searchQuery.ToLower();
            }
            foreach (ImportMode visibleTab in _config.GetUserConfig(User).VisibleTabs)
            {
                switch (visibleTab)
                {
                    case ImportMode.ServiceRecord:
                        {
                            var results = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
                            if (caseSensitive)
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.ServiceRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                            }
                            else
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).ToLower().Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.ServiceRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                            }
                        }
                        break;
                    case ImportMode.RepairRecord:
                        {
                            var results = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
                            if (caseSensitive)
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.RepairRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                            }
                            else
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).ToLower().Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.RepairRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                            }
                        }
                        break;
                    case ImportMode.UpgradeRecord:
                        {
                            var results = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
                            if (caseSensitive)
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.UpgradeRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                            }
                            else
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).ToLower().Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.UpgradeRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                            }
                        }
                        break;
                    case ImportMode.TaxRecord:
                        {
                            var results = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
                            if (caseSensitive)
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.TaxRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                            }
                            else
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).ToLower().Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.TaxRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                            }
                        }
                        break;
                    case ImportMode.SupplyRecord:
                        {
                            var results = _supplyRecordDataAccess.GetSupplyRecordsByVehicleId(vehicleId);
                            if (caseSensitive)
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.SupplyRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                            }
                            else
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).ToLower().Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.SupplyRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                            }
                        }
                        break;
                    case ImportMode.PlanRecord:
                        {
                            var results = _planRecordDataAccess.GetPlanRecordsByVehicleId(vehicleId);
                            if (caseSensitive)
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.PlanRecord, Description = $"{x.DateCreated.ToShortDateString()} - {x.Description}" }));
                            }
                            else
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).ToLower().Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.PlanRecord, Description = $"{x.DateCreated.ToShortDateString()} - {x.Description}" }));
                            }
                        }
                        break;
                    case ImportMode.OdometerRecord:
                        {
                            var results = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
                            if (caseSensitive)
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.OdometerRecord, Description = $"{x.Date.ToShortDateString()} - {x.Mileage}" }));
                            }
                            else
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).ToLower().Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.OdometerRecord, Description = $"{x.Date.ToShortDateString()} - {x.Mileage}" }));
                            }
                        }
                        break;
                    case ImportMode.GasRecord:
                        {
                            var results = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
                            if (caseSensitive)
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.GasRecord, Description = $"{x.Date.ToShortDateString()} - {x.Mileage}" }));
                            }
                            else
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).ToLower().Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.GasRecord, Description = $"{x.Date.ToShortDateString()} - {x.Mileage}" }));
                            }
                        }
                        break;
                    case ImportMode.NoteRecord:
                        {
                            var results = _noteDataAccess.GetNotesByVehicleId(vehicleId);
                            if (caseSensitive)
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.NoteRecord, Description = $"{x.Description}" }));
                            }
                            else
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).ToLower().Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.NoteRecord, Description = $"{x.Description}" }));
                            }
                        }
                        break;
                    case ImportMode.ReminderRecord:
                        {
                            var results = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
                            if (caseSensitive)
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.ReminderRecord, Description = $"{x.Description}" }));
                            }
                            else
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).ToLower().Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.ReminderRecord, Description = $"{x.Description}" }));
                            }
                        }
                        break;
                    case ImportMode.InspectionRecord:
                        {
                            var results = _inspectionRecordDataAccess.GetInspectionRecordsByVehicleId(vehicleId);
                            if (caseSensitive)
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.InspectionRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                            }
                            else
                            {
                                searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).ToLower().Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.InspectionRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                            }
                        }
                        break;
                }
            }
            return PartialView("_GlobalSearchResult", searchResults);
        }
        [HttpPost]
        [TypeFilter(typeof(CollaboratorFilter))]
        public IActionResult SearchRecordsByTags(int vehicleId, string tags)
        {
            List<SearchResult> searchResults = new List<SearchResult>();
            if (string.IsNullOrWhiteSpace(tags))
            {
                return Json(searchResults);
            }
            var tagsFilter = tags.Split(' ').Distinct();
            foreach (ImportMode visibleTab in _config.GetUserConfig(User).VisibleTabs)
            {
                switch (visibleTab)
                {
                    case ImportMode.ServiceRecord:
                        {
                            var results = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
                            results.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
                            searchResults.AddRange(results.Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.ServiceRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                        }
                        break;
                    case ImportMode.RepairRecord:
                        {
                            var results = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
                            results.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
                            searchResults.AddRange(results.Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.RepairRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                        }
                        break;
                    case ImportMode.UpgradeRecord:
                        {
                            var results = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
                            results.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
                            searchResults.AddRange(results.Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.UpgradeRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                        }
                        break;
                    case ImportMode.TaxRecord:
                        {
                            var results = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
                            results.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
                            searchResults.AddRange(results.Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.TaxRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                        }
                        break;
                    case ImportMode.SupplyRecord:
                        {
                            var results = _supplyRecordDataAccess.GetSupplyRecordsByVehicleId(vehicleId);
                            results.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
                            searchResults.AddRange(results.Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.SupplyRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                        }
                        break;
                    case ImportMode.OdometerRecord:
                        {
                            var results = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
                            results.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
                            searchResults.AddRange(results.Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.OdometerRecord, Description = $"{x.Date.ToShortDateString()} - {x.Mileage}" }));
                        }
                        break;
                    case ImportMode.GasRecord:
                        {
                            var results = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
                            results.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
                            searchResults.AddRange(results.Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.GasRecord, Description = $"{x.Date.ToShortDateString()} - {x.Mileage}" }));
                        }
                        break;
                    case ImportMode.NoteRecord:
                        {
                            var results = _noteDataAccess.GetNotesByVehicleId(vehicleId);
                            results.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
                            searchResults.AddRange(results.Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.NoteRecord, Description = $"{x.Description}" }));
                        }
                        break;
                    case ImportMode.ReminderRecord:
                        {
                            var results = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
                            results.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
                            searchResults.AddRange(results.Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.ReminderRecord, Description = $"{x.Description}" }));
                        }
                        break;
                    case ImportMode.InspectionRecord:
                        {
                            var results = _inspectionRecordDataAccess.GetInspectionRecordsByVehicleId(vehicleId);
                            results.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
                            searchResults.AddRange(results.Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.InspectionRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                        }
                        break;
                }
            }
            return PartialView("_MapSearchResult", searchResults);
        }
        [HttpPost]
        [TypeFilter(typeof(CollaboratorFilter))]
        public IActionResult CheckRecordExist(int vehicleId, ImportMode importMode, int recordId)
        {
            if (recordId == default)
            {
                return Json(OperationResponse.Failed("Invalid Record"));
            }
            switch (importMode)
            {
                case ImportMode.ServiceRecord:
                    {
                        var results = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
                        return Json(OperationResponse.Conditional(results.Any(x => x.Id == recordId), "", "Service Record Not Found"));
                    }
                case ImportMode.RepairRecord:
                    {
                        var results = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
                        return Json(OperationResponse.Conditional(results.Any(x => x.Id == recordId), "", "Repair Record Not Found"));
                    }
                case ImportMode.UpgradeRecord:
                    {
                        var results = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
                        return Json(OperationResponse.Conditional(results.Any(x => x.Id == recordId), "", "Upgrade Record Not Found"));
                    }
                case ImportMode.TaxRecord:
                    {
                        var results = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
                        return Json(OperationResponse.Conditional(results.Any(x => x.Id == recordId), "", "Tax Record Not Found"));
                    }
                case ImportMode.SupplyRecord:
                    {
                        var results = _supplyRecordDataAccess.GetSupplyRecordsByVehicleId(vehicleId);
                        return Json(OperationResponse.Conditional(results.Any(x => x.Id == recordId), "", "Supply Record Not Found"));
                    }
                case ImportMode.PlanRecord:
                    {
                        var results = _planRecordDataAccess.GetPlanRecordsByVehicleId(vehicleId);
                        return Json(OperationResponse.Conditional(results.Any(x => x.Id == recordId), "", "Plan Record Not Found"));
                    }
                case ImportMode.OdometerRecord:
                    {
                        var results = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
                        return Json(OperationResponse.Conditional(results.Any(x => x.Id == recordId), "", "Odometer Record Not Found"));
                    }
                case ImportMode.GasRecord:
                    {
                        var results = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
                        return Json(OperationResponse.Conditional(results.Any(x => x.Id == recordId), "", "Gas Record Not Found"));
                    }
                case ImportMode.NoteRecord:
                    {
                        var results = _noteDataAccess.GetNotesByVehicleId(vehicleId);
                        return Json(OperationResponse.Conditional(results.Any(x => x.Id == recordId), "", "Note Record Not Found"));
                    }
                case ImportMode.ReminderRecord:
                    {
                        var results = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
                        return Json(OperationResponse.Conditional(results.Any(x => x.Id == recordId), "", "Reminder Not Found"));
                    }
                case ImportMode.InspectionRecord:
                    {
                        var results = _inspectionRecordDataAccess.GetInspectionRecordsByVehicleId(vehicleId);
                        return Json(OperationResponse.Conditional(results.Any(x => x.Id == recordId), "", "Inspection Record Not Found"));
                    }
            }
            return Json(OperationResponse.Failed("Record Not Found"));
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        public IActionResult GetMaxMileage(int vehicleId)
        {
            var result = _vehicleLogic.GetMaxMileage(vehicleId);
            return Json(result);
        }
        public IActionResult MoveRecord(int recordId, ImportMode source, ImportMode destination)
        {
            var genericRecord = new GenericRecord();
            bool result = false;
            //get
            switch (source)
            {
                case ImportMode.ServiceRecord:
                    genericRecord = _serviceRecordDataAccess.GetServiceRecordById(recordId);
                    break;
                case ImportMode.RepairRecord:
                    genericRecord = _collisionRecordDataAccess.GetCollisionRecordById(recordId);
                    break;
                case ImportMode.UpgradeRecord:
                    genericRecord = _upgradeRecordDataAccess.GetUpgradeRecordById(recordId);
                    break;
            }
            //save
            switch (destination)
            {
                case ImportMode.ServiceRecord:
                    result = _serviceRecordDataAccess.SaveServiceRecordToVehicle(StaticHelper.GenericToServiceRecord(genericRecord));
                    break;
                case ImportMode.RepairRecord:
                    result = _collisionRecordDataAccess.SaveCollisionRecordToVehicle(StaticHelper.GenericToRepairRecord(genericRecord));
                    break;
                case ImportMode.UpgradeRecord:
                    result = _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(StaticHelper.GenericToUpgradeRecord(genericRecord));
                    break;
            }
            //delete
            if (result)
            {
                switch (source)
                {
                    case ImportMode.ServiceRecord:
                        _serviceRecordDataAccess.DeleteServiceRecordById(recordId);
                        break;
                    case ImportMode.RepairRecord:
                        _collisionRecordDataAccess.DeleteCollisionRecordById(recordId);
                        break;
                    case ImportMode.UpgradeRecord:
                        _upgradeRecordDataAccess.DeleteUpgradeRecordById(recordId);
                        break;
                }
            }
            return Json(result);
        }
        public IActionResult MoveRecords(List<int> recordIds, ImportMode source, ImportMode destination)
        {
            var genericRecord = new GenericRecord();
            bool result = false;
            foreach (int recordId in recordIds)
            {
                //get
                switch (source)
                {
                    case ImportMode.ServiceRecord:
                        genericRecord = _serviceRecordDataAccess.GetServiceRecordById(recordId);
                        break;
                    case ImportMode.RepairRecord:
                        genericRecord = _collisionRecordDataAccess.GetCollisionRecordById(recordId);
                        break;
                    case ImportMode.UpgradeRecord:
                        genericRecord = _upgradeRecordDataAccess.GetUpgradeRecordById(recordId);
                        break;
                }
                //save
                switch (destination)
                {
                    case ImportMode.ServiceRecord:
                        result = _serviceRecordDataAccess.SaveServiceRecordToVehicle(StaticHelper.GenericToServiceRecord(genericRecord));
                        break;
                    case ImportMode.RepairRecord:
                        result = _collisionRecordDataAccess.SaveCollisionRecordToVehicle(StaticHelper.GenericToRepairRecord(genericRecord));
                        break;
                    case ImportMode.UpgradeRecord:
                        result = _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(StaticHelper.GenericToUpgradeRecord(genericRecord));
                        break;
                }
                //delete
                if (result)
                {
                    switch (source)
                    {
                        case ImportMode.ServiceRecord:
                            _serviceRecordDataAccess.DeleteServiceRecordById(recordId);
                            break;
                        case ImportMode.RepairRecord:
                            _collisionRecordDataAccess.DeleteCollisionRecordById(recordId);
                            break;
                        case ImportMode.UpgradeRecord:
                            _upgradeRecordDataAccess.DeleteUpgradeRecordById(recordId);
                            break;
                    }
                }
            }
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.Generic($"Moved multiple {source.ToString()} to {destination.ToString()} - Ids: {string.Join(",", recordIds)}", "bulk.move", User.Identity.Name, string.Empty));
            }
            return Json(result);
        }
        public IActionResult DeleteRecords(List<int> recordIds, ImportMode importMode)
        {
            bool result = false;
            foreach (int recordId in recordIds)
            {
                switch (importMode)
                {
                    case ImportMode.ServiceRecord:
                        result = DeleteServiceRecordWithChecks(recordId);
                        break;
                    case ImportMode.RepairRecord:
                        result = DeleteCollisionRecordWithChecks(recordId);
                        break;
                    case ImportMode.UpgradeRecord:
                        result = DeleteUpgradeRecordWithChecks(recordId);
                        break;
                    case ImportMode.GasRecord:
                        result = DeleteGasRecordWithChecks(recordId);
                        break;
                    case ImportMode.TaxRecord:
                        result = DeleteTaxRecordWithChecks(recordId);
                        break;
                    case ImportMode.SupplyRecord:
                        result = DeleteSupplyRecordWithChecks(recordId);
                        break;
                    case ImportMode.NoteRecord:
                        result = DeleteNoteWithChecks(recordId);
                        break;
                    case ImportMode.OdometerRecord:
                        result = DeleteOdometerRecordWithChecks(recordId);
                        break;
                    case ImportMode.ReminderRecord:
                        result = DeleteReminderRecordWithChecks(recordId);
                        break;
                    case ImportMode.InspectionRecord:
                        result = DeleteInspectionRecordWithChecks(recordId);
                        break;
                }
            }
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.Generic($"Deleted multiple {importMode.ToString()} - Ids: {string.Join(", ", recordIds)}", "bulk.delete", User.Identity.Name, string.Empty));
            }
            return Json(result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
        public IActionResult AdjustRecordsOdometer(List<int> recordIds, int vehicleId, ImportMode importMode)
        {
            bool result = false;
            //get vehicle's odometer adjustments
            var vehicleData = _dataAccess.GetVehicleById(vehicleId);
            foreach (int recordId in recordIds)
            {
                switch (importMode)
                {
                    case ImportMode.ServiceRecord:
                        {
                            var existingRecord = _serviceRecordDataAccess.GetServiceRecordById(recordId);
                            existingRecord.Mileage += int.Parse(vehicleData.OdometerDifference);
                            existingRecord.Mileage = decimal.ToInt32(existingRecord.Mileage * decimal.Parse(vehicleData.OdometerMultiplier, NumberStyles.Any));
                            result = _serviceRecordDataAccess.SaveServiceRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.RepairRecord:
                        {
                            var existingRecord = _collisionRecordDataAccess.GetCollisionRecordById(recordId);
                            existingRecord.Mileage += int.Parse(vehicleData.OdometerDifference);
                            existingRecord.Mileage = decimal.ToInt32(existingRecord.Mileage * decimal.Parse(vehicleData.OdometerMultiplier, NumberStyles.Any));
                            result = _collisionRecordDataAccess.SaveCollisionRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.UpgradeRecord:
                        {
                            var existingRecord = _upgradeRecordDataAccess.GetUpgradeRecordById(recordId);
                            existingRecord.Mileage += int.Parse(vehicleData.OdometerDifference);
                            existingRecord.Mileage = decimal.ToInt32(existingRecord.Mileage * decimal.Parse(vehicleData.OdometerMultiplier, NumberStyles.Any));
                            result = _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.GasRecord:
                        {
                            var existingRecord = _gasRecordDataAccess.GetGasRecordById(recordId);
                            existingRecord.Mileage += int.Parse(vehicleData.OdometerDifference);
                            existingRecord.Mileage = decimal.ToInt32(existingRecord.Mileage * decimal.Parse(vehicleData.OdometerMultiplier, NumberStyles.Any));
                            result = _gasRecordDataAccess.SaveGasRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.OdometerRecord:
                        {
                            var existingRecord = _odometerRecordDataAccess.GetOdometerRecordById(recordId);
                            existingRecord.Mileage += int.Parse(vehicleData.OdometerDifference);
                            existingRecord.Mileage = decimal.ToInt32(existingRecord.Mileage * decimal.Parse(vehicleData.OdometerMultiplier, NumberStyles.Any));
                            result = _odometerRecordDataAccess.SaveOdometerRecordToVehicle(existingRecord);
                        }
                        break;
                }
            }
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.Generic($"Adjusted odometer for multiple {importMode.ToString()} - Ids: {string.Join(",", recordIds)}", "bulk.odometer.adjust", User.Identity.Name, string.Empty));
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult DuplicateRecords(List<int> recordIds, ImportMode importMode)
        {
            bool result = false;
            foreach (int recordId in recordIds)
            {
                switch (importMode)
                {
                    case ImportMode.ServiceRecord:
                        {
                            var existingRecord = _serviceRecordDataAccess.GetServiceRecordById(recordId);
                            existingRecord.Id = default;
                            existingRecord.RequisitionHistory = new List<SupplyUsageHistory>();
                            result = _serviceRecordDataAccess.SaveServiceRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.RepairRecord:
                        {
                            var existingRecord = _collisionRecordDataAccess.GetCollisionRecordById(recordId);
                            existingRecord.Id = default;
                            existingRecord.RequisitionHistory = new List<SupplyUsageHistory>();
                            result = _collisionRecordDataAccess.SaveCollisionRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.UpgradeRecord:
                        {
                            var existingRecord = _upgradeRecordDataAccess.GetUpgradeRecordById(recordId);
                            existingRecord.Id = default;
                            existingRecord.RequisitionHistory = new List<SupplyUsageHistory>();
                            result = _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.GasRecord:
                        {
                            var existingRecord = _gasRecordDataAccess.GetGasRecordById(recordId);
                            existingRecord.Id = default;
                            result = _gasRecordDataAccess.SaveGasRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.TaxRecord:
                        {
                            var existingRecord = _taxRecordDataAccess.GetTaxRecordById(recordId);
                            existingRecord.Id = default;
                            result = _taxRecordDataAccess.SaveTaxRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.SupplyRecord:
                        {
                            var existingRecord = _supplyRecordDataAccess.GetSupplyRecordById(recordId);
                            existingRecord.Id = default;
                            existingRecord.RequisitionHistory = new List<SupplyUsageHistory>();
                            result = _supplyRecordDataAccess.SaveSupplyRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.NoteRecord:
                        {
                            var existingRecord = _noteDataAccess.GetNoteById(recordId);
                            existingRecord.Id = default;
                            result = _noteDataAccess.SaveNoteToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.OdometerRecord:
                        {
                            var existingRecord = _odometerRecordDataAccess.GetOdometerRecordById(recordId);
                            existingRecord.Id = default;
                            result = _odometerRecordDataAccess.SaveOdometerRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.ReminderRecord:
                        {
                            var existingRecord = _reminderRecordDataAccess.GetReminderRecordById(recordId);
                            existingRecord.Id = default;
                            result = _reminderRecordDataAccess.SaveReminderRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.PlanRecord:
                        {
                            var existingRecord = _planRecordDataAccess.GetPlanRecordById(recordId);
                            existingRecord.Id = default;
                            existingRecord.ReminderRecordId = default;
                            existingRecord.RequisitionHistory = new List<SupplyUsageHistory>();
                            result = _planRecordDataAccess.SavePlanRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.InspectionRecord:
                        {
                            var existingRecord = _inspectionRecordTemplateDataAccess.GetInspectionRecordTemplateById(recordId);
                            existingRecord.Id = default;
                            existingRecord.ReminderRecordId = new List<int>();
                            result = _inspectionRecordTemplateDataAccess.SaveInspectionReportTemplateToVehicle(existingRecord);
                        }
                        break;
                }
            }
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.Generic($"Duplicated multiple {importMode.ToString()} - Ids: {string.Join(",", recordIds)}", "bulk.duplicate", User.Identity.Name, string.Empty));
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult DuplicateRecordsToOtherVehicles(List<int> recordIds, List<int> vehicleIds, ImportMode importMode)
        {
            bool result = false;
            if (!recordIds.Any() || !vehicleIds.Any())
            {
                return Json(result);
            }
            foreach (int recordId in recordIds)
            {
                switch (importMode)
                {
                    case ImportMode.ServiceRecord:
                        {
                            var existingRecord = _serviceRecordDataAccess.GetServiceRecordById(recordId);
                            existingRecord.Id = default;
                            existingRecord.RequisitionHistory = new List<SupplyUsageHistory>();
                            foreach (int vehicleId in vehicleIds)
                            {
                                existingRecord.VehicleId = vehicleId;
                                result = _serviceRecordDataAccess.SaveServiceRecordToVehicle(existingRecord);
                            }
                        }
                        break;
                    case ImportMode.RepairRecord:
                        {
                            var existingRecord = _collisionRecordDataAccess.GetCollisionRecordById(recordId);
                            existingRecord.Id = default;
                            existingRecord.RequisitionHistory = new List<SupplyUsageHistory>();
                            foreach (int vehicleId in vehicleIds)
                            {
                                existingRecord.VehicleId = vehicleId;
                                result = _collisionRecordDataAccess.SaveCollisionRecordToVehicle(existingRecord);
                            }
                        }
                        break;
                    case ImportMode.UpgradeRecord:
                        {
                            var existingRecord = _upgradeRecordDataAccess.GetUpgradeRecordById(recordId);
                            existingRecord.Id = default;
                            existingRecord.RequisitionHistory = new List<SupplyUsageHistory>();
                            foreach (int vehicleId in vehicleIds)
                            {
                                existingRecord.VehicleId = vehicleId;
                                result = _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(existingRecord);
                            }
                        }
                        break;
                    case ImportMode.GasRecord:
                        {
                            var existingRecord = _gasRecordDataAccess.GetGasRecordById(recordId);
                            existingRecord.Id = default;
                            foreach (int vehicleId in vehicleIds)
                            {
                                existingRecord.VehicleId = vehicleId;
                                result = _gasRecordDataAccess.SaveGasRecordToVehicle(existingRecord);
                            }
                        }
                        break;
                    case ImportMode.TaxRecord:
                        {
                            var existingRecord = _taxRecordDataAccess.GetTaxRecordById(recordId);
                            existingRecord.Id = default;
                            foreach (int vehicleId in vehicleIds)
                            {
                                existingRecord.VehicleId = vehicleId;
                                result = _taxRecordDataAccess.SaveTaxRecordToVehicle(existingRecord);
                            }
                        }
                        break;
                    case ImportMode.SupplyRecord:
                        {
                            var existingRecord = _supplyRecordDataAccess.GetSupplyRecordById(recordId);
                            existingRecord.Id = default;
                            foreach (int vehicleId in vehicleIds)
                            {
                                existingRecord.VehicleId = vehicleId;
                                result = _supplyRecordDataAccess.SaveSupplyRecordToVehicle(existingRecord);
                            }
                        }
                        break;
                    case ImportMode.NoteRecord:
                        {
                            var existingRecord = _noteDataAccess.GetNoteById(recordId);
                            existingRecord.Id = default;
                            foreach (int vehicleId in vehicleIds)
                            {
                                existingRecord.VehicleId = vehicleId;
                                result = _noteDataAccess.SaveNoteToVehicle(existingRecord);
                            }
                        }
                        break;
                    case ImportMode.OdometerRecord:
                        {
                            var existingRecord = _odometerRecordDataAccess.GetOdometerRecordById(recordId);
                            existingRecord.Id = default;
                            foreach (int vehicleId in vehicleIds)
                            {
                                existingRecord.VehicleId = vehicleId;
                                result = _odometerRecordDataAccess.SaveOdometerRecordToVehicle(existingRecord);
                            }
                        }
                        break;
                    case ImportMode.ReminderRecord:
                        {
                            var existingRecord = _reminderRecordDataAccess.GetReminderRecordById(recordId);
                            existingRecord.Id = default;
                            foreach (int vehicleId in vehicleIds)
                            {
                                existingRecord.VehicleId = vehicleId;
                                result = _reminderRecordDataAccess.SaveReminderRecordToVehicle(existingRecord);
                            }
                        }
                        break;
                    case ImportMode.PlanRecord:
                        {
                            var existingRecord = _planRecordDataAccess.GetPlanRecordById(recordId);
                            existingRecord.Id = default;
                            existingRecord.ReminderRecordId = default;
                            existingRecord.RequisitionHistory = new List<SupplyUsageHistory>();
                            foreach (int vehicleId in vehicleIds)
                            {
                                existingRecord.VehicleId = vehicleId;
                                result = _planRecordDataAccess.SavePlanRecordToVehicle(existingRecord);
                            }
                        }
                        break;
                    case ImportMode.InspectionRecord:
                        {
                            var existingRecord = _inspectionRecordTemplateDataAccess.GetInspectionRecordTemplateById(recordId);
                            existingRecord.Id = default;
                            existingRecord.ReminderRecordId = new List<int>();
                            foreach (int vehicleId in vehicleIds)
                            {
                                existingRecord.VehicleId = vehicleId;
                                result = _inspectionRecordTemplateDataAccess.SaveInspectionReportTemplateToVehicle(existingRecord);
                            }
                        }
                        break;
                }
            }
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.Generic($"Duplicated multiple {importMode.ToString()} - Ids: {string.Join(",", recordIds)} - to Vehicle Ids: {string.Join(",", vehicleIds)}", "bulk.duplicate.to.vehicles", User.Identity.Name, string.Join(",", vehicleIds)));
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult BulkCreateOdometerRecords(List<int> recordIds, ImportMode importMode)
        {
            bool result = false;
            foreach (int recordId in recordIds)
            {
                switch (importMode)
                {
                    case ImportMode.ServiceRecord:
                        {
                            var existingRecord = _serviceRecordDataAccess.GetServiceRecordById(recordId);
                            result = _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                            {
                                Date = existingRecord.Date,
                                VehicleId = existingRecord.VehicleId,
                                Mileage = existingRecord.Mileage,
                                Notes = $"Auto Insert From Service Record: {existingRecord.Description}",
                                Files = StaticHelper.CreateAttachmentFromRecord(importMode, existingRecord.Id, existingRecord.Description)
                            });
                        }
                        break;
                    case ImportMode.RepairRecord:
                        {
                            var existingRecord = _collisionRecordDataAccess.GetCollisionRecordById(recordId);
                            result = _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                            {
                                Date = existingRecord.Date,
                                VehicleId = existingRecord.VehicleId,
                                Mileage = existingRecord.Mileage,
                                Notes = $"Auto Insert From Repair Record: {existingRecord.Description}",
                                Files = StaticHelper.CreateAttachmentFromRecord(importMode, existingRecord.Id, existingRecord.Description)
                            });
                        }
                        break;
                    case ImportMode.UpgradeRecord:
                        {
                            var existingRecord = _upgradeRecordDataAccess.GetUpgradeRecordById(recordId);
                            result = _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                            {
                                Date = existingRecord.Date,
                                VehicleId = existingRecord.VehicleId,
                                Mileage = existingRecord.Mileage,
                                Notes = $"Auto Insert From Upgrade Record: {existingRecord.Description}",
                                Files = StaticHelper.CreateAttachmentFromRecord(importMode, existingRecord.Id, existingRecord.Description)
                            });
                        }
                        break;
                    case ImportMode.GasRecord:
                        {
                            var existingRecord = _gasRecordDataAccess.GetGasRecordById(recordId);
                            result = _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                            {
                                Date = existingRecord.Date,
                                VehicleId = existingRecord.VehicleId,
                                Mileage = existingRecord.Mileage,
                                Notes = $"Auto Insert From Gas Record. {existingRecord.Notes}",
                                Files = StaticHelper.CreateAttachmentFromRecord(importMode, existingRecord.Id, $"Gas Record - {existingRecord.Mileage.ToString()}")
                            });
                        }
                        break;
                }
            }
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.Generic($"Created Odometer Records based on {importMode.ToString()} - Ids: {string.Join(",", recordIds)}", "bulk.odometer.insert", User.Identity.Name, string.Empty));
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult GetGenericRecordModal(List<int> recordIds, ImportMode dataType)
        {
            var extraFields = _extraFieldDataAccess.GetExtraFieldsById((int)dataType).ExtraFields;
            return PartialView("_GenericRecordModal", new GenericRecordEditModel() { DataType = dataType, RecordIds = recordIds, EditRecord = new GenericRecord { ExtraFields = extraFields } });
        }
        [HttpPost]
        public IActionResult EditMultipleRecords(GenericRecordEditModel genericRecordEditModel)
        {
            var dateIsEdited = genericRecordEditModel.EditRecord.Date != default;
            var descriptionIsEdited = !string.IsNullOrWhiteSpace(genericRecordEditModel.EditRecord.Description);
            var mileageIsEdited = genericRecordEditModel.EditRecord.Mileage != default;
            var costIsEdited = genericRecordEditModel.EditRecord.Cost != default;
            var noteIsEdited = !string.IsNullOrWhiteSpace(genericRecordEditModel.EditRecord.Notes);
            var tagsIsEdited = genericRecordEditModel.EditRecord.Tags.Any();
            var extraFieldIsEdited = genericRecordEditModel.EditRecord.ExtraFields.Any();
            //handle clear overrides
            if (tagsIsEdited && genericRecordEditModel.EditRecord.Tags.Contains("---"))
            {
                genericRecordEditModel.EditRecord.Tags = new List<string>();
            }
            if (noteIsEdited && genericRecordEditModel.EditRecord.Notes == "---")
            {
                genericRecordEditModel.EditRecord.Notes = "";
            }
            bool result = false;
            foreach (int recordId in genericRecordEditModel.RecordIds)
            {
                switch (genericRecordEditModel.DataType)
                {
                    case ImportMode.ServiceRecord:
                        {
                            var existingRecord = _serviceRecordDataAccess.GetServiceRecordById(recordId);
                            if (dateIsEdited)
                            {
                                existingRecord.Date = genericRecordEditModel.EditRecord.Date;
                            }
                            if (descriptionIsEdited)
                            {
                                existingRecord.Description = genericRecordEditModel.EditRecord.Description;
                            }
                            if (mileageIsEdited)
                            {
                                existingRecord.Mileage = genericRecordEditModel.EditRecord.Mileage;
                            }
                            if (costIsEdited)
                            {
                                existingRecord.Cost = genericRecordEditModel.EditRecord.Cost;
                            }
                            if (noteIsEdited)
                            {
                                existingRecord.Notes = genericRecordEditModel.EditRecord.Notes;
                            }
                            if (tagsIsEdited)
                            {
                                existingRecord.Tags = genericRecordEditModel.EditRecord.Tags;
                            }
                            if (extraFieldIsEdited)
                            {
                                foreach (ExtraField extraField in genericRecordEditModel.EditRecord.ExtraFields)
                                {
                                    if (existingRecord.ExtraFields.Any(x => x.Name == extraField.Name))
                                    {
                                        var insertIndex = existingRecord.ExtraFields.FindIndex(x => x.Name == extraField.Name);
                                        existingRecord.ExtraFields.RemoveAll(x => x.Name == extraField.Name);
                                        existingRecord.ExtraFields.Insert(insertIndex, extraField);
                                    }
                                    else
                                    {
                                        existingRecord.ExtraFields.Add(extraField);
                                    }
                                }
                            }
                            result = _serviceRecordDataAccess.SaveServiceRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.RepairRecord:
                        {
                            var existingRecord = _collisionRecordDataAccess.GetCollisionRecordById(recordId);
                            if (dateIsEdited)
                            {
                                existingRecord.Date = genericRecordEditModel.EditRecord.Date;
                            }
                            if (descriptionIsEdited)
                            {
                                existingRecord.Description = genericRecordEditModel.EditRecord.Description;
                            }
                            if (mileageIsEdited)
                            {
                                existingRecord.Mileage = genericRecordEditModel.EditRecord.Mileage;
                            }
                            if (costIsEdited)
                            {
                                existingRecord.Cost = genericRecordEditModel.EditRecord.Cost;
                            }
                            if (noteIsEdited)
                            {
                                existingRecord.Notes = genericRecordEditModel.EditRecord.Notes;
                            }
                            if (tagsIsEdited)
                            {
                                existingRecord.Tags = genericRecordEditModel.EditRecord.Tags;
                            }
                            if (extraFieldIsEdited)
                            {
                                foreach (ExtraField extraField in genericRecordEditModel.EditRecord.ExtraFields)
                                {
                                    if (existingRecord.ExtraFields.Any(x => x.Name == extraField.Name))
                                    {
                                        var insertIndex = existingRecord.ExtraFields.FindIndex(x => x.Name == extraField.Name);
                                        existingRecord.ExtraFields.RemoveAll(x => x.Name == extraField.Name);
                                        existingRecord.ExtraFields.Insert(insertIndex, extraField);
                                    }
                                    else
                                    {
                                        existingRecord.ExtraFields.Add(extraField);
                                    }
                                }
                            }
                            result = _collisionRecordDataAccess.SaveCollisionRecordToVehicle(existingRecord);
                        }
                        break;
                    case ImportMode.UpgradeRecord:
                        {
                            var existingRecord = _upgradeRecordDataAccess.GetUpgradeRecordById(recordId);
                            if (dateIsEdited)
                            {
                                existingRecord.Date = genericRecordEditModel.EditRecord.Date;
                            }
                            if (descriptionIsEdited)
                            {
                                existingRecord.Description = genericRecordEditModel.EditRecord.Description;
                            }
                            if (mileageIsEdited)
                            {
                                existingRecord.Mileage = genericRecordEditModel.EditRecord.Mileage;
                            }
                            if (costIsEdited)
                            {
                                existingRecord.Cost = genericRecordEditModel.EditRecord.Cost;
                            }
                            if (noteIsEdited)
                            {
                                existingRecord.Notes = genericRecordEditModel.EditRecord.Notes;
                            }
                            if (tagsIsEdited)
                            {
                                existingRecord.Tags = genericRecordEditModel.EditRecord.Tags;
                            }
                            if (extraFieldIsEdited)
                            {
                                foreach (ExtraField extraField in genericRecordEditModel.EditRecord.ExtraFields)
                                {
                                    if (existingRecord.ExtraFields.Any(x => x.Name == extraField.Name))
                                    {
                                        var insertIndex = existingRecord.ExtraFields.FindIndex(x => x.Name == extraField.Name);
                                        existingRecord.ExtraFields.RemoveAll(x => x.Name == extraField.Name);
                                        existingRecord.ExtraFields.Insert(insertIndex, extraField);
                                    }
                                    else
                                    {
                                        existingRecord.ExtraFields.Add(extraField);
                                    }
                                }
                            }
                            result = _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(existingRecord);
                        }
                        break;
                }
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult PrintRecordStickers(int vehicleId, List<int> recordIds, ImportMode importMode)
        {
            bool result = false;
            if (!recordIds.Any())
            {
                return Json(result);
            }
            var stickerViewModel = new StickerViewModel() { RecordType = importMode };
            if (vehicleId != default)
            {
                var vehicleData = _dataAccess.GetVehicleById(vehicleId);
                if (vehicleData != null && vehicleData.Id != default)
                {
                    stickerViewModel.VehicleData = vehicleData;
                }
            }

            int recordsAdded = 0;
            switch (importMode)
            {
                case ImportMode.ServiceRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            stickerViewModel.GenericRecords.Add(_serviceRecordDataAccess.GetServiceRecordById(recordId));
                            recordsAdded++;
                        }

                    }
                    break;
                case ImportMode.RepairRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            stickerViewModel.GenericRecords.Add(_collisionRecordDataAccess.GetCollisionRecordById(recordId));
                            recordsAdded++;
                        }
                    }
                    break;
                case ImportMode.UpgradeRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            stickerViewModel.GenericRecords.Add(_upgradeRecordDataAccess.GetUpgradeRecordById(recordId));
                            recordsAdded++;
                        }
                    }
                    break;
                case ImportMode.GasRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            var record = _gasRecordDataAccess.GetGasRecordById(recordId);
                            stickerViewModel.GenericRecords.Add(new GenericRecord
                            {
                                Cost = record.Cost,
                                Date = record.Date,
                                Notes = record.Notes,
                                Mileage = record.Mileage,
                                ExtraFields = record.ExtraFields
                            });
                            recordsAdded++;
                        }

                    }
                    break;
                case ImportMode.TaxRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            var record = _taxRecordDataAccess.GetTaxRecordById(recordId);
                            stickerViewModel.GenericRecords.Add(new GenericRecord
                            {
                                Description = record.Description,
                                Cost = record.Cost,
                                Notes = record.Notes,
                                Date = record.Date,
                                ExtraFields = record.ExtraFields
                            });
                            recordsAdded++;
                        }
                    }
                    break;
                case ImportMode.SupplyRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            var record = _supplyRecordDataAccess.GetSupplyRecordById(recordId);
                            stickerViewModel.SupplyRecords.Add(record);
                            recordsAdded++;
                        }
                    }
                    break;
                case ImportMode.NoteRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            var record = _noteDataAccess.GetNoteById(recordId);
                            stickerViewModel.GenericRecords.Add(new GenericRecord
                            {
                                Description = record.Description,
                                Notes = record.NoteText
                            });
                            recordsAdded++;
                        }

                    }
                    break;
                case ImportMode.OdometerRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            var record = _odometerRecordDataAccess.GetOdometerRecordById(recordId);
                            stickerViewModel.GenericRecords.Add(new GenericRecord
                            {
                                Date = record.Date,
                                Mileage = record.Mileage,
                                Notes = record.Notes,
                                ExtraFields = record.ExtraFields
                            });
                            recordsAdded++;
                        }

                    }
                    break;
                case ImportMode.ReminderRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            stickerViewModel.ReminderRecords.Add(_reminderRecordDataAccess.GetReminderRecordById(recordId));
                            recordsAdded++;
                        }

                    }
                    break;
                case ImportMode.PlanRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            var record = _planRecordDataAccess.GetPlanRecordById(recordId);
                            stickerViewModel.GenericRecords.Add(new GenericRecord
                            {
                                Description = record.Description,
                                Cost = record.Cost,
                                Notes = record.Notes,
                                Date = record.DateModified,
                                ExtraFields = record.ExtraFields,
                                RequisitionHistory = record.RequisitionHistory
                            });
                            recordsAdded++;
                        }
                    }
                    break;
                case ImportMode.InspectionRecord:
                    foreach (int recordId in recordIds)
                    {
                        var record = _inspectionRecordDataAccess.GetInspectionRecordById(recordId);
                        stickerViewModel.InspectionRecords.Add(record);
                        recordsAdded++;
                    }
                    break;
            }
            if (recordsAdded > 0)
            {
                return PartialView("_Stickers", stickerViewModel);
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult SaveUserColumnPreferences(UserColumnPreference columnPreference)
        {
            try
            {
                var userConfig = _config.GetUserConfig(User);
                var existingUserColumnPreference = userConfig.UserColumnPreferences.Where(x => x.Tab == columnPreference.Tab);
                if (existingUserColumnPreference.Any())
                {
                    var existingPreference = existingUserColumnPreference.Single();
                    existingPreference.VisibleColumns = columnPreference.VisibleColumns;
                    existingPreference.ColumnOrder = columnPreference.ColumnOrder;
                }
                else
                {
                    userConfig.UserColumnPreferences.Add(columnPreference);
                }
                var result = _config.SaveUserConfig(User, userConfig);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(false);
            }
        }
        #endregion
    }
}

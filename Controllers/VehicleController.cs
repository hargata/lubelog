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
            IConfigHelper config)
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
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpPost]
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
                _supplyRecordDataAccess.DeleteAllSupplyRecordsByVehicleId(vehicleId) &&
                _odometerRecordDataAccess.DeleteAllOdometerRecordsByVehicleId(vehicleId) &&
                _userLogic.DeleteAllAccessToVehicle(vehicleId) &&
                _dataAccess.DeleteVehicle(vehicleId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.Generic(string.Empty, "vehicle.delete", User.Identity.Name, vehicleId.ToString()));
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult DuplicateVehicleCollaborators(int sourceVehicleId, int destVehicleId)
        {
            try
            {
                //retrieve collaborators for both source and destination vehicle id.
                if (_userLogic.UserCanEditVehicle(GetUserID(), sourceVehicleId) && _userLogic.UserCanEditVehicle(GetUserID(), destVehicleId))
                {
                    var sourceCollaborators = _userLogic.GetCollaboratorsForVehicle(sourceVehicleId).Select(x => x.UserVehicle.UserId).ToList();
                    var destCollaborators = _userLogic.GetCollaboratorsForVehicle(destVehicleId).Select(x => x.UserVehicle.UserId).ToList();
                    sourceCollaborators.RemoveAll(x => destCollaborators.Contains(x));
                    if (sourceCollaborators.Any())
                    {
                        foreach (int collaboratorId in sourceCollaborators)
                        {
                            _userLogic.AddUserAccessToVehicle(collaboratorId, destVehicleId);
                        }
                    }
                    else
                    {
                        return Json(OperationResponse.Failed("Both vehicles already have identical collaborators"));
                    }
                }
                return Json(OperationResponse.Succeed("Collaborators Copied"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(OperationResponse.Failed());
            }
        }

        #region "Shared Methods"
        [HttpPost]
        public IActionResult GetFilesPendingUpload(List<UploadedFiles> uploadedFiles)
        {
            var filesPendingUpload = uploadedFiles.Where(x => x.Location.StartsWith("/temp/")).ToList();
            return PartialView("_FilesToUpload", filesPendingUpload);
        }
        [HttpPost]
        [TypeFilter(typeof(CollaboratorFilter))]
        public IActionResult SearchRecords(int vehicleId, string searchQuery)
        {
            List<SearchResult> searchResults = new List<SearchResult>();
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return Json(searchResults);
            }
            foreach (ImportMode visibleTab in _config.GetUserConfig(User).VisibleTabs)
            {
                switch (visibleTab)
                {
                    case ImportMode.ServiceRecord:
                        {
                            var results = _serviceRecordDataAccess.GetServiceRecordsByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.ServiceRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                        }
                        break;
                    case ImportMode.RepairRecord:
                        {
                            var results = _collisionRecordDataAccess.GetCollisionRecordsByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.RepairRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                        }
                        break;
                    case ImportMode.UpgradeRecord:
                        {
                            var results = _upgradeRecordDataAccess.GetUpgradeRecordsByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.UpgradeRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                        }
                        break;
                    case ImportMode.TaxRecord:
                        {
                            var results = _taxRecordDataAccess.GetTaxRecordsByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.TaxRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                        }
                        break;
                    case ImportMode.SupplyRecord:
                        {
                            var results = _supplyRecordDataAccess.GetSupplyRecordsByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.SupplyRecord, Description = $"{x.Date.ToShortDateString()} - {x.Description}" }));
                        }
                        break;
                    case ImportMode.PlanRecord:
                        {
                            var results = _planRecordDataAccess.GetPlanRecordsByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.PlanRecord, Description = $"{x.DateCreated.ToShortDateString()} - {x.Description}" }));
                        }
                        break;
                    case ImportMode.OdometerRecord:
                        {
                            var results = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.OdometerRecord, Description = $"{x.Date.ToShortDateString()} - {x.Mileage}" }));
                        }
                        break;
                    case ImportMode.GasRecord:
                        {
                            var results = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.GasRecord, Description = $"{x.Date.ToShortDateString()} - {x.Mileage}" }));
                        }
                        break;
                    case ImportMode.NoteRecord:
                        {
                            var results = _noteDataAccess.GetNotesByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.NoteRecord, Description = $"{x.Description}" }));
                        }
                        break;
                    case ImportMode.ReminderRecord:
                        {
                            var results = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
                            searchResults.AddRange(results.Where(x => JsonSerializer.Serialize(x).Contains(searchQuery)).Select(x => new SearchResult { Id = x.Id, RecordType = ImportMode.ReminderRecord, Description = $"{x.Description}" }));
                        }
                        break;
                }
            }
            return PartialView("_GlobalSearchResult", searchResults);
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
                                Notes = $"Auto Insert From Service Record: {existingRecord.Description}"
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
                                Notes = $"Auto Insert From Repair Record: {existingRecord.Description}"
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
                                Notes = $"Auto Insert From Upgrade Record: {existingRecord.Description}"
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
                                Notes = $"Auto Insert From Gas Record. {existingRecord.Notes}"
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
            var stickerViewModel = new StickerViewModel();
            int recordsAdded = 0;
            switch (importMode)
            {
                case ImportMode.ServiceRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            stickerViewModel.VehicleRecords.ServiceRecords.Add(_serviceRecordDataAccess.GetServiceRecordById(recordId));
                            recordsAdded++;
                        }

                    }
                    break;
                case ImportMode.RepairRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            stickerViewModel.VehicleRecords.CollisionRecords.Add(_collisionRecordDataAccess.GetCollisionRecordById(recordId));
                            recordsAdded++;
                        }
                    }
                    break;
                case ImportMode.UpgradeRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            stickerViewModel.VehicleRecords.UpgradeRecords.Add(_upgradeRecordDataAccess.GetUpgradeRecordById(recordId));
                            recordsAdded++;
                        }
                    }
                    break;
                case ImportMode.GasRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            stickerViewModel.VehicleRecords.GasRecords.Add(_gasRecordDataAccess.GetGasRecordById(recordId));
                            recordsAdded++;
                        }

                    }
                    break;
                case ImportMode.TaxRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            stickerViewModel.VehicleRecords.TaxRecords.Add(_taxRecordDataAccess.GetTaxRecordById(recordId));
                            recordsAdded++;
                        }
                    }
                    break;
                case ImportMode.SupplyRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            stickerViewModel.VehicleRecords.SupplyRecords.Add(_supplyRecordDataAccess.GetSupplyRecordById(recordId));
                            recordsAdded++;
                        }

                    }
                    break;
                case ImportMode.NoteRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            stickerViewModel.VehicleRecords.NoteRecords.Add(_noteDataAccess.GetNoteById(recordId));
                            recordsAdded++;
                        }

                    }
                    break;
                case ImportMode.OdometerRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            stickerViewModel.VehicleRecords.OdometerRecords.Add(_odometerRecordDataAccess.GetOdometerRecordById(recordId));
                            recordsAdded++;
                        }

                    }
                    break;
                case ImportMode.ReminderRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            stickerViewModel.VehicleRecords.ReminderRecords.Add(_reminderRecordDataAccess.GetReminderRecordById(recordId));
                            recordsAdded++;
                        }

                    }
                    break;
                case ImportMode.PlanRecord:
                    {
                        foreach (int recordId in recordIds)
                        {
                            stickerViewModel.VehicleRecords.PlanRecords.Add(_planRecordDataAccess.GetPlanRecordById(recordId));
                            recordsAdded++;
                        }
                    }
                    break;
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

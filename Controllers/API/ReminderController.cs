using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public partial class APIController
    {
        [HttpGet]
        [Route("/api/vehicle/reminders/all")]
        public IActionResult AllReminders(ReminderMethodParameter parameters)
        {
            List<int> vehicleIds = new List<int>();
            var vehicles = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehicles = _userLogic.FilterUserVehicles(vehicles, GetUserID());
            }
            vehicleIds.AddRange(vehicles.Select(x => x.Id));
            List<ReminderRecordViewModel> reminderResults = new List<ReminderRecordViewModel>();
            foreach (int vehicleId in vehicleIds)
            {
                var currentMileage = _vehicleLogic.GetMaxMileage(vehicleId);
                var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
                reminderResults.AddRange(_reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, DateTime.Now));
            }
            if (parameters.Id != default)
            {
                reminderResults.RemoveAll(x => x.Id != parameters.Id);
            }
            if (parameters.Urgencies == null || !parameters.Urgencies.Any())
            {
                //if no urgencies parameter, we will default to all urgencies.
                parameters.Urgencies = new List<ReminderUrgency> { ReminderUrgency.NotUrgent, ReminderUrgency.Urgent, ReminderUrgency.VeryUrgent, ReminderUrgency.PastDue };
            }
            reminderResults.RemoveAll(x => !parameters.Urgencies.Contains(x.Urgency));
            if (!string.IsNullOrWhiteSpace(parameters.Tags))
            {
                var tagsFilter = parameters.Tags.Split(' ').Distinct();
                reminderResults.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
            }
            var results = reminderResults.Select(x => new ReminderAPIExportModel { VehicleId = x.VehicleId.ToString(), Id = x.Id.ToString(), Description = x.Description, Urgency = x.Urgency.ToString(), Metric = x.Metric.ToString(), UserMetric = x.UserMetric.ToString(), Notes = x.Notes, DueDate = x.Date.ToShortDateString(), DueOdometer = x.Mileage.ToString(), DueDays = x.DueDays.ToString(), DueDistance = x.DueMileage.ToString(), Tags = string.Join(' ', x.Tags) });
            if (_config.GetInvariantApi() || Request.Headers.ContainsKey("culture-invariant"))
            {
                return Json(results, StaticHelper.GetInvariantOption());
            }
            else
            {
                return Json(results);
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/reminders")]
        public IActionResult Reminders(int vehicleId, ReminderMethodParameter parameters)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            if (parameters.Urgencies == null || !parameters.Urgencies.Any())
            {
                //if no urgencies parameter, we will default to all urgencies.
                parameters.Urgencies = new List<ReminderUrgency> { ReminderUrgency.NotUrgent, ReminderUrgency.Urgent, ReminderUrgency.VeryUrgent, ReminderUrgency.PastDue };
            }
            var currentMileage = _vehicleLogic.GetMaxMileage(vehicleId);
            var reminders = _reminderRecordDataAccess.GetReminderRecordsByVehicleId(vehicleId);
            var reminderResults = _reminderHelper.GetReminderRecordViewModels(reminders, currentMileage, DateTime.Now);
            if (parameters.Id != default)
            {
                reminderResults.RemoveAll(x => x.Id != parameters.Id);
            }
            reminderResults.RemoveAll(x => !parameters.Urgencies.Contains(x.Urgency));
            if (!string.IsNullOrWhiteSpace(parameters.Tags))
            {
                var tagsFilter = parameters.Tags.Split(' ').Distinct();
                reminderResults.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
            }
            var results = reminderResults.Select(x => new ReminderAPIExportModel { VehicleId = x.VehicleId.ToString(), Id = x.Id.ToString(), Description = x.Description, Urgency = x.Urgency.ToString(), Metric = x.Metric.ToString(), UserMetric = x.UserMetric.ToString(), Notes = x.Notes, DueDate = x.Date.ToShortDateString(), DueOdometer = x.Mileage.ToString(), DueDays = x.DueDays.ToString(), DueDistance = x.DueMileage.ToString(), Tags = string.Join(' ', x.Tags) });
            if (_config.GetInvariantApi() || Request.Headers.ContainsKey("culture-invariant"))
            {
                return Json(results, StaticHelper.GetInvariantOption());
            }
            else
            {
                return Json(results);
            }
        }
        [TypeFilter(typeof(QueryParamFilter), Arguments = new object[] { new string[] { "vehicleId" } })]
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { false, true, HouseholdPermission.Edit })]
        [HttpPost]
        [Route("/api/vehicle/reminders/add")]
        [Consumes("application/json")]
        public IActionResult AddReminderRecordJson(int vehicleId, [FromBody] ReminderExportModel input) => AddReminderRecord(vehicleId, input);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { false, true, HouseholdPermission.Edit })]
        [HttpPost]
        [Route("/api/vehicle/reminders/add")]
        public IActionResult AddReminderRecord(int vehicleId, ReminderExportModel input)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            if (string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.Metric))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Description and Metric cannot be empty."));
            }
            bool validMetric = Enum.TryParse(input.Metric, out ReminderMetric parsedMetric);
            bool validDate = DateTime.TryParse(input.DueDate, out DateTime parsedDate);
            bool validOdometer = int.TryParse(input.DueOdometer, out int parsedOdometer);
            if (!validMetric)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, values for Metric(Date, Odometer, Both) is invalid."));
            }
            //validate metrics
            switch (parsedMetric)
            {
                case ReminderMetric.Both:
                    //validate due date and odometer
                    if (!validDate || !validOdometer)
                    {
                        return Json(OperationResponse.Failed("Input object invalid, DueDate and DueOdometer must be valid if Metric is Both"));
                    }
                    break;
                case ReminderMetric.Date:
                    if (!validDate)
                    {
                        return Json(OperationResponse.Failed("Input object invalid, DueDate must be valid if Metric is Date"));
                    }
                    break;
                case ReminderMetric.Odometer:
                    if (!validOdometer)
                    {
                        return Json(OperationResponse.Failed("Input object invalid, DueOdometer must be valid if Metric is Odometer"));
                    }
                    break;
            }
            try
            {
                var reminderRecord = new ReminderRecord()
                {
                    VehicleId = vehicleId,
                    Description = input.Description,
                    Mileage = parsedOdometer,
                    Date = parsedDate,
                    Metric = parsedMetric,
                    Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes,
                    Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList()
                };
                _reminderRecordDataAccess.SaveReminderRecordToVehicle(reminderRecord);
                _eventLogic.PublishEvent(GetUserID(), WebHookPayload.FromReminderRecord(reminderRecord, "reminderrecord.add.api", User.Identity?.Name ?? string.Empty));
                return Json(OperationResponse.Succeed("Reminder Record Added", new { recordId = reminderRecord.Id }));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/reminders/update")]
        [Consumes("application/json")]
        public IActionResult UpdateReminderRecordJson([FromBody] ReminderExportModel input) => UpdateReminderRecord(input);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/reminders/update")]
        public IActionResult UpdateReminderRecord(ReminderExportModel input)
        {
            if (string.IsNullOrWhiteSpace(input.Id) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.Metric))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Id, Description and Metric cannot be empty."));
            }
            bool validMetric = Enum.TryParse(input.Metric, out ReminderMetric parsedMetric);
            bool validDate = DateTime.TryParse(input.DueDate, out DateTime parsedDate);
            bool validOdometer = int.TryParse(input.DueOdometer, out int parsedOdometer);
            if (!validMetric)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, values for Metric(Date, Odometer, Both) is invalid."));
            }
            //validate metrics
            switch (parsedMetric)
            {
                case ReminderMetric.Both:
                    //validate due date and odometer
                    if (!validDate || !validOdometer)
                    {
                        return Json(OperationResponse.Failed("Input object invalid, DueDate and DueOdometer must be valid if Metric is Both"));
                    }
                    break;
                case ReminderMetric.Date:
                    if (!validDate)
                    {
                        return Json(OperationResponse.Failed("Input object invalid, DueDate must be valid if Metric is Date"));
                    }
                    break;
                case ReminderMetric.Odometer:
                    if (!validOdometer)
                    {
                        return Json(OperationResponse.Failed("Input object invalid, DueOdometer must be valid if Metric is Odometer"));
                    }
                    break;
            }
            try
            {
                //retrieve existing record
                var existingRecord = _reminderRecordDataAccess.GetReminderRecordById(int.Parse(input.Id));
                if (existingRecord != null && existingRecord.Id == int.Parse(input.Id))
                {
                    //check if user has access to the vehicleId
                    if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Edit))
                    {
                        Response.StatusCode = 401;
                        return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
                    }
                    existingRecord.Date = parsedDate;
                    existingRecord.Mileage = parsedOdometer;
                    existingRecord.Description = input.Description;
                    existingRecord.Metric = parsedMetric;
                    existingRecord.Notes = string.IsNullOrWhiteSpace(input.Notes) ? "" : input.Notes;
                    existingRecord.Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList();
                    _reminderRecordDataAccess.SaveReminderRecordToVehicle(existingRecord);
                    _eventLogic.PublishEvent(GetUserID(), WebHookPayload.FromReminderRecord(existingRecord, "reminderrecord.update.api", User.Identity?.Name ?? string.Empty));
                }
                else
                {
                    Response.StatusCode = 400;
                    return Json(OperationResponse.Failed("Invalid Record Id"));
                }
                return Json(OperationResponse.Succeed("Reminder Record Updated"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Delete })]
        [HttpDelete]
        [Route("/api/vehicle/reminders/delete")]
        public IActionResult DeleteReminderRecord(int id)
        {
            var existingRecord = _reminderRecordDataAccess.GetReminderRecordById(id);
            if (existingRecord == null || existingRecord.Id == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Invalid Record Id"));
            }
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Delete))
            {
                Response.StatusCode = 401;
                return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
            }
            var result = _reminderRecordDataAccess.DeleteReminderRecordById(existingRecord.Id);
            if (result)
            {
                _eventLogic.PublishEvent(GetUserID(), WebHookPayload.FromReminderRecord(existingRecord, "reminderrecord.delete.api", User.Identity?.Name ?? string.Empty));
            }
            return Json(OperationResponse.Conditional(result, "Reminder Record Deleted"));
        }
        [HttpGet]
        [Route("/api/calendar")]
        public IActionResult Calendar()
        {
            var vehiclesStored = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehiclesStored = _userLogic.FilterUserVehicles(vehiclesStored, GetUserID());
            }
            var reminders = _vehicleLogic.GetReminders(vehiclesStored, true);
            var calendarContent = StaticHelper.RemindersToCalendar(reminders);
            return File(calendarContent, "text/calendar");
        }
    }
}

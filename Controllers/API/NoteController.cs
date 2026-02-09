using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public partial class APIController
    {
        [HttpGet]
        [Route("/api/vehicle/notes/all")]
        public IActionResult AllNotes(MethodParameter parameters)
        {
            List<int> vehicleIds = new List<int>();
            var vehicles = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehicles = _userLogic.FilterUserVehicles(vehicles, GetUserID());
            }
            vehicleIds.AddRange(vehicles.Select(x => x.Id));
            List<Note> vehicleRecords = new List<Note>();
            foreach (int vehicleId in vehicleIds)
            {
                vehicleRecords.AddRange(_noteDataAccess.GetNotesByVehicleId(vehicleId));
            }
            if (parameters.Id != default)
            {
                vehicleRecords.RemoveAll(x => x.Id != parameters.Id);
            }
            if (!string.IsNullOrWhiteSpace(parameters.Tags))
            {
                var tagsFilter = parameters.Tags.Split(' ').Distinct();
                vehicleRecords.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
            }
            var result = vehicleRecords
                .Select(x => new NoteRecordExportModel
                {
                    VehicleId = x.VehicleId.ToString(),
                    Id = x.Id.ToString(),
                    Description = x.Description,
                    NoteText = x.NoteText,
                    Pinned = x.Pinned.ToString(),
                    ExtraFields = x.ExtraFields,
                    Files = x.Files,
                    Tags = string.Join(' ', x.Tags)
                });
            if (_config.GetInvariantApi() || Request.Headers.ContainsKey("culture-invariant"))
            {
                return Json(result, StaticHelper.GetInvariantOption());
            }
            else
            {
                return Json(result);
            }
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        [Route("/api/vehicle/notes")]
        public IActionResult Notes(int vehicleId, MethodParameter parameters)
        {
            if (vehicleId == default)
            {
                var response = OperationResponse.Failed("Must provide a valid vehicle id");
                Response.StatusCode = 400;
                return Json(response);
            }
            var vehicleRecords = _noteDataAccess.GetNotesByVehicleId(vehicleId);
            if (parameters.Id != default)
            {
                vehicleRecords.RemoveAll(x => x.Id != parameters.Id);
            }
            if (!string.IsNullOrWhiteSpace(parameters.Tags))
            {
                var tagsFilter = parameters.Tags.Split(' ').Distinct();
                vehicleRecords.RemoveAll(x => !x.Tags.Any(y => tagsFilter.Contains(y)));
            }
            var result = vehicleRecords
                .Select(x => new NoteRecordExportModel
                {
                    VehicleId = x.VehicleId.ToString(),
                    Id = x.Id.ToString(),
                    Description = x.Description,
                    NoteText = x.NoteText,
                    Pinned = x.Pinned.ToString(),
                    ExtraFields = x.ExtraFields,
                    Files = x.Files,
                    Tags = string.Join(' ', x.Tags)
                });
            if (_config.GetInvariantApi() || Request.Headers.ContainsKey("culture-invariant"))
            {
                return Json(result, StaticHelper.GetInvariantOption());
            }
            else
            {
                return Json(result);
            }
        }
        [TypeFilter(typeof(QueryParamFilter), Arguments = new object[] { new string[] { "vehicleId" } })]
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { false, true, HouseholdPermission.Edit })]
        [HttpPost]
        [Route("/api/vehicle/notes/add")]
        [Consumes("application/json")]
        public IActionResult AddNoteJson(int vehicleId, [FromBody] NoteRecordExportModel input) => AddNote(vehicleId, input);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [TypeFilter(typeof(CollaboratorFilter), Arguments = new object[] { false, true, HouseholdPermission.Edit })]
        [HttpPost]
        [Route("/api/vehicle/notes/add")]
        public IActionResult AddNote(int vehicleId, NoteRecordExportModel input)
        {
            if (vehicleId == default)
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Must provide a valid vehicle id"));
            }
            if (string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.NoteText))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Description and NoteText cannot be empty."));
            }
            if (input.Files == null)
            {
                input.Files = new List<UploadedFiles>();
            }
            if (input.ExtraFields == null)
            {
                input.ExtraFields = new List<ExtraField>();
            }
            try
            {
                var note = new Note()
                {
                    VehicleId = vehicleId,
                    Description = input.Description,
                    NoteText = input.NoteText,
                    Pinned = string.IsNullOrWhiteSpace(input.Pinned) ? false : bool.Parse(input.Pinned),
                    ExtraFields = input.ExtraFields,
                    Files = input.Files,
                    Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList()
                };
                _noteDataAccess.SaveNoteToVehicle(note);
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(),WebHookPayload.FromNoteRecord(note, "note.add.api", User.Identity.Name));
                return Json(OperationResponse.Succeed("Note Added", new { recordId = note.Id }));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Delete })]
        [HttpDelete]
        [Route("/api/vehicle/notes/delete")]
        public IActionResult DeleteNote(int id)
        {
            var existingRecord = _noteDataAccess.GetNoteById(id);
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
            var result = _noteDataAccess.DeleteNoteById(existingRecord.Id);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromNoteRecord(existingRecord, "note.delete.api", User.Identity.Name));
            }
            return Json(OperationResponse.Conditional(result, "Note Deleted"));
        }
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/notes/update")]
        [Consumes("application/json")]
        public IActionResult UpdateNoteJson([FromBody] NoteRecordExportModel input) => UpdateNote(input);
        [TypeFilter(typeof(APIKeyFilter), Arguments = new object[] { HouseholdPermission.Edit })]
        [HttpPut]
        [Route("/api/vehicle/notes/update")]
        public IActionResult UpdateNote(NoteRecordExportModel input)
        {
            if (string.IsNullOrWhiteSpace(input.Id) ||
                string.IsNullOrWhiteSpace(input.Description) ||
                string.IsNullOrWhiteSpace(input.NoteText))
            {
                Response.StatusCode = 400;
                return Json(OperationResponse.Failed("Input object invalid, Id, Description, and NoteText cannot be empty."));
            }
            if (input.Files == null)
            {
                input.Files = new List<UploadedFiles>();
            }
            if (input.ExtraFields == null)
            {
                input.ExtraFields = new List<ExtraField>();
            }
            try
            {
                //retrieve existing record
                var existingRecord = _noteDataAccess.GetNoteById(int.Parse(input.Id));
                if (existingRecord != null && existingRecord.Id == int.Parse(input.Id))
                {
                    //check if user has access to the vehicleId
                    if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Edit))
                    {
                        Response.StatusCode = 401;
                        return Json(OperationResponse.Failed("Access Denied, you don't have access to this vehicle."));
                    }
                    existingRecord.Description = input.Description;
                    existingRecord.NoteText = input.NoteText;
                    existingRecord.Pinned = string.IsNullOrWhiteSpace(input.Pinned) ? false : bool.Parse(input.Pinned);
                    existingRecord.Files = input.Files;
                    existingRecord.ExtraFields = input.ExtraFields;
                    existingRecord.Tags = string.IsNullOrWhiteSpace(input.Tags) ? new List<string>() : input.Tags.Split(' ').Distinct().ToList();
                    _noteDataAccess.SaveNoteToVehicle(existingRecord);
                    StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromNoteRecord(existingRecord, "note.update.api", User.Identity.Name));
                }
                else
                {
                    Response.StatusCode = 400;
                    return Json(OperationResponse.Failed("Invalid Record Id"));
                }
                return Json(OperationResponse.Succeed("Note Updated"));
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(OperationResponse.Failed(ex.Message));
            }
        }
    }
}
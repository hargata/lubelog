using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public partial class VehicleController
    {
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetNotesByVehicleId(int vehicleId)
        {
            var result = _noteDataAccess.GetNotesByVehicleId(vehicleId);
            result = result.OrderByDescending(x => x.Pinned).ToList();
            return PartialView("_Notes", result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetPinnedNotesByVehicleId(int vehicleId)
        {
            var result = _noteDataAccess.GetNotesByVehicleId(vehicleId);
            result = result.Where(x => x.Pinned).ToList();
            return Json(result);
        }
        [HttpPost]
        public IActionResult SaveNoteToVehicleId(Note note)
        {
            note.Files = note.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var result = _noteDataAccess.SaveNoteToVehicle(note);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), note.VehicleId, User.Identity.Name, $"{(note.Id == default ? "Created" : "Edited")} Note - Description: {note.Description}");
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddNotePartialView()
        {
            return PartialView("_NoteModal", new Note());
        }
        [HttpGet]
        public IActionResult GetNoteForEditById(int noteId)
        {
            var result = _noteDataAccess.GetNoteById(noteId);
            return PartialView("_NoteModal", result);
        }
        [HttpPost]
        public IActionResult DeleteNoteById(int noteId)
        {
            var result = _noteDataAccess.DeleteNoteById(noteId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Note - Id: {noteId}");
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult PinNotes(List<int> noteIds, bool isToggle = false, bool pinStatus = false)
        {
            var result = false;
            foreach (int noteId in noteIds)
            {
                var existingNote = _noteDataAccess.GetNoteById(noteId);
                if (isToggle)
                {
                    existingNote.Pinned = !existingNote.Pinned;
                }
                else
                {
                    existingNote.Pinned = pinStatus;
                }
                result = _noteDataAccess.SaveNoteToVehicle(existingNote);
            }
            return Json(result);
        }
    }
}

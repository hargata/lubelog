﻿using CarCareTracker.Filter;
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
            result = result.OrderByDescending(x => x.Pinned).ThenBy(x => x.Description).ToList();
            return PartialView("Note/_Notes", result);
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
        public async Task<IActionResult> SaveNoteToVehicleId(Note note)
        {
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), note.VehicleId))
            {
                return Json(false);
            }
            note.Files = note.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            bool isCreate = note.Id == default; //needed here since Notes don't use an input object.
            var result = _noteDataAccess.SaveNoteToVehicle(note);
            if (result)
            {
                await _notificationChannelService.WriteAsync(WebHookPayload.FromNoteRecord(note, isCreate ? "noterecord.add" : "noterecord.update", User.Identity.Name));
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddNotePartialView()
        {
            var extraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.NoteRecord).ExtraFields;
            return PartialView("Note/_NoteModal", new Note() { ExtraFields = extraFields });
        }
        [HttpGet]
        public IActionResult GetNoteForEditById(int noteId)
        {
            var result = _noteDataAccess.GetNoteById(noteId);
            result.ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.NoteRecord).ExtraFields);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), result.VehicleId))
            {
                return Redirect("/Error/Unauthorized");
            }
            return PartialView("Note/_NoteModal", result);
        }
        private async Task<bool> DeleteNoteWithChecks(int noteId)
        {
            var existingRecord = _noteDataAccess.GetNoteById(noteId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
            {
                return false;
            }
            var result = _noteDataAccess.DeleteNoteById(existingRecord.Id);
            if (result)
            {
                await _notificationChannelService.WriteAsync(WebHookPayload.FromNoteRecord(existingRecord, "noterecord.delete", User.Identity.Name));
            }
            return result;
        }
        [HttpPost]
        public async Task<IActionResult> DeleteNoteById(int noteId)
        {
            var result = await DeleteNoteWithChecks(noteId);
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

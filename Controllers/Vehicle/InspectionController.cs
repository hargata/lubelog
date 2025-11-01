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
        public IActionResult GetInspectionRecordTemplatesByVehicleId(int vehicleId)
        {
            var result = _inspectionRecordTemplateDataAccess.GetInspectionRecordTemplatesByVehicleId(vehicleId);
            return PartialView("Inspection/_InspectionRecordTemplateSelector", result);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetInspectionRecordsByVehicleId(int vehicleId)
        {
            var result = _inspectionRecordDataAccess.GetInspectionRecordsByVehicleId(vehicleId);
            bool _useDescending = _config.GetUserConfig(User).UseDescending;
            if (_useDescending)
            {
                result = result.OrderByDescending(x => x.Date).ThenByDescending(x => x.Mileage).ToList();
            }
            else
            {
                result = result.OrderBy(x => x.Date).ThenBy(x => x.Mileage).ToList();
            }
            return PartialView("Inspection/_InspectionRecords", result);
        }
        [HttpGet]
        public IActionResult GetAddInspectionRecordTemplatePartialView()
        {
            return PartialView("Inspection/_InspectionRecordTemplateEditModal", new InspectionRecordInput());
        }
        [HttpGet]
        public IActionResult GetEditInspectionRecordTemplatePartialView(int inspectionRecordTemplateId)
        {
            var result = _inspectionRecordTemplateDataAccess.GetInspectionRecordTemplateById(inspectionRecordTemplateId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), result.VehicleId))
            {
                return Redirect("/Error/Unauthorized");
            }
            return PartialView("Inspection/_InspectionRecordTemplateEditModal", result);
        }
        [HttpGet]
        public IActionResult GetAddInspectionRecordFieldPartialView()
        {
            return PartialView("Inspection/_InspectionRecordField", new InspectionRecordTemplateField());
        }
        public IActionResult GetAddInspectionRecordFieldOptionsPartialView()
        {
            return PartialView("Inspection/_InspectionRecordFieldOptions", new List<InspectionRecordTemplateFieldOption>());
        }
        public IActionResult GetAddInspectionRecordFieldOptionPartialView()
        {
            return PartialView("Inspection/_InspectionRecordFieldOption", new InspectionRecordTemplateFieldOption());
        }
        [HttpPost]
        public IActionResult SaveInspectionRecordTemplateToVehicleId(InspectionRecordInput inspectionRecordTemplate)
        {
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), inspectionRecordTemplate.VehicleId))
            {
                return Json(false);
            }
            var result = _inspectionRecordTemplateDataAccess.SaveInspectionReportTemplateToVehicle(inspectionRecordTemplate);
            return Json(result);
        }
        private bool DeleteInspectionRecordTemplateWithChecks(int inspectionRecordTemplateId)
        {
            var existingRecord = _inspectionRecordTemplateDataAccess.GetInspectionRecordTemplateById(inspectionRecordTemplateId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
            {
                return false;
            }
            var result = _inspectionRecordTemplateDataAccess.DeleteInspectionRecordTemplateById(existingRecord.Id);
            return result;
        }
        [HttpPost]
        public IActionResult DeleteInspectionRecordTemplateById(int inspectionRecordTemplateId)
        {
            var result = DeleteInspectionRecordTemplateWithChecks(inspectionRecordTemplateId);
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddInspectionRecordPartialView(int inspectionRecordTemplateId)
        {
            var result = _inspectionRecordTemplateDataAccess.GetInspectionRecordTemplateById(inspectionRecordTemplateId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), result.VehicleId))
            {
                return Redirect("/Error/Unauthorized");
            }
            return PartialView("Inspection/_InspectionRecordModal", result);
        }
        //[HttpGet]
        //public IActionResult GetAddInspectionRecordPartialView()
        //{
        //    var extraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.NoteRecord).ExtraFields;
        //    return PartialView("Note/_NoteModal", new Note() { ExtraFields = extraFields });
        //}
        //[HttpGet]
        //public IActionResult GetAddInspectionRecordTemplatePartialView()
        //{
        //    var extraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.NoteRecord).ExtraFields;
        //    return PartialView("Note/_NoteModal", new Note() { ExtraFields = extraFields });
        //}
        //[HttpGet]
        //public IActionResult GetInspectionRecordTemplateForEditById(int noteId)
        //{
        //    var result = _noteDataAccess.GetNoteById(noteId);
        //    result.ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.NoteRecord).ExtraFields);
        //    //security check.
        //    if (!_userLogic.UserCanEditVehicle(GetUserID(), result.VehicleId))
        //    {
        //        return Redirect("/Error/Unauthorized");
        //    }
        //    return PartialView("Note/_NoteModal", result);
        //}
        //private bool DeleteInspectionRecordWithChecks(int noteId)
        //{
        //    var existingRecord = _noteDataAccess.GetNoteById(noteId);
        //    //security check.
        //    if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
        //    {
        //        return false;
        //    }
        //    var result = _noteDataAccess.DeleteNoteById(existingRecord.Id);
        //    if (result)
        //    {
        //        StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromNoteRecord(existingRecord, "noterecord.delete", User.Identity.Name));
        //    }
        //    return result;
        //}
        //[HttpPost]
        //public IActionResult DeleteInspectionRecordById(int noteId)
        //{
        //    var result = DeleteInspectionRecordWithChecks(noteId);
        //    return Json(result);
        //}
        
    }
}

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
        private bool DeleteInspectionRecordWithChecks(int inspectionRecordId)
        {
            var existingRecord = _inspectionRecordDataAccess.GetInspectionRecordById(inspectionRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
            {
                return false;
            }
            var result = _inspectionRecordDataAccess.DeleteInspectionRecordById(existingRecord.Id);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromInspectionRecord(existingRecord, "inspectionrecord.delete", User.Identity.Name));
            }
            return result;
        }
        [HttpPost]
        public IActionResult DeleteInspectionRecordTemplateById(int inspectionRecordTemplateId)
        {
            var result = DeleteInspectionRecordTemplateWithChecks(inspectionRecordTemplateId);
            return Json(result);
        }
        [HttpPost]
        public IActionResult DeleteInspectionRecordById(int inspectionRecordId)
        {
            var result = DeleteInspectionRecordWithChecks(inspectionRecordId);
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
            //populate date
            result.Date = DateTime.Now.ToShortDateString();
            return PartialView("Inspection/_InspectionRecordModal", result);
        }
        [HttpGet]
        public IActionResult GetViewInspectionRecordPartialView(int inspectionRecordId)
        {
            var result = _inspectionRecordDataAccess.GetInspectionRecordById(inspectionRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), result.VehicleId))
            {
                return Redirect("/Error/Unauthorized");
            }
            return PartialView("Inspection/_InspectionRecordViewModal", result);
        }
        [HttpPost]
        public IActionResult SaveInspectionRecordToVehicleId(InspectionRecordInput inspectionRecord)
        {
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), inspectionRecord.VehicleId))
            {
                return Json(false);
            }
            //move files from temp.
            inspectionRecord.Files = inspectionRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            //push back any reminders
            if (inspectionRecord.ReminderRecordId.Any())
            {
                foreach (int reminderRecordId in inspectionRecord.ReminderRecordId)
                {
                    PushbackRecurringReminderRecordWithChecks(reminderRecordId, DateTime.Parse(inspectionRecord.Date), inspectionRecord.Mileage);
                }
            }
            var convertedRecord = inspectionRecord.ToInspectionRecord();
            var result = _inspectionRecordDataAccess.SaveInspectionRecordToVehicle(convertedRecord);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromInspectionRecord(convertedRecord, "inspectionrecord.add", User.Identity.Name));
            }
            if (convertedRecord.Id != 0)
            {
                //insert into service record
                List<UploadedFiles> newAttachments = new List<UploadedFiles>();
                newAttachments.Add(new UploadedFiles { Name = inspectionRecord.Description, Location = StaticHelper.GetRecordAttachment(ImportMode.InspectionRecord, convertedRecord.Id)});
                newAttachments.AddRange(inspectionRecord.Files);
                _serviceRecordDataAccess.SaveServiceRecordToVehicle(new ServiceRecord
                {
                    Date = DateTime.Parse(inspectionRecord.Date),
                    VehicleId = inspectionRecord.VehicleId,
                    Mileage = inspectionRecord.Mileage,
                    Description = inspectionRecord.Description,
                    Cost = inspectionRecord.Cost,
                    Notes = $"Auto Insert From Inspection Record: {inspectionRecord.Description}",
                    Files = newAttachments
                });
                //auto-insert into odometer if configured
                if (inspectionRecord.Id == default && _config.GetUserConfig(User).EnableAutoOdometerInsert)
                {
                    _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                    {
                        Date = DateTime.Parse(inspectionRecord.Date),
                        VehicleId = inspectionRecord.VehicleId,
                        Mileage = inspectionRecord.Mileage,
                        Notes = $"Auto Insert From Inspection Record: {inspectionRecord.Description}",
                        Files = StaticHelper.CreateAttachmentFromRecord(ImportMode.InspectionRecord, convertedRecord.Id, convertedRecord.Description)
                    });
                }
                //create action items
                var inspectionFieldsWithActionItems = inspectionRecord.Fields.Where(x => x.HasActionItem);
                if (inspectionFieldsWithActionItems.Any())
                {
                    foreach (InspectionRecordTemplateField inspectionField in inspectionFieldsWithActionItems)
                    {
                        if (inspectionField.ToInspectionRecordResult().Failed)
                        {
                            _planRecordDataAccess.SavePlanRecordToVehicle(new PlanRecord
                            {
                                DateCreated = DateTime.Now,
                                DateModified = DateTime.Now,
                                VehicleId = inspectionRecord.VehicleId,
                                Description = inspectionField.ActionItemDescription,
                                ImportMode = inspectionField.ActionItemType,
                                Priority = inspectionField.ActionItemPriority,
                                Notes = $"Auto Insert From Inspection Record: {inspectionRecord.Description}",
                                Files = StaticHelper.CreateAttachmentFromRecord(ImportMode.InspectionRecord, convertedRecord.Id, convertedRecord.Description)
                            });
                        }
                    }
                }
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult UpdateInspectionRecordTags(int inspectionRecordId, List<string> tags)
        {
            var existingRecord = _inspectionRecordDataAccess.GetInspectionRecordById(inspectionRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
            {
                return Json(false);
            }
            existingRecord.Tags = tags;
            var result = _inspectionRecordDataAccess.SaveInspectionRecordToVehicle(existingRecord);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), WebHookPayload.FromInspectionRecord(existingRecord, "inspectionrecord.update", User.Identity.Name));
            }
            return Json(result);
        }
    }
}

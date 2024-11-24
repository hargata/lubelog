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
        public IActionResult GetPlanRecordsByVehicleId(int vehicleId)
        {
            var result = _planRecordDataAccess.GetPlanRecordsByVehicleId(vehicleId);
            return PartialView("_PlanRecords", result);
        }
        [HttpPost]
        public IActionResult SavePlanRecordToVehicleId(PlanRecordInput planRecord)
        {
            //populate createdDate
            if (planRecord.Id == default)
            {
                planRecord.DateCreated = DateTime.Now.ToString("G");
            }
            planRecord.DateModified = DateTime.Now.ToString("G");
            //move files from temp.
            planRecord.Files = planRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            if (planRecord.Supplies.Any())
            {
                planRecord.RequisitionHistory.AddRange(RequisitionSupplyRecordsByUsage(planRecord.Supplies, DateTime.Parse(planRecord.DateCreated), planRecord.Description));
                if (planRecord.CopySuppliesAttachment)
                {
                    planRecord.Files.AddRange(GetSuppliesAttachments(planRecord.Supplies));
                }
            }
            if (planRecord.DeletedRequisitionHistory.Any())
            {
                RestoreSupplyRecordsByUsage(planRecord.DeletedRequisitionHistory, planRecord.Description);
            }
            var result = _planRecordDataAccess.SavePlanRecordToVehicle(planRecord.ToPlanRecord());
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), planRecord.VehicleId, User.Identity.Name, $"{(planRecord.Id == default ? "Created" : "Edited")} Plan Record - Description: {planRecord.Description}");
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult SavePlanRecordTemplateToVehicleId(PlanRecordInput planRecord)
        {
            //check if template name already taken.
            var existingRecord = _planRecordTemplateDataAccess.GetPlanRecordTemplatesByVehicleId(planRecord.VehicleId).Where(x => x.Description == planRecord.Description).Any();
            if (planRecord.Id == default && existingRecord)
            {
                return Json(OperationResponse.Failed("A template with that description already exists for this vehicle"));
            }
            planRecord.Files = planRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var result = _planRecordTemplateDataAccess.SavePlanRecordTemplateToVehicle(planRecord);
            return Json(OperationResponse.Conditional(result, "Template Added", string.Empty));
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetPlanRecordTemplatesForVehicleId(int vehicleId)
        {
            var result = _planRecordTemplateDataAccess.GetPlanRecordTemplatesByVehicleId(vehicleId);
            return PartialView("_PlanRecordTemplateModal", result);
        }
        [HttpPost]
        public IActionResult DeletePlanRecordTemplateById(int planRecordTemplateId)
        {
            var result = _planRecordTemplateDataAccess.DeletePlanRecordTemplateById(planRecordTemplateId);
            return Json(result);
        }
        [HttpGet]
        public IActionResult OrderPlanSupplies(int planRecordTemplateId)
        {
            var existingRecord = _planRecordTemplateDataAccess.GetPlanRecordTemplateById(planRecordTemplateId);
            if (existingRecord.Id == default)
            {
                return Json(OperationResponse.Failed("Unable to find template"));
            }
            if (existingRecord.Supplies.Any())
            {
                var suppliesToOrder = CheckSupplyRecordsAvailability(existingRecord.Supplies);
                return PartialView("_PlanOrderSupplies", suppliesToOrder);
            } 
            else
            {
                return Json(OperationResponse.Failed("Template has No Supplies"));
            }
        }
        [HttpPost]
        public IActionResult ConvertPlanRecordTemplateToPlanRecord(int planRecordTemplateId)
        {
            var existingRecord = _planRecordTemplateDataAccess.GetPlanRecordTemplateById(planRecordTemplateId);
            if (existingRecord.Id == default)
            {
                return Json(OperationResponse.Failed("Unable to find template"));
            }
            if (existingRecord.Supplies.Any())
            {
                //check if all supplies are available
                var supplyAvailability = CheckSupplyRecordsAvailability(existingRecord.Supplies);
                if (supplyAvailability.Any(x => x.Missing))
                {
                    return Json(OperationResponse.Failed("Missing Supplies, Please Delete This Template and Recreate It."));
                }
                else if (supplyAvailability.Any(x => x.Insufficient))
                {
                    return Json(OperationResponse.Failed("Insufficient Supplies"));
                }
            }
            if (existingRecord.ReminderRecordId != default)
            {
                //check if reminder still exists and is still recurring.
                var existingReminder = _reminderRecordDataAccess.GetReminderRecordById(existingRecord.ReminderRecordId);
                if (existingReminder is null || existingReminder.Id == default || !existingReminder.IsRecurring)
                {
                    return Json(OperationResponse.Failed("Missing or Non-recurring Reminder, Please Delete This Template and Recreate It."));
                }
            }
            //populate createdDate
            existingRecord.DateCreated = DateTime.Now.ToString("G");
            existingRecord.DateModified = DateTime.Now.ToString("G");
            existingRecord.Id = default;
            if (existingRecord.Supplies.Any())
            {
                existingRecord.RequisitionHistory = RequisitionSupplyRecordsByUsage(existingRecord.Supplies, DateTime.Parse(existingRecord.DateCreated), existingRecord.Description);
                if (existingRecord.CopySuppliesAttachment)
                {
                    existingRecord.Files.AddRange(GetSuppliesAttachments(existingRecord.Supplies));
                }
            }
            var result = _planRecordDataAccess.SavePlanRecordToVehicle(existingRecord.ToPlanRecord());
            return Json(OperationResponse.Conditional(result, "Plan Record Added", string.Empty));
        }
        [HttpGet]
        public IActionResult GetAddPlanRecordPartialView()
        {
            return PartialView("_PlanRecordModal", new PlanRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.PlanRecord).ExtraFields });
        }
        [HttpPost]
        public IActionResult GetAddPlanRecordPartialView(PlanRecordInput? planModel)
        {
            if (planModel is not null)
            {
                planModel.ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.PlanRecord).ExtraFields;
                return PartialView("_PlanRecordModal", planModel);
            }
            return PartialView("_PlanRecordModal", new PlanRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.PlanRecord).ExtraFields });
        }
        [HttpPost]
        public IActionResult UpdatePlanRecordProgress(int planRecordId, PlanProgress planProgress, int odometer = 0)
        {
            if (planRecordId == default)
            {
                return Json(false);
            }
            var existingRecord = _planRecordDataAccess.GetPlanRecordById(planRecordId);
            existingRecord.Progress = planProgress;
            existingRecord.DateModified = DateTime.Now;
            var result = _planRecordDataAccess.SavePlanRecordToVehicle(existingRecord);
            if (planProgress == PlanProgress.Done)
            {
                if (_config.GetUserConfig(User).EnableAutoOdometerInsert)
                {
                    _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                    {
                        Date = DateTime.Now.Date,
                        VehicleId = existingRecord.VehicleId,
                        Mileage = odometer,
                        Notes = $"Auto Insert From Plan Record: {existingRecord.Description}",
                        ExtraFields = existingRecord.ExtraFields
                    });
                }
                //convert plan record to service/upgrade/repair record.
                if (existingRecord.ImportMode == ImportMode.ServiceRecord)
                {
                    var newRecord = new ServiceRecord()
                    {
                        VehicleId = existingRecord.VehicleId,
                        Date = DateTime.Now.Date,
                        Mileage = odometer,
                        Description = existingRecord.Description,
                        Cost = existingRecord.Cost,
                        Notes = existingRecord.Notes,
                        Files = existingRecord.Files,
                        RequisitionHistory = existingRecord.RequisitionHistory,
                        ExtraFields = existingRecord.ExtraFields
                    };
                    _serviceRecordDataAccess.SaveServiceRecordToVehicle(newRecord);
                }
                else if (existingRecord.ImportMode == ImportMode.RepairRecord)
                {
                    var newRecord = new CollisionRecord()
                    {
                        VehicleId = existingRecord.VehicleId,
                        Date = DateTime.Now.Date,
                        Mileage = odometer,
                        Description = existingRecord.Description,
                        Cost = existingRecord.Cost,
                        Notes = existingRecord.Notes,
                        Files = existingRecord.Files,
                        RequisitionHistory = existingRecord.RequisitionHistory,
                        ExtraFields = existingRecord.ExtraFields
                    };
                    _collisionRecordDataAccess.SaveCollisionRecordToVehicle(newRecord);
                }
                else if (existingRecord.ImportMode == ImportMode.UpgradeRecord)
                {
                    var newRecord = new UpgradeRecord()
                    {
                        VehicleId = existingRecord.VehicleId,
                        Date = DateTime.Now.Date,
                        Mileage = odometer,
                        Description = existingRecord.Description,
                        Cost = existingRecord.Cost,
                        Notes = existingRecord.Notes,
                        Files = existingRecord.Files,
                        RequisitionHistory = existingRecord.RequisitionHistory,
                        ExtraFields = existingRecord.ExtraFields
                    };
                    _upgradeRecordDataAccess.SaveUpgradeRecordToVehicle(newRecord);
                }
                //push back any reminders
                if (existingRecord.ReminderRecordId != default)
                {
                    PushbackRecurringReminderRecordWithChecks(existingRecord.ReminderRecordId, DateTime.Now, odometer);
                }
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetPlanRecordTemplateForEditById(int planRecordTemplateId)
        {
            var result = _planRecordTemplateDataAccess.GetPlanRecordTemplateById(planRecordTemplateId);
            return PartialView("_PlanRecordTemplateEditModal", result);
        }
        [HttpGet]
        public IActionResult GetPlanRecordForEditById(int planRecordId)
        {
            var result = _planRecordDataAccess.GetPlanRecordById(planRecordId);
            //convert to Input object.
            var convertedResult = new PlanRecordInput
            {
                Id = result.Id,
                Description = result.Description,
                DateCreated = result.DateCreated.ToString("G"),
                DateModified = result.DateModified.ToString("G"),
                ImportMode = result.ImportMode,
                Priority = result.Priority,
                Progress = result.Progress,
                Cost = result.Cost,
                Notes = result.Notes,
                VehicleId = result.VehicleId,
                Files = result.Files,
                RequisitionHistory = result.RequisitionHistory,
                ReminderRecordId = result.ReminderRecordId,
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.PlanRecord).ExtraFields)
            };
            return PartialView("_PlanRecordModal", convertedResult);
        }
        [HttpPost]
        public IActionResult DeletePlanRecordById(int planRecordId)
        {
            var existingRecord = _planRecordDataAccess.GetPlanRecordById(planRecordId);
            //security check.
            if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId))
            {
                return Json(false);
            }
            //restore any requisitioned supplies if it has not been converted to other record types.
            if (existingRecord.RequisitionHistory.Any() && existingRecord.Progress != PlanProgress.Done)
            {
                RestoreSupplyRecordsByUsage(existingRecord.RequisitionHistory, existingRecord.Description);
            }
            var result = _planRecordDataAccess.DeletePlanRecordById(existingRecord.Id);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Plan Record - Id: {planRecordId}");
            }
            return Json(result);
        }
    }
}

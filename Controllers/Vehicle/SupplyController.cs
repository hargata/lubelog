using CarCareTracker.Filter;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace CarCareTracker.Controllers
{
    public partial class VehicleController
    {
        private List<SupplyAvailability> CheckSupplyRecordsAvailability(List<SupplyUsage> supplyUsage)
        {
            //returns empty string if all supplies are available
            var result = new List<SupplyAvailability>();
            foreach (SupplyUsage supply in supplyUsage)
            {
                //get supply record.
                var supplyData = _supplyRecordDataAccess.GetSupplyRecordById(supply.SupplyId);
                if (supplyData == null)
                {
                    result.Add(new SupplyAvailability { Missing = true });
                }
                else
                {
                    result.Add(new SupplyAvailability { Missing = false, Description = supplyData.Description, Required = supply.Quantity, InStock = supplyData.Quantity });
                }
            }
            return result;
        }
        private List<UploadedFiles> GetSuppliesAttachments(List<SupplyUsage> supplyUsage)
        {
            List<UploadedFiles> results = new List<UploadedFiles>();
            foreach (SupplyUsage supply in supplyUsage)
            {
                var result = _supplyRecordDataAccess.GetSupplyRecordById(supply.SupplyId);
                results.AddRange(result.Files);
            }
            return results;
        }
        private List<SupplyUsageHistory> RequisitionSupplyRecordsByUsage(List<SupplyUsage> supplyUsage, DateTime dateRequisitioned, string usageDescription)
        {
            List<SupplyUsageHistory> results = new List<SupplyUsageHistory>();
            foreach (SupplyUsage supply in supplyUsage)
            {
                //get supply record.
                var result = _supplyRecordDataAccess.GetSupplyRecordById(supply.SupplyId);
                var unitCost = (result.Quantity != 0) ? result.Cost / result.Quantity : 0;
                //deduct quantity used.
                result.Quantity -= supply.Quantity;
                //deduct cost.
                result.Cost -= (supply.Quantity * unitCost);
                //check decimal places to ensure that it always has a max of 3 decimal places.
                var roundedDecimal = decimal.Round(result.Cost, 3);
                if (roundedDecimal != result.Cost)
                {
                    //Too many decimals
                    result.Cost = roundedDecimal;
                }
                //create new requisitionrrecord
                var requisitionRecord = new SupplyUsageHistory
                {
                    Id = supply.SupplyId,
                    Date = dateRequisitioned,
                    Description = usageDescription,
                    Quantity = supply.Quantity,
                    Cost = (supply.Quantity * unitCost)
                };
                result.RequisitionHistory.Add(requisitionRecord);
                //save
                _supplyRecordDataAccess.SaveSupplyRecordToVehicle(result);
                requisitionRecord.Description = result.Description; //change the name of the description for plan/service/repair/upgrade records
                requisitionRecord.PartNumber = result.PartNumber; //populate part number if not displayed in supplies modal.
                results.Add(requisitionRecord);
            }
            return results;
        }
        
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetSupplyRecordsByVehicleId(int vehicleId)
        {
            var result = _supplyRecordDataAccess.GetSupplyRecordsByVehicleId(vehicleId);
            bool _useDescending = _config.GetUserConfig(User).UseDescending;
            if (_useDescending)
            {
                result = result.OrderByDescending(x => x.Date).ToList();
            }
            else
            {
                result = result.OrderBy(x => x.Date).ToList();
            }
            return PartialView("Supply/_SupplyRecords", result);
        }
        [HttpGet]
        public IActionResult GetSupplyRecordsForPlanRecordTemplate(int planRecordTemplateId)
        {
            var viewModel = new SupplyUsageViewModel();
            var planRecordTemplate = _planRecordTemplateDataAccess.GetPlanRecordTemplateById(planRecordTemplateId);
            if (planRecordTemplate != default && planRecordTemplate.VehicleId != default)
            {
                var supplies = _supplyRecordDataAccess.GetSupplyRecordsByVehicleId(planRecordTemplate.VehicleId);
                if (_config.GetServerEnableShopSupplies())
                {
                    supplies.AddRange(_supplyRecordDataAccess.GetSupplyRecordsByVehicleId(0)); // add shop supplies
                }
                supplies.RemoveAll(x => x.Quantity <= 0);
                bool _useDescending = _config.GetUserConfig(User).UseDescending;
                if (_useDescending)
                {
                    supplies = supplies.OrderByDescending(x => x.Date).ToList();
                }
                else
                {
                    supplies = supplies.OrderBy(x => x.Date).ToList();
                }
                viewModel.Supplies = supplies;
                viewModel.Usage = planRecordTemplate.Supplies;
            }
            return PartialView("Supply/_SupplyUsage", viewModel);
        }
        [TypeFilter(typeof(CollaboratorFilter))]
        [HttpGet]
        public IActionResult GetSupplyRecordsForRecordsByVehicleId(int vehicleId)
        {
            var result = _supplyRecordDataAccess.GetSupplyRecordsByVehicleId(vehicleId);
            if (_config.GetServerEnableShopSupplies())
            {
                result.AddRange(_supplyRecordDataAccess.GetSupplyRecordsByVehicleId(0)); // add shop supplies
            }
            result.RemoveAll(x => x.Quantity <= 0);
            bool _useDescending = _config.GetUserConfig(User).UseDescending;
            if (_useDescending)
            {
                result = result.OrderByDescending(x => x.Date).ToList();
            }
            else
            {
                result = result.OrderBy(x => x.Date).ToList();
            }
            var viewModel = new SupplyUsageViewModel
            {
                Supplies = result
            };
            return PartialView("Supply/_SupplyUsage", viewModel);
        }
        [HttpPost]
        public IActionResult SaveSupplyRecordToVehicleId(SupplyRecordInput supplyRecord)
        {
            if (supplyRecord.VehicleId != default)
            {
                //security check only if not editing shop supply.
                if (!_userLogic.UserCanEditVehicle(GetUserID(), supplyRecord.VehicleId, HouseholdPermission.Edit))
                {
                    return Json(OperationResponse.Failed("Access Denied"));
                }
            }
            else if (!_config.GetServerEnableShopSupplies())
            {
                return Json(OperationResponse.Failed("Access Denied"));
            }
            //move files from temp.
            supplyRecord.Files = supplyRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var result = _supplyRecordDataAccess.SaveSupplyRecordToVehicle(supplyRecord.ToSupplyRecord());
            if (result)
            {
                _eventLogic.PublishEvent(WebHookPayload.FromSupplyRecord(supplyRecord.ToSupplyRecord(), supplyRecord.Id == default ? "supplyrecord.add" : "supplyrecord.update", User.Identity?.Name ?? string.Empty));
            }
            return Json(OperationResponse.Conditional(result, string.Empty, StaticHelper.GenericErrorMessage));
        }
        [HttpGet]
        public IActionResult GetAddSupplyRecordPartialView()
        {
            return PartialView("Supply/_SupplyRecordModal", new SupplyRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.SupplyRecord).ExtraFields });
        }
        [HttpGet]
        public IActionResult GetSupplyRecordForEditById(int supplyRecordId)
        {
            var result = _supplyRecordDataAccess.GetSupplyRecordById(supplyRecordId);
            if (result.VehicleId != default)
            {
                //security check only if not editing shop supply.
                if (!_userLogic.UserCanEditVehicle(GetUserID(), result.VehicleId, HouseholdPermission.View))
                {
                    return Redirect("/Error/Unauthorized");
                }
            }
            else if (!_config.GetServerEnableShopSupplies())
            {
                return Redirect("/Error/Unauthorized");
            }
            if (result.RequisitionHistory.Any())
            {
                //requisition history when viewed through the supply is always immutable.
                result.RequisitionHistory = result.RequisitionHistory.Select(x => new SupplyUsageHistory { Id = default, Cost = x.Cost, Description = x.Description, Date = x.Date, PartNumber = x.PartNumber, Quantity = x.Quantity }).ToList();
            }
            //convert to Input object.
            var convertedResult = new SupplyRecordInput
            {
                Id = result.Id,
                Cost = result.Cost,
                Date = result.Date.ToShortDateString(),
                Description = result.Description,
                PartNumber = result.PartNumber,
                Quantity = result.Quantity,
                PartSupplier = result.PartSupplier,
                Notes = result.Notes,
                VehicleId = result.VehicleId,
                Files = result.Files,
                Tags = result.Tags,
                RequisitionHistory = result.RequisitionHistory,
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.SupplyRecord).ExtraFields)
            };
            return PartialView("Supply/_SupplyRecordModal", convertedResult);
        }
        private OperationResponse DeleteSupplyRecordWithChecks(int supplyRecordId)
        {
            var existingRecord = _supplyRecordDataAccess.GetSupplyRecordById(supplyRecordId);
            if (existingRecord.VehicleId != default)
            {
                //security check only if not editing shop supply.
                if (!_userLogic.UserCanEditVehicle(GetUserID(), existingRecord.VehicleId, HouseholdPermission.Delete))
                {
                    return OperationResponse.Failed("Access Denied");
                }
            }
            else if (!_config.GetServerEnableShopSupplies())
            {
                return OperationResponse.Failed("Access Denied");
            }
            var result = _supplyRecordDataAccess.DeleteSupplyRecordById(existingRecord.Id);
            if (result)
            {
                _eventLogic.PublishEvent(WebHookPayload.FromSupplyRecord(existingRecord, "supplyrecord.delete", User.Identity?.Name ?? string.Empty));
            }
            return OperationResponse.Conditional(result, string.Empty, StaticHelper.GenericErrorMessage);
        }
        [HttpPost]
        public IActionResult DeleteSupplyRecordById(int supplyRecordId)
        {
            var result = DeleteSupplyRecordWithChecks(supplyRecordId);
            return Json(result);
        }
    }
}

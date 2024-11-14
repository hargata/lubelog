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
        public IActionResult GetGasRecordsByVehicleId(int vehicleId)
        {
            var result = _gasRecordDataAccess.GetGasRecordsByVehicleId(vehicleId);
            //check if the user uses MPG or Liters per 100km.
            var userConfig = _config.GetUserConfig(User);
            bool useMPG = userConfig.UseMPG;
            bool useUKMPG = userConfig.UseUKMPG;
            var computedResults = _gasHelper.GetGasRecordViewModels(result, useMPG, useUKMPG);
            if (userConfig.UseDescending)
            {
                computedResults = computedResults.OrderByDescending(x => DateTime.Parse(x.Date)).ThenByDescending(x => x.Mileage).ToList();
            }
            var vehicleData = _dataAccess.GetVehicleById(vehicleId);
            var vehicleIsElectric = vehicleData.IsElectric;
            var vehicleUseHours = vehicleData.UseHours;
            var viewModel = new GasRecordViewModelContainer()
            {
                UseKwh = vehicleIsElectric,
                UseHours = vehicleUseHours,
                GasRecords = computedResults
            };
            return PartialView("_Gas", viewModel);
        }
        [HttpPost]
        public IActionResult SaveGasRecordToVehicleId(GasRecordInput gasRecord)
        {
            if (gasRecord.Id == default && _config.GetUserConfig(User).EnableAutoOdometerInsert)
            {
                _odometerLogic.AutoInsertOdometerRecord(new OdometerRecord
                {
                    Date = DateTime.Parse(gasRecord.Date),
                    VehicleId = gasRecord.VehicleId,
                    Mileage = gasRecord.Mileage,
                    Notes = $"Auto Insert From Gas Record. {gasRecord.Notes}"
                });
            }
            gasRecord.Files = gasRecord.Files.Select(x => { return new UploadedFiles { Name = x.Name, Location = _fileHelper.MoveFileFromTemp(x.Location, "documents/") }; }).ToList();
            var result = _gasRecordDataAccess.SaveGasRecordToVehicle(gasRecord.ToGasRecord());
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), gasRecord.VehicleId, User.Identity.Name, $"{(gasRecord.Id == default ? "Created" : "Edited")} Gas Record - Mileage: {gasRecord.Mileage.ToString()}");
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult GetAddGasRecordPartialView(int vehicleId)
        {
            var vehicleData = _dataAccess.GetVehicleById(vehicleId);
            var vehicleIsElectric = vehicleData.IsElectric;
            var vehicleUseHours = vehicleData.UseHours;
            return PartialView("_GasModal", new GasRecordInputContainer() { UseKwh = vehicleIsElectric, UseHours = vehicleUseHours, GasRecord = new GasRecordInput() { ExtraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.GasRecord).ExtraFields } });
        }
        [HttpGet]
        public IActionResult GetGasRecordForEditById(int gasRecordId)
        {
            var result = _gasRecordDataAccess.GetGasRecordById(gasRecordId);
            var convertedResult = new GasRecordInput
            {
                Id = result.Id,
                Mileage = result.Mileage,
                VehicleId = result.VehicleId,
                Cost = result.Cost,
                Date = result.Date.ToShortDateString(),
                Files = result.Files,
                Gallons = result.Gallons,
                IsFillToFull = result.IsFillToFull,
                MissedFuelUp = result.MissedFuelUp,
                Notes = result.Notes,
                Tags = result.Tags,
                ExtraFields = StaticHelper.AddExtraFields(result.ExtraFields, _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.GasRecord).ExtraFields)
            };
            var vehicleData = _dataAccess.GetVehicleById(convertedResult.VehicleId);
            var vehicleIsElectric = vehicleData.IsElectric;
            var vehicleUseHours = vehicleData.UseHours;
            var viewModel = new GasRecordInputContainer()
            {
                UseKwh = vehicleIsElectric,
                UseHours = vehicleUseHours,
                GasRecord = convertedResult
            };
            return PartialView("_GasModal", viewModel);
        }
        [HttpPost]
        public IActionResult DeleteGasRecordById(int gasRecordId)
        {
            var result = _gasRecordDataAccess.DeleteGasRecordById(gasRecordId);
            if (result)
            {
                StaticHelper.NotifyAsync(_config.GetWebHookUrl(), 0, User.Identity.Name, $"Deleted Gas Record - Id: {gasRecordId}");
            }
            return Json(result);
        }
        [HttpPost]
        public IActionResult SaveUserGasTabPreferences(string gasUnit, string fuelMileageUnit)
        {
            var currentConfig = _config.GetUserConfig(User);
            currentConfig.PreferredGasUnit = gasUnit;
            currentConfig.PreferredGasMileageUnit = fuelMileageUnit;
            var result = _config.SaveUserConfig(User, currentConfig);
            return Json(result);
        }
        [HttpPost]
        public IActionResult GetGasRecordsEditModal(List<int> recordIds)
        {
            var extraFields = _extraFieldDataAccess.GetExtraFieldsById((int)ImportMode.GasRecord).ExtraFields;
            return PartialView("_GasRecordsModal", new GasRecordEditModel { RecordIds = recordIds, EditRecord = new GasRecord { ExtraFields = extraFields } });
        }
        [HttpPost]
        public IActionResult SaveMultipleGasRecords(GasRecordEditModel editModel)
        {
            var dateIsEdited = editModel.EditRecord.Date != default;
            var mileageIsEdited = editModel.EditRecord.Mileage != default;
            var consumptionIsEdited = editModel.EditRecord.Gallons != default;
            var costIsEdited = editModel.EditRecord.Cost != default;
            var noteIsEdited = !string.IsNullOrWhiteSpace(editModel.EditRecord.Notes);
            var tagsIsEdited = editModel.EditRecord.Tags.Any();
            var extraFieldIsEdited = editModel.EditRecord.ExtraFields.Any();
            //handle clear overrides
            if (tagsIsEdited && editModel.EditRecord.Tags.Contains("---"))
            {
                editModel.EditRecord.Tags = new List<string>();
            }
            if (noteIsEdited && editModel.EditRecord.Notes == "---")
            {
                editModel.EditRecord.Notes = "";
            }
            bool result = false;
            foreach (int recordId in editModel.RecordIds)
            {
                var existingRecord = _gasRecordDataAccess.GetGasRecordById(recordId);
                if (dateIsEdited)
                {
                    existingRecord.Date = editModel.EditRecord.Date;
                }
                if (consumptionIsEdited)
                {
                    existingRecord.Gallons = editModel.EditRecord.Gallons;
                }
                if (costIsEdited)
                {
                    existingRecord.Cost = editModel.EditRecord.Cost;
                }
                if (mileageIsEdited)
                {
                    existingRecord.Mileage = editModel.EditRecord.Mileage;
                }
                if (noteIsEdited)
                {
                    existingRecord.Notes = editModel.EditRecord.Notes;
                }
                if (tagsIsEdited)
                {
                    existingRecord.Tags = editModel.EditRecord.Tags;
                }
                if (extraFieldIsEdited)
                {
                    foreach (ExtraField extraField in editModel.EditRecord.ExtraFields)
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
                result = _gasRecordDataAccess.SaveGasRecordToVehicle(existingRecord);
            }
            return Json(result);
        }
    }
}

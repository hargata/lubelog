using CarCareTracker.Models;

namespace CarCareTracker.Helper
{
    public interface IEquipmentHelper
    {
        List<EquipmentRecordViewModel> GetEquipmentRecordViewModels(List<EquipmentRecord>  equipmentRecords, List<OdometerRecord> odometerRecords);
        EquipmentRecordStickerViewModel GetEquipmentRecordStickerViewModel(EquipmentRecord equipmentRecord, List<OdometerRecord> odometerRecords);
    }
    public class EquipmentHelper : IEquipmentHelper
    {
        public List<EquipmentRecordViewModel> GetEquipmentRecordViewModels(List<EquipmentRecord> equipmentRecords, List<OdometerRecord> odometerRecords)
        {
            List<EquipmentRecordViewModel> result = new List<EquipmentRecordViewModel>();
            foreach (EquipmentRecord equipmentRecord in equipmentRecords)
            {
                var distanceTraveled = odometerRecords.Where(x => x.EquipmentRecordId.Contains(equipmentRecord.Id)).Sum(y => y.DistanceTraveled);
                result.Add(new EquipmentRecordViewModel
                {
                    Id = equipmentRecord.Id,
                    DistanceTraveled = distanceTraveled,
                    Description = equipmentRecord.Description,
                    IsEquipped = equipmentRecord.IsEquipped,
                    VehicleId = equipmentRecord.VehicleId,
                    ExtraFields = equipmentRecord.ExtraFields,
                    Files = equipmentRecord.Files,
                    Notes = equipmentRecord.Notes,
                    Tags = equipmentRecord.Tags
                });
            }
            return result;
        }
        public EquipmentRecordStickerViewModel GetEquipmentRecordStickerViewModel(EquipmentRecord equipmentRecord, List<OdometerRecord> odometerRecords)
        {
            var linkedOdometerRecords = odometerRecords.Where(x => x.EquipmentRecordId.Contains(equipmentRecord.Id)).ToList();
            return new EquipmentRecordStickerViewModel {
                Description = equipmentRecord.Description,
                IsEquipped = equipmentRecord.IsEquipped,
                Notes = equipmentRecord.Notes,
                ExtraFields = equipmentRecord.ExtraFields,
                Distance = linkedOdometerRecords.Sum(x=>x.DistanceTraveled),
                OdometerRecords = linkedOdometerRecords
            };
        }
    }
}

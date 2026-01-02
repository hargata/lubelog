using CarCareTracker.Models;

namespace CarCareTracker.Helper
{
    public interface IEquipmentHelper
    {
        List<EquipmentRecordViewModel> GetEquipmentRecordViewModels(List<EquipmentRecord>  equipmentRecords, List<OdometerRecord> odometerRecords);
    }
    public class EquipmentHelper: IEquipmentHelper
    {
        public List<EquipmentRecordViewModel> GetEquipmentRecordViewModels(List<EquipmentRecord> equipmentRecords, List<OdometerRecord> odometerRecords)
        {
            List<EquipmentRecordViewModel> result = new List<EquipmentRecordViewModel>();
            foreach(EquipmentRecord equipmentRecord in equipmentRecords)
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
    }
}

namespace CarCareTracker.Models
{
    public class OdometerRecordEditModel
    {
        public List<int> RecordIds { get; set; } = new List<int>();
        public OdometerRecord EditRecord { get; set; } = new OdometerRecord();
        public bool EditEquipment { get; set; } = false;
        public List<EquipmentRecord> EquipmentRecords { get; set; } = new List<EquipmentRecord>();
    }
}

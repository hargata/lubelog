namespace CarCareTracker.Models
{
    public class EquipmentRecordStickerViewModel
    {
        public string Description { get; set; }
        public bool IsEquipped { get; set; }
        public string Notes { get; set; }
        public int Distance { get; set; }
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<OdometerRecord> OdometerRecords { get; set; } = new List<OdometerRecord>();
    }
}
namespace CarCareTracker.Models
{
    public class EquipmentRecord
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsEquipped { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
    }
}
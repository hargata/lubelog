namespace CarCareTracker.Models
{
    public class VehicleRecords
    {
        public List<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
        public List<CollisionRecord> CollisionRecords { get; set; } = new List<CollisionRecord>();
        public List<UpgradeRecord> UpgradeRecords { get; set; } = new List<UpgradeRecord>();
        public List<GasRecord> GasRecords { get; set; } = new List<GasRecord>();
        public List<TaxRecord> TaxRecords { get; set; } = new List<TaxRecord>();
        public List<OdometerRecord> OdometerRecords { get; set; } = new List<OdometerRecord>();
    }
}

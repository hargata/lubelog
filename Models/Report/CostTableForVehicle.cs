namespace CarCareTracker.Models
{
    public class CostTableForVehicle
    {
        public string DistanceUnit { get; set; } = "Per Mile";
        public int TotalDistance { get; set; }
        public int NumberOfDays { get; set; }
        public decimal ServiceRecordSum { get; set; }
        public decimal GasRecordSum { get; set; }
        public decimal TaxRecordSum { get; set; }
        public decimal CollisionRecordSum { get; set; }
        public decimal UpgradeRecordSum { get; set; }
        public decimal ServiceRecordPerMile { get { return TotalDistance != default ? ServiceRecordSum / TotalDistance : 0; } }
        public decimal GasRecordPerMile { get { return TotalDistance != default ? GasRecordSum / TotalDistance : 0; } }
        public decimal CollisionRecordPerMile { get { return TotalDistance != default ? CollisionRecordSum / TotalDistance : 0; } }
        public decimal UpgradeRecordPerMile { get { return TotalDistance != default ? UpgradeRecordSum / TotalDistance : 0; } }
        public decimal ServiceRecordPerMonth { get { return NumberOfDays != default ? ServiceRecordSum / NumberOfDays : 0; } }
        public decimal GasRecordPerMonth { get { return NumberOfDays != default ? GasRecordSum / NumberOfDays : 0; } }
        public decimal CollisionRecordPerMonth { get { return NumberOfDays != default ? CollisionRecordSum / NumberOfDays : 0; } }
        public decimal UpgradeRecordPerMonth { get { return NumberOfDays != default ? UpgradeRecordSum / NumberOfDays : 0; } }
        public decimal TaxRecordPerMonth { get { return NumberOfDays != default ? TaxRecordSum / NumberOfDays : 0; } }
        public decimal TotalPerMonth { get { return ServiceRecordPerMonth + CollisionRecordPerMonth + UpgradeRecordPerMonth + GasRecordPerMonth + TaxRecordPerMonth; } }
        public decimal TotalPerMile { get { return ServiceRecordPerMile + CollisionRecordPerMile + UpgradeRecordPerMile + GasRecordPerMile; } }
        public decimal TotalCost { get { return ServiceRecordSum + CollisionRecordSum + UpgradeRecordSum + GasRecordSum + TaxRecordSum; } }
    }
}

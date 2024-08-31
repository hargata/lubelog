namespace CarCareTracker.Models
{
    public class CostTableForVehicle
    {
        public string DistanceUnit { get; set; } = "Per Mile";
        public int TotalDistance { get; set; }
        public int NumberOfMonths { get; set; }
        public decimal ServiceRecordSum { get; set; }
        public decimal GasRecordSum { get; set; }
        public decimal TaxRecordSum { get; set; }
        public decimal CollisionRecordSum { get; set; }
        public decimal UpgradeRecordSum { get; set; }
        public decimal ServiceRecordPerMile { get { return TotalDistance != default ? ServiceRecordSum / TotalDistance : 0; } }
        public decimal GasRecordPerMile { get { return TotalDistance != default ? GasRecordSum / TotalDistance : 0; } }
        public decimal CollisionRecordPerMile { get { return TotalDistance != default ? CollisionRecordSum / TotalDistance : 0; } }
        public decimal UpgradeRecordPerMile { get { return TotalDistance != default ? UpgradeRecordSum / TotalDistance : 0; } }
        public decimal ServiceRecordPerMonth { get { return NumberOfMonths != default ? ServiceRecordSum / NumberOfMonths : 0; } }
        public decimal GasRecordPerMonth { get { return NumberOfMonths != default ? GasRecordSum / NumberOfMonths : 0; } }
        public decimal CollisionRecordPerMonth { get { return NumberOfMonths != default ? CollisionRecordSum / NumberOfMonths : 0; } }
        public decimal UpgradeRecordPerMonth { get { return NumberOfMonths != default ? UpgradeRecordSum / NumberOfMonths : 0; } }
        public decimal TaxRecordPerMonth { get { return NumberOfMonths != default ? TaxRecordSum / NumberOfMonths : 0; } }
        public decimal TotalPerMonth { get { return ServiceRecordPerMonth + CollisionRecordPerMonth + UpgradeRecordPerMonth + GasRecordPerMonth + TaxRecordPerMonth; } }
        public decimal TotalPerMile { get { return ServiceRecordPerMile + CollisionRecordPerMile + UpgradeRecordPerMile + GasRecordPerMile; } }
        public decimal TotalCost { get { return ServiceRecordSum + CollisionRecordSum + UpgradeRecordSum + GasRecordSum + TaxRecordSum; } }
    }
}

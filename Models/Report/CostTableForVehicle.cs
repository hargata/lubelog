namespace CarCareTracker.Models
{
    public class CostTableForVehicle
    {
        public string DistanceUnit { get; set; } = "Cost Per Mile";
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
        public decimal TaxRecordPerMile { get { return TotalDistance != default ? TaxRecordSum / TotalDistance : 0; } }
        public decimal ServiceRecordPerDay { get { return NumberOfDays != default ? ServiceRecordSum / NumberOfDays : 0; } }
        public decimal GasRecordPerDay { get { return NumberOfDays != default ? GasRecordSum / NumberOfDays : 0; } }
        public decimal CollisionRecordPerDay { get { return NumberOfDays != default ? CollisionRecordSum / NumberOfDays : 0; } }
        public decimal UpgradeRecordPerDay { get { return NumberOfDays != default ? UpgradeRecordSum / NumberOfDays : 0; } }
        public decimal TaxRecordPerDay { get { return NumberOfDays != default ? TaxRecordSum / NumberOfDays : 0; } }
        public decimal TotalPerDay { get { return ServiceRecordPerDay + CollisionRecordPerDay + UpgradeRecordPerDay + GasRecordPerDay + TaxRecordPerDay; } }
        public decimal TotalPerMile { get { return ServiceRecordPerMile + CollisionRecordPerMile + UpgradeRecordPerMile + GasRecordPerMile + TaxRecordPerMile; } }
        public decimal TotalCost { get { return ServiceRecordSum + CollisionRecordSum + UpgradeRecordSum + GasRecordSum + TaxRecordSum; } }
    }
}

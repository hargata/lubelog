namespace CarCareTracker.Models
{
    public class KioskVehicleViewModel
    {
        public int ServiceRecordCount { get; set; }
        public decimal ServiceRecordCost { get; set; }
        public string MostCommonServiceRecord { get; set; } = string.Empty;
        public int MostCommonServiceRecordOccurrence { get; set; }
        public decimal MostCommonServiceRecordAverageCost { get; set; }
        public DateTime MostCommonServiceRecordLastOccurred { get; set; }
        public string MostExpensiveServiceRecord { get; set; } = string.Empty;
        public decimal MostExpensiveServiceRecordCost { get; set; }
        public DateTime MostExpensiveServiceRecordDate { get; set; }
        public int MostExpensiveServiceRecordOdometer { get; set;  }
        public int RepairRecordCount { get; set; }
        public decimal RepairRecordCost { get; set; }
        public string MostCommonRepairRecord { get; set; } = string.Empty;
        public int MostCommonRepairRecordOccurrence { get; set; }
        public decimal MostCommonRepairRecordAverageCost { get; set; }
        public DateTime MostCommonRepairRecordLastOccurred { get; set; }
        public string MostExpensiveRepairRecord { get; set; } = string.Empty;
        public decimal MostExpensiveRepairRecordCost { get; set; }
        public DateTime MostExpensiveRepairRecordDate { get; set; }
        public int MostExpensiveRepairRecordOdometer { get; set; }
        public int UpgradeRecordCount { get; set; }
        public decimal UpgradeRecordCost { get; set; }
        public string MostCommonUpgradeRecord { get; set; } = string.Empty;
        public int MostCommonUpgradeRecordOccurrence { get; set; }
        public decimal MostCommonUpgradeRecordAverageCost { get; set; }
        public DateTime MostCommonUpgradeRecordLastOccurred { get; set; }
        public string MostExpensiveUpgradeRecord { get; set; } = string.Empty;
        public decimal MostExpensiveUpgradeRecordCost { get; set; }
        public DateTime MostExpensiveUpgradeRecordDate { get; set; }
        public int MostExpensiveUpgradeRecordOdometer { get; set; }
    }
}

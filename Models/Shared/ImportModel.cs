namespace CarCareTracker.Models
{
    /// <summary>
    /// Import model used for importing records via CSV.
    /// </summary>
    public class ImportModel
    {
        public string Date { get; set; }
        public string Day { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public string DateCreated { get; set; }
        public string DateModified { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; }
        public string Progress { get; set; }
        public string InitialOdometer { get; set; }
        public string Odometer { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string FuelConsumed { get; set; }
        public string Cost { get; set; }
        public string Price { get; set; }
        public string PartialFuelUp { get; set; }
        public string IsFillToFull { get; set; }
        public string MissedFuelUp { get; set; }
        public string PartNumber { get; set; }
        public string PartSupplier { get; set; }
        public string PartQuantity { get; set; }
        public string Tags { get; set; }
        public Dictionary<string,string> ExtraFields {get;set;}
    }

    public class SupplyRecordExportModel
    {
        public string Date { get; set; }
        public string PartNumber { get; set; }
        public string PartSupplier { get; set; }
        public string PartQuantity { get; set; }
        public string Description { get; set; }
        public string Cost { get; set; }
        public string Notes { get; set; }
        public string Tags { get; set; }
        public List<ExtraField> ExtraFields { get; set; }
    }
    public class GenericRecordExportModel
    {
        public string Id { get; set; }
        public string Date { get; set; }
        public string Odometer { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string Cost { get; set; }
        public string Tags { get; set; }
        public List<ExtraField> ExtraFields { get; set; }
    }
    public class OdometerRecordExportModel
    {
        public string Id { get; set; }
        public string Date { get; set; }
        public string InitialOdometer { get; set; }
        public string Odometer { get; set; }
        public string Notes { get; set; }
        public string Tags { get; set; }
        public List<ExtraField> ExtraFields { get; set; }
    }
    public class TaxRecordExportModel
    {
        public string Id { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string Cost { get; set; }
        public string Tags { get; set; }
        public List<ExtraField> ExtraFields { get; set; }
    }
    public class GasRecordExportModel
    {
        public string Id { get; set; }
        public string Date { get; set; }
        public string Odometer { get; set; }
        public string FuelConsumed { get; set; }
        public string Cost { get; set; }
        public string FuelEconomy { get; set; }
        public string IsFillToFull { get; set; }
        public string MissedFuelUp { get; set; }
        public string Notes { get; set; }
        public string Tags { get; set; }
        public List<ExtraField> ExtraFields { get; set; }
    }
    public class ReminderExportModel
    {
        public string Description { get; set; }
        public string Urgency { get; set; }
        public string Metric { get; set; }
        public string Notes { get; set; }
        public string DueDate { get; set; }
        public string DueOdometer { get; set; }
    }
    public class PlanRecordExportModel 
    {
        public string DateCreated { get; set; }
        public string DateModified { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; }
        public string Progress { get; set; }
        public string Cost { get; set; }
        public List<ExtraField> ExtraFields { get; set; }
    }
}

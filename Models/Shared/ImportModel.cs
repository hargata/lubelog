using System.Text.Json.Serialization;

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
        public string IsEquipped { get; set; }
        public Dictionary<string,string> ExtraFields {get;set;}
    }

    public class SupplyRecordExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; }
        [JsonConverter(typeof(FromDateOptional))]
        public string Date { get; set; }
        public string PartNumber { get; set; }
        public string PartSupplier { get; set; }
        [JsonConverter(typeof(FromDecimalOptional))]
        public string PartQuantity { get; set; }
        public string Description { get; set; }
        [JsonConverter(typeof(FromDecimalOptional))]
        public string Cost { get; set; }
        public string Notes { get; set; }
        public string Tags { get; set; }
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
    }
    public class GenericRecordExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; }
        [JsonConverter(typeof(FromDateOptional))]
        public string Date { get; set; }
        [JsonConverter(typeof(FromIntOptional))]
        public string Odometer { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        [JsonConverter(typeof(FromDecimalOptional))]
        public string Cost { get; set; }
        public string Tags { get; set; }
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
    }
    public class OdometerRecordExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; }
        [JsonConverter(typeof(FromDateOptional))]
        public string Date { get; set; }
        [JsonConverter(typeof(FromIntOptional))]
        public string InitialOdometer { get; set; }
        [JsonConverter(typeof(FromIntOptional))]
        public string Odometer { get; set; }
        public string Notes { get; set; }
        public string Tags { get; set; }
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public string EquipmentRecordId { get; set; }
        [JsonConverter(typeof(FromBoolOptional))]
        public string AutoIncludeEquipment { get; set; }
    }
    public class TaxRecordExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; }
        [JsonConverter(typeof(FromDateOptional))]
        public string Date { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        [JsonConverter(typeof(FromDecimalOptional))]
        public string Cost { get; set; }
        public string Tags { get; set; }
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
    }
    public class GasRecordExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; }
        [JsonConverter(typeof(FromDateOptional))]
        public string Date { get; set; }
        [JsonConverter(typeof(FromIntOptional))]
        public string Odometer { get; set; }
        [JsonConverter(typeof(FromDecimalOptional))]
        public string FuelConsumed { get; set; }
        [JsonConverter(typeof(FromDecimalOptional))]
        public string Cost { get; set; }
        [JsonConverter(typeof(FromDecimalOptional))]
        public string FuelEconomy { get; set; }
        [JsonConverter(typeof(FromBoolOptional))]
        public string IsFillToFull { get; set; }
        [JsonConverter(typeof(FromBoolOptional))]
        public string MissedFuelUp { get; set; }
        public string Notes { get; set; }
        public string Tags { get; set; }
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
    }
    public class EquipmentRecordExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; }
        public string Description { get; set; }
        [JsonConverter(typeof(FromBoolOptional))]
        public string IsEquipped { get; set; }
        public string Notes { get; set; }
        public string Tags { get; set; }
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
    }
    /// <summary>
    /// Only used for the API GET Method
    /// </summary>
    public class EquipmentRecordAPIExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; }
        public string Description { get; set; }
        [JsonConverter(typeof(FromBoolOptional))]
        public string IsEquipped { get; set; }
        [JsonConverter(typeof(FromIntOptional))]
        public string DistanceTraveled { get; set; }
        public string Notes { get; set; }
        public string Tags { get; set; }
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
    }
    public class ReminderExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; }
        public string Description { get; set; }
        public string Urgency { get; set; }
        public string Metric { get; set; }
        public string Notes { get; set; }
        [JsonConverter(typeof(FromDateOptional))]
        public string DueDate { get; set; }
        [JsonConverter(typeof(FromIntOptional))]
        public string DueOdometer { get; set; }
        public string Tags { get; set; }
    }
    /// <summary>
    /// Only used for the API GET Method
    /// </summary>
    public class ReminderAPIExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; }
        public string Description { get; set; }
        public string Urgency { get; set; }
        public string Metric { get; set; }
        public string UserMetric { get; set; }
        public string Notes { get; set; }
        [JsonConverter(typeof(FromDateOptional))]
        public string DueDate { get; set; }
        [JsonConverter(typeof(FromIntOptional))]
        public string DueOdometer { get; set; }
        [JsonConverter(typeof(FromIntOptional))]
        public string DueDays { get; set; }
        [JsonConverter(typeof(FromIntOptional))]
        public string DueDistance { get; set; }
        public string Tags { get; set; }
    }
    public class PlanRecordExportModel 
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; }
        [JsonConverter(typeof(FromDateOptional))]
        public string DateCreated { get; set; }
        [JsonConverter(typeof(FromDateOptional))]
        public string DateModified { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; }
        public string Progress { get; set; }
        [JsonConverter(typeof(FromDecimalOptional))]
        public string Cost { get; set; }
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
    }
    public class UserExportModel
    {
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        [JsonConverter(typeof(FromBoolOptional))]
        public string IsAdmin { get; set; }
        [JsonConverter(typeof(FromBoolOptional))]
        public string IsRoot { get; set; }
    }
    public class AttachmentExportModel
    {
        public string DataType { get; set; }
        public string Date { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
    }
    public class RecordExtraFieldExportModel
    {
        public string RecordType { get; set; }
        public List<ExtraFieldExportModel> ExtraFields { get; set; }
    }
    public class ExtraFieldExportModel
    {
        public string Name { get; set; }
        [JsonConverter(typeof(FromBoolOptional))]
        public string IsRequired { get; set; }
        public string FieldType { get; set; }
    }
    public class VehicleImportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; }
        [JsonConverter(typeof(FromIntOptional))]
        public string Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string LicensePlate { get; set; }
        public string Identifier { get; set; } = "LicensePlate";
        public string FuelType { get; set; } = "Gasoline";
        [JsonConverter(typeof(FromBoolOptional))]
        public string UseEngineHours { get; set; }
        [JsonConverter(typeof(FromBoolOptional))]
        public string OdometerOptional { get; set; }
        public string Tags { get; set; }
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
    }
}
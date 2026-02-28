using System.Text.Json.Serialization;

namespace CarCareTracker.Models
{
    /// <summary>
    /// Import model used for importing records via CSV.
    /// </summary>
    public class ImportModel
    {
        public string Date { get; set; } = string.Empty;
        public string Day { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string DateCreated { get; set; } = string.Empty;
        public string DateModified { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Progress { get; set; } = string.Empty;
        public string InitialOdometer { get; set; } = string.Empty;
        public string Odometer { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string FuelConsumed { get; set; } = string.Empty;
        public string Cost { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public string PartialFuelUp { get; set; } = string.Empty;
        public string IsFillToFull { get; set; } = string.Empty;
        public string SoC { get; set; } = string.Empty;
        public string MissedFuelUp { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string PartSupplier { get; set; } = string.Empty;
        public string PartQuantity { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string IsEquipped { get; set; } = string.Empty;
        public Dictionary<string,string> ExtraFields {get;set; } = new Dictionary<string, string>();
    }

    public class SupplyRecordExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string VehicleId { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; } = string.Empty;
        [JsonConverter(typeof(FromDateOptional))]
        public string Date { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string PartSupplier { get; set; } = string.Empty;
        [JsonConverter(typeof(FromDecimalOptional))]
        public string PartQuantity { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [JsonConverter(typeof(FromDecimalOptional))]
        public string Cost { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
    }
    public class GenericRecordExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string VehicleId { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; } = string.Empty;
        [JsonConverter(typeof(FromDateOptional))]
        public string Date { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string Odometer { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        [JsonConverter(typeof(FromDecimalOptional))]
        public string Cost { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
    }
    public class OdometerRecordExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string VehicleId { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; } = string.Empty;
        [JsonConverter(typeof(FromDateOptional))]
        public string Date { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string InitialOdometer { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string Odometer { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
        public string EquipmentRecordId { get; set; } = string.Empty;
    }
    public class TaxRecordExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string VehicleId { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; } = string.Empty;
        [JsonConverter(typeof(FromDateOptional))]
        public string Date { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        [JsonConverter(typeof(FromDecimalOptional))]
        public string Cost { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
    }
    public class GasRecordExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string VehicleId { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; } = string.Empty;
        [JsonConverter(typeof(FromDateOptional))]
        public string Date { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string Odometer { get; set; } = string.Empty;
        [JsonConverter(typeof(FromDecimalOptional))]
        public string FuelConsumed { get; set; } = string.Empty;
        [JsonConverter(typeof(FromDecimalOptional))]
        public string Cost { get; set; } = string.Empty;
        [JsonConverter(typeof(FromDecimalOptional))]
        public string FuelEconomy { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string SoC { get; set; } = string.Empty;
        [JsonConverter(typeof(FromBoolOptional))]
        public string IsFillToFull { get; set; } = string.Empty;
        [JsonConverter(typeof(FromBoolOptional))]
        public string MissedFuelUp { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
    }
    public class EquipmentRecordExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string VehicleId { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [JsonConverter(typeof(FromBoolOptional))]
        public string IsEquipped { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
    }
    /// <summary>
    /// Only used for the API GET Method
    /// </summary>
    public class EquipmentRecordAPIExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string VehicleId { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [JsonConverter(typeof(FromBoolOptional))]
        public string IsEquipped { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string DistanceTraveled { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
    }
     public class NoteRecordExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string VehicleId { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string NoteText { get; set; } = string.Empty;
        [JsonConverter(typeof(FromBoolOptional))]
        public string Pinned { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
    }
     public class ReminderExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string VehicleId { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Urgency { get; set; } = string.Empty;
        public string Metric { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        [JsonConverter(typeof(FromDateOptional))]
        public string DueDate { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string DueOdometer { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
    }
    /// <summary>
    /// Only used for the API GET Method
    /// </summary>
    public class ReminderAPIExportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string VehicleId { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Urgency { get; set; } = string.Empty;
        public string Metric { get; set; } = string.Empty;
        public string UserMetric { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        [JsonConverter(typeof(FromDateOptional))]
        public string DueDate { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string DueOdometer { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string DueDays { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string DueDistance { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
    }
    public class PlanRecordExportModel 
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string VehicleId { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; } = string.Empty;
        [JsonConverter(typeof(FromDateOptional))]
        public string DateCreated { get; set; } = string.Empty;
        [JsonConverter(typeof(FromDateOptional))]
        public string DateModified { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Progress { get; set; } = string.Empty;
        [JsonConverter(typeof(FromDecimalOptional))]
        public string Cost { get; set; } = string.Empty;
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
        public List<UploadedFiles> Files { get; set; } = new List<UploadedFiles>();
    }
    public class UserExportModel
    {
        public string Username { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        [JsonConverter(typeof(FromBoolOptional))]
        public string IsAdmin { get; set; } = string.Empty;
        [JsonConverter(typeof(FromBoolOptional))]
        public string IsRoot { get; set; } = string.Empty;
    }
    public class AttachmentExportModel
    {
        public string DataType { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
    public class RecordExtraFieldExportModel
    {
        public string RecordType { get; set; } = string.Empty;
        public List<ExtraFieldExportModel> ExtraFields { get; set; } = new List<ExtraFieldExportModel>();
    }
    public class ExtraFieldExportModel
    {
        public string Name { get; set; } = string.Empty;
        [JsonConverter(typeof(FromBoolOptional))]
        public string IsRequired { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;
    }
    public class VehicleImportModel
    {
        [JsonConverter(typeof(FromIntOptional))]
        public string Id { get; set; } = string.Empty;
        [JsonConverter(typeof(FromIntOptional))]
        public string Year { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string Identifier { get; set; } = "LicensePlate";
        public string FuelType { get; set; } = "Gasoline";
        [JsonConverter(typeof(FromBoolOptional))]
        public string UseEngineHours { get; set; } = string.Empty;
        [JsonConverter(typeof(FromBoolOptional))]
        public string OdometerOptional { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public List<ExtraField> ExtraFields { get; set; } = new List<ExtraField>();
    }
}
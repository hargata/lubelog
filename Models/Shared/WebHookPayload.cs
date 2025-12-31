using System.Text.Json.Serialization;

namespace CarCareTracker.Models
{
    /// <summary>
    /// WebHookPayload Object
    /// </summary>
    public class WebHookPayloadBase
    {
        public string Type { get; set; } = "";
        public string Timestamp
        {
            get { return DateTime.UtcNow.ToString("O"); }
        }
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
        /// <summary>
        /// Legacy attributes below
        /// </summary>
        public string VehicleId { get; set; } = "";
        public string Username { get; set; } = "";
        public string Action { get; set; } = "";
    }
    public class DiscordWebHook
    {
        public string Username { get { return "LubeLogger"; } }
        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get { return "https://hargata.github.io/hargata/lubelogger_logo_small.png"; } }
        public string Content { get; set; } = "";
        public static DiscordWebHook FromWebHookPayload(WebHookPayload webHookPayload)
        {
            return new DiscordWebHook
            {
                Content = webHookPayload.Action,
            };
        } 
    }
    public class WebHookPayload: WebHookPayloadBase
    {
        private static string GetFriendlyActionType(string actionType)
        {
            var actionTypeParts = actionType.Split('.');
            if (actionTypeParts.Length == 2)
            {
                var recordType = actionTypeParts[0];
                var recordAction = actionTypeParts[1];
                switch (recordAction)
                {
                    case "add":
                        recordAction = "Added";
                        break;
                    case "update":
                        recordAction = "Updated";
                        break;
                    case "delete":
                        recordAction = "Deleted";
                        break;
                }
                if (recordType.ToLower().Contains("record"))
                {
                    var cleanedRecordType = recordType.ToLower().Replace("record", "");
                    cleanedRecordType = $"{char.ToUpper(cleanedRecordType[0])}{cleanedRecordType.Substring(1)} Record";
                    recordType = cleanedRecordType;
                } else
                {
                    recordType = $"{char.ToUpper(recordType[0])}{recordType.Substring(1)}";
                }
                return $"{recordAction} {recordType}";
            } else if (actionTypeParts.Length == 3)
            {
                var recordType = actionTypeParts[0];
                var recordAction = actionTypeParts[1];
                var thirdPart = actionTypeParts[2];
                switch (recordAction)
                {
                    case "add":
                        recordAction = "Added";
                        break;
                    case "update":
                        recordAction = "Updated";
                        break;
                    case "delete":
                        recordAction = "Deleted";
                        break;
                }
                if (recordType.ToLower().Contains("record"))
                {
                    var cleanedRecordType = recordType.ToLower().Replace("record", "");
                    cleanedRecordType = $"{char.ToUpper(cleanedRecordType[0])}{cleanedRecordType.Substring(1)} Record";
                    recordType = cleanedRecordType;
                }
                else
                {
                    recordType = $"{char.ToUpper(recordType[0])}{recordType.Substring(1)}";
                }
                if (thirdPart == "api")
                {
                    return $"{recordAction} {recordType} via API";
                } else
                {
                    return $"{recordAction} {recordType}";
                }
            }
            return actionType;
        }
        public static WebHookPayload FromGenericRecord(GenericRecord genericRecord, string actionType, string userName)
        {
            Dictionary<string, string> payloadDictionary = new Dictionary<string, string>();
            payloadDictionary.Add("user", userName);
            payloadDictionary.Add("description", genericRecord.Description);
            payloadDictionary.Add("odometer", genericRecord.Mileage.ToString());
            payloadDictionary.Add("vehicleId", genericRecord.VehicleId.ToString());
            payloadDictionary.Add("cost", genericRecord.Cost.ToString("F2"));
            return new WebHookPayload
            {
                Type = actionType,
                Data = payloadDictionary,
                VehicleId = genericRecord.VehicleId.ToString(),
                Username = userName,
                Action = $"{userName} {GetFriendlyActionType(actionType)} Description: {genericRecord.Description}"
            };
        }
        public static WebHookPayload FromGasRecord(GasRecord gasRecord, string actionType, string userName)
        {
            Dictionary<string, string> payloadDictionary = new Dictionary<string, string>();
            payloadDictionary.Add("user", userName);
            payloadDictionary.Add("odometer", gasRecord.Mileage.ToString());
            payloadDictionary.Add("fuelconsumed", gasRecord.Gallons.ToString());
            payloadDictionary.Add("vehicleId", gasRecord.VehicleId.ToString());
            payloadDictionary.Add("cost", gasRecord.Cost.ToString("F2"));
            return new WebHookPayload
            {
                Type = actionType,
                Data = payloadDictionary,
                VehicleId = gasRecord.VehicleId.ToString(),
                Username = userName,
                Action = $"{userName} {GetFriendlyActionType(actionType)} Odometer: {gasRecord.Mileage}"
            };
        }
        public static WebHookPayload FromOdometerRecord(OdometerRecord odometerRecord, string actionType, string userName)
        {
            Dictionary<string, string> payloadDictionary = new Dictionary<string, string>();
            payloadDictionary.Add("user", userName);
            payloadDictionary.Add("initialodometer", odometerRecord.InitialMileage.ToString());
            payloadDictionary.Add("odometer", odometerRecord.Mileage.ToString());
            payloadDictionary.Add("vehicleId", odometerRecord.VehicleId.ToString());
            return new WebHookPayload
            {
                Type = actionType,
                Data = payloadDictionary,
                VehicleId = odometerRecord.VehicleId.ToString(),
                Username = userName,
                Action = $"{userName} {GetFriendlyActionType(actionType)} Odometer: {odometerRecord.Mileage}"
            };
        }
        public static WebHookPayload FromTaxRecord(TaxRecord taxRecord, string actionType, string userName)
        {
            Dictionary<string, string> payloadDictionary = new Dictionary<string, string>();
            payloadDictionary.Add("user", userName);
            payloadDictionary.Add("description", taxRecord.Description);
            payloadDictionary.Add("vehicleId", taxRecord.VehicleId.ToString());
            payloadDictionary.Add("cost", taxRecord.Cost.ToString("F2"));
            return new WebHookPayload
            {
                Type = actionType,
                Data = payloadDictionary,
                VehicleId = taxRecord.VehicleId.ToString(),
                Username = userName,
                Action = $"{userName} {GetFriendlyActionType(actionType)} Description: {taxRecord.Description}"
            };
        }
        public static WebHookPayload FromEquipmentRecord(EquipmentRecord equipmentRecord, string actionType, string userName)
        {
            Dictionary<string, string> payloadDictionary = new Dictionary<string, string>();
            payloadDictionary.Add("user", userName);
            payloadDictionary.Add("description", equipmentRecord.Description);
            payloadDictionary.Add("vehicleId", equipmentRecord.VehicleId.ToString());
            return new WebHookPayload
            {
                Type = actionType,
                Data = payloadDictionary,
                VehicleId = equipmentRecord.VehicleId.ToString(),
                Username = userName,
                Action = $"{userName} {GetFriendlyActionType(actionType)} Description: {equipmentRecord.Description}"
            };
        }
        public static WebHookPayload FromPlanRecord(PlanRecord planRecord, string actionType, string userName)
        {
            Dictionary<string, string> payloadDictionary = new Dictionary<string, string>();
            payloadDictionary.Add("user", userName);
            payloadDictionary.Add("description", planRecord.Description);
            payloadDictionary.Add("vehicleId", planRecord.VehicleId.ToString());
            payloadDictionary.Add("cost", planRecord.Cost.ToString("F2"));
            return new WebHookPayload
            {
                Type = actionType,
                Data = payloadDictionary,
                VehicleId = planRecord.VehicleId.ToString(),
                Username = userName,
                Action = $"{userName} {GetFriendlyActionType(actionType)} Description: {planRecord.Description}"
            };
        }
        public static WebHookPayload FromInspectionRecord(InspectionRecord inspectionRecord, string actionType, string userName)
        {
            Dictionary<string, string> payloadDictionary = new Dictionary<string, string>();
            payloadDictionary.Add("user", userName);
            payloadDictionary.Add("description", inspectionRecord.Description);
            payloadDictionary.Add("vehicleId", inspectionRecord.VehicleId.ToString());
            payloadDictionary.Add("cost", inspectionRecord.Cost.ToString("F2"));
            return new WebHookPayload
            {
                Type = actionType,
                Data = payloadDictionary,
                VehicleId = inspectionRecord.VehicleId.ToString(),
                Username = userName,
                Action = $"{userName} {GetFriendlyActionType(actionType)} Description: {inspectionRecord.Description}"
            };
        }
        public static WebHookPayload FromReminderRecord(ReminderRecord reminderRecord, string actionType, string userName)
        {
            Dictionary<string, string> payloadDictionary = new Dictionary<string, string>();
            payloadDictionary.Add("user", userName);
            payloadDictionary.Add("description", reminderRecord.Description);
            payloadDictionary.Add("vehicleId", reminderRecord.VehicleId.ToString());
            payloadDictionary.Add("metric", reminderRecord.Metric.ToString());
            return new WebHookPayload
            {
                Type = actionType,
                Data = payloadDictionary,
                VehicleId = reminderRecord.VehicleId.ToString(),
                Username = userName,
                Action = $"{userName} {GetFriendlyActionType(actionType)} Description: {reminderRecord.Description}"
            };
        }
        public static WebHookPayload FromSupplyRecord(SupplyRecord supplyRecord, string actionType, string userName)
        {
            Dictionary<string, string> payloadDictionary = new Dictionary<string, string>();
            payloadDictionary.Add("user", userName);
            payloadDictionary.Add("description", supplyRecord.Description);
            payloadDictionary.Add("vehicleId", supplyRecord.VehicleId.ToString());
            payloadDictionary.Add("cost", supplyRecord.Cost.ToString("F2"));
            payloadDictionary.Add("quantity", supplyRecord.Quantity.ToString("F2"));
            return new WebHookPayload
            {
                Type = actionType,
                Data = payloadDictionary,
                VehicleId = supplyRecord.VehicleId.ToString(),
                Username = userName,
                Action = $"{userName} {GetFriendlyActionType(actionType)} Description: {supplyRecord.Description}"
            };
        }
        public static WebHookPayload FromNoteRecord(Note noteRecord, string actionType, string userName)
        {
            Dictionary<string, string> payloadDictionary = new Dictionary<string, string>();
            payloadDictionary.Add("user", userName);
            payloadDictionary.Add("description", noteRecord.Description);
            payloadDictionary.Add("vehicleId", noteRecord.VehicleId.ToString());
            return new WebHookPayload
            {
                Type = actionType,
                Data = payloadDictionary,
                VehicleId = noteRecord.VehicleId.ToString(),
                Username = userName,
                Action = $"{userName} {GetFriendlyActionType(actionType)} Description: {noteRecord.Description}"
            };
        }
        public static WebHookPayload Generic(string payload, string actionType, string userName, string vehicleId)
        {
            Dictionary<string, string> payloadDictionary = new Dictionary<string, string>();
            payloadDictionary.Add("user", userName);
            if (!string.IsNullOrWhiteSpace(payload))
            {
                payloadDictionary.Add("description", payload);
            }
            return new WebHookPayload
            {
                Type = actionType,
                Data = payloadDictionary,
                VehicleId = string.IsNullOrWhiteSpace(vehicleId) ? "N/A" : vehicleId,
                Username = userName,
                Action = string.IsNullOrWhiteSpace(payload) ? $"{userName} {GetFriendlyActionType(actionType)}" : $"{userName} {payload}"
            };
        }
    }
}

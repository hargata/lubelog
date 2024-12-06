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
    public class WebHookPayload: WebHookPayloadBase
    {
        private static string GetFriendlyActionType(string actionType)
        {
            var actionTypeParts = actionType.Split('.');
            if (actionTypeParts.Length == 2)
            {
                var recordType = actionTypeParts[0];
                var recordAction = actionTypeParts[1];
                if (recordAction == "add")
                {
                    recordAction = "ADDED";
                } else
                {
                    recordAction = $"{recordAction.ToUpper()}D";
                }
                return $"{recordAction} {recordType.ToUpper()}";
            } else if (actionTypeParts.Length == 3)
            {
                var recordType = actionTypeParts[0];
                var recordAction = actionTypeParts[1];
                var thirdPart = actionTypeParts[2];
                if (recordAction == "delete")
                {
                    recordAction = "DELETED";
                }
                else
                {
                    recordAction = $"{recordAction.ToUpper()}ED";
                }
                if (thirdPart == "api")
                {
                    return $"{recordAction} {recordType.ToUpper()} via API";
                } else
                {
                    return $"{recordAction} {recordType.ToUpper()}";
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
                Action = string.IsNullOrWhiteSpace(payload) ? GetFriendlyActionType(actionType) : payload
            };
        }
    }
}

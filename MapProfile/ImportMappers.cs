using CarCareTracker.Models;
using CsvHelper.Configuration;

namespace CarCareTracker.MapProfile
{
    public class ImportMapper: ClassMap<ImportModel>
    {
        public ImportMapper()
        {
            Map(m => m.Date).Name(["date", "fuelup_date"]);
            Map(m => m.Day).Name(["day"]);
            Map(m => m.Month).Name(["month"]);
            Map(m => m.Year).Name(["year"]);
            Map(m => m.DateCreated).Name(["datecreated"]);
            Map(m => m.DateModified).Name(["datemodified"]);
            Map(m => m.InitialOdometer).Name(["initialodometer"]);
            Map(m => m.Odometer).Name(["odometer", "odo"]);
            Map(m => m.FuelConsumed).Name(["gallons", "liters", "litres", "consumption", "quantity", "fuelconsumed", "qty"]);
            Map(m => m.Cost).Name(["cost", "total cost", "totalcost", "total price"]);
            Map(m => m.Notes).Name("notes", "note");
            Map(m => m.Price).Name(["price"]);
            Map(m => m.PartialFuelUp).Name(["partial_fuelup", "partial tank", "partial_fill"]);
            Map(m => m.IsFillToFull).Name(["isfilltofull", "filled up"]);
            Map(m => m.SoC).Name(["soc", "state_of_charge", "charge_level", "chargelevel"]);
            Map(m => m.Description).Name(["description"]);
            Map(m => m.MissedFuelUp).Name(["missed_fuelup", "missedfuelup", "missed fill up", "missed_fill"]);
            Map(m => m.PartSupplier).Name(["partsupplier"]);
            Map(m => m.PartQuantity).Name(["partquantity"]);
            Map(m => m.PartNumber).Name(["partnumber"]);
            Map(m => m.Progress).Name(["progress"]);
            Map(m => m.Type).Name(["type"]);
            Map(m => m.Priority).Name(["priority"]);
            Map(m => m.Tags).Name(["tags"]);
            Map(m => m.IsEquipped).Name(["isequipped"]);
            Map(m => m.ExtraFields).Convert(row =>
            {
                var attributes = new Dictionary<string, string>();
                foreach (var header in row.Row.HeaderRecord)
                {
                    if (header.ToLower().StartsWith("extrafield_"))
                    {
                        attributes.Add(header.Substring(11), row.Row.GetField(header));
                    }
                }
                return attributes;
            });
        }
    }
}

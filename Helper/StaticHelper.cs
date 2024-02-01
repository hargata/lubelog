using CarCareTracker.Models;
using System.Globalization;

namespace CarCareTracker.Helper
{
    /// <summary>
    /// helper method for static vars
    /// </summary>
    public static class StaticHelper
    {
        public static string DbName = "data/cartracker.db";
        public static string UserConfigPath = "config/userConfig.json";
        public static string GenericErrorMessage = "An error occurred, please try again later";

        public static string TruncateStrings(string input, int maxLength = 25)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }
            if (input.Length > maxLength)
            {
                return (input.Substring(0, maxLength) + "...");
            }
            else
            {
                return input;
            }
        }
        public static string DefaultActiveTab(UserConfig userConfig, ImportMode tab)
        {
            var defaultTab = userConfig.DefaultTab;
            var visibleTabs = userConfig.VisibleTabs;
            if (visibleTabs.Contains(tab) && tab == defaultTab)
            {
                return "active";
            }
            else if (!visibleTabs.Contains(tab))
            {
                return "d-none";
            }
            return "";
        }
        public static string DefaultActiveTabContent(UserConfig userConfig, ImportMode tab)
        {
            var defaultTab = userConfig.DefaultTab;
            if (tab == defaultTab)
            {
                return "show active";
            }
            return "";
        }
        public static string DefaultTabSelected(UserConfig userConfig, ImportMode tab)
        {
            var defaultTab = userConfig.DefaultTab;
            var visibleTabs = userConfig.VisibleTabs;
            if (!visibleTabs.Contains(tab))
            {
                return "disabled";
            }
            else if (tab == defaultTab)
            {
                return "selected";
            }
            return "";
        }
        public static List<CostForVehicleByMonth> GetBaseLineCosts()
        {
            return new List<CostForVehicleByMonth>()
            {
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(1), MonthId = 1, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(2), MonthId = 2, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(3), MonthId = 3, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(4), MonthId = 4, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(5), MonthId = 5, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(6), MonthId = 6, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(7), MonthId = 7, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(8), MonthId = 8, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(9), MonthId = 9, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(10), MonthId = 10, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(11), MonthId = 11, Cost = 0M},
                new CostForVehicleByMonth {MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(12), MonthId = 12, Cost = 0M}
            };
        }
        public static List<CostForVehicleByMonth> GetBaseLineCostsNoMonthName()
        {
            return new List<CostForVehicleByMonth>()
            {
                new CostForVehicleByMonth { MonthId = 1, Cost = 0M},
                new CostForVehicleByMonth {MonthId = 2, Cost = 0M},
                new CostForVehicleByMonth {MonthId = 3, Cost = 0M},
                new CostForVehicleByMonth {MonthId = 4, Cost = 0M},
                new CostForVehicleByMonth {MonthId = 5, Cost = 0M},
                new CostForVehicleByMonth {MonthId = 6, Cost = 0M},
                new CostForVehicleByMonth {MonthId = 7, Cost = 0M},
                new CostForVehicleByMonth {MonthId = 8, Cost = 0M},
                new CostForVehicleByMonth {MonthId = 9, Cost = 0M},
                new CostForVehicleByMonth { MonthId = 10, Cost = 0M},
                new CostForVehicleByMonth { MonthId = 11, Cost = 0M},
                new CostForVehicleByMonth { MonthId = 12, Cost = 0M}
            };
        }

        public static ServiceRecord GenericToServiceRecord(GenericRecord input)
        {
            return new ServiceRecord
            {
                VehicleId = input.VehicleId,
                Date = input.Date,
                Description = input.Description,
                Cost = input.Cost,
                Mileage = input.Mileage,
                Files = input.Files,
                Notes = input.Notes
            };
        }
        public static CollisionRecord GenericToRepairRecord(GenericRecord input)
        {
            return new CollisionRecord
            {
                VehicleId = input.VehicleId,
                Date = input.Date,
                Description = input.Description,
                Cost = input.Cost,
                Mileage = input.Mileage,
                Files = input.Files,
                Notes = input.Notes
            };
        }
        public static UpgradeRecord GenericToUpgradeRecord(GenericRecord input)
        {
            return new UpgradeRecord
            {
                VehicleId = input.VehicleId,
                Date = input.Date,
                Description = input.Description,
                Cost = input.Cost,
                Mileage = input.Mileage,
                Files = input.Files,
                Notes = input.Notes
            };
        }

        public static string GetFuelEconomyUnit(bool useKwh, bool useHours, bool useMPG, bool useUKMPG)
        {
            string fuelEconomyUnit;
            if (useKwh)
            {
                var distanceUnit = useHours ? "h" : (useMPG ? "mi." : "km");
                fuelEconomyUnit = useMPG ? $"{distanceUnit}/kWh" : $"kWh/100{distanceUnit}";
            }
            else if (useMPG && useUKMPG)
            {
                fuelEconomyUnit = useHours ? "h/g" : "mpg";
            }
            else if (useUKMPG)
            {
                fuelEconomyUnit = useHours ? "l/100h" : "l/100mi.";
            }
            else
            {
                fuelEconomyUnit = useHours ? (useMPG ? "h/g" : "l/100h") : (useMPG ? "mpg" : "l/100km");
            }
            return fuelEconomyUnit;
        }
    }
}

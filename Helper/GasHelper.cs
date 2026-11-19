using CarCareTracker.Models;

namespace CarCareTracker.Helper
{
    public interface IGasHelper
    {
        List<GasRecordViewModel> GetGasRecordViewModels(List<GasRecord> result, bool useMPG, bool useUKMPG, bool isElectric);
        List<GasRecordViewModel> GetAdditionalFuelViewModels(List<GasRecordViewModel> results, AdditionalFuelType fuelType);
        string GetAverageGasMileage(List<GasRecordViewModel> results, bool useMPG);
    }
    public class GasHelper : IGasHelper
    {
        public string GetAverageGasMileage(List<GasRecordViewModel> results, bool useMPG)
        {
            var recordsToCalculate = results.Where(x => x.IncludeInAverage);
            if (recordsToCalculate.Any())
            {
                try
                {
                    var totalMileage = recordsToCalculate.Sum(x => x.DeltaMileage);
                    var totalGallons = recordsToCalculate.Sum(x => x.Gallons);
                    var averageGasMileage = totalMileage / totalGallons;
                    if (!useMPG && averageGasMileage > 0)
                    {
                        averageGasMileage = 100 / averageGasMileage;
                    }
                    return averageGasMileage.ToString("F");
                } catch
                {
                    return "0";
                }
            }
            return "0";
        }
        public List<GasRecordViewModel> GetGasRecordViewModels(List<GasRecord> result, bool useMPG, bool useUKMPG, bool isElectric)
        {
            //need to order by to get correct results
            result = result.OrderBy(x => x.Date).ThenBy(x => x.Mileage).ThenBy(x=>x.EndingSoc).ToList();
            //records that only contain additional fuels(e.g.: DEF top-ups) are excluded from the primary fuel economy calculation.
            var primaryFuelResults = CalculateFuelEconomy(result.Where(x => !IsAdditionalFuelOnly(x)).ToList(), useMPG, useUKMPG, isElectric);
            var computedResults = new List<GasRecordViewModel>();
            int primaryFuelIndex = 0;
            foreach (GasRecord gasRecord in result)
            {
                if (IsAdditionalFuelOnly(gasRecord))
                {
                    computedResults.Add(new GasRecordViewModel()
                    {
                        Id = gasRecord.Id,
                        VehicleId = gasRecord.VehicleId,
                        MonthId = gasRecord.Date.Month,
                        Date = gasRecord.Date.ToShortDateString(),
                        Mileage = gasRecord.Mileage,
                        Gallons = gasRecord.Gallons,
                        Cost = gasRecord.Cost,
                        DeltaMileage = 0,
                        MilesPerGallon = 0,
                        CostPerGallon = 0,
                        StartingSoc = gasRecord.StartingSoc,
                        EndingSoc = gasRecord.EndingSoc,
                        IsFillToFull = gasRecord.IsFillToFull,
                        MissedFuelUp = gasRecord.MissedFuelUp,
                        Notes = gasRecord.Notes,
                        Tags = gasRecord.Tags,
                        ExtraFields = gasRecord.ExtraFields,
                        Files = gasRecord.Files
                    });
                }
                else
                {
                    computedResults.Add(primaryFuelResults[primaryFuelIndex]);
                    primaryFuelIndex++;
                }
            }
            //each additional fuel type is calculated independently using the distance between its own refuels.
            var additionalFuelTypes = result.SelectMany(x => x.AdditionalFuels).Where(x => x.FuelConsumed > 0).Select(x => x.FuelType).Distinct().OrderBy(x => x);
            foreach (AdditionalFuelType fuelType in additionalFuelTypes)
            {
                var recordIndexes = new List<int>();
                var fuelRecords = new List<GasRecord>();
                for (int i = 0; i < result.Count; i++)
                {
                    var additionalFuels = result[i].AdditionalFuels.Where(x => x.FuelType == fuelType && x.FuelConsumed > 0);
                    if (additionalFuels.Any())
                    {
                        recordIndexes.Add(i);
                        fuelRecords.Add(new GasRecord
                        {
                            Id = result[i].Id,
                            VehicleId = result[i].VehicleId,
                            Date = result[i].Date,
                            Mileage = result[i].Mileage,
                            Gallons = additionalFuels.Sum(x => x.FuelConsumed),
                            Cost = additionalFuels.Sum(x => x.Cost),
                            IsFillToFull = additionalFuels.All(x => x.IsFillToFull)
                        });
                    }
                }
                var fuelResults = CalculateFuelEconomy(fuelRecords, useMPG, useUKMPG, false);
                for (int i = 0; i < fuelResults.Count; i++)
                {
                    var fuelResult = fuelResults[i];
                    computedResults[recordIndexes[i]].AdditionalFuels.Add(new AdditionalFuelViewModel
                    {
                        FuelType = fuelType,
                        FuelConsumed = fuelResult.Gallons,
                        Cost = fuelResult.Cost,
                        CostPerUnit = fuelResult.CostPerGallon,
                        IsFillToFull = fuelResult.IsFillToFull,
                        DeltaMileage = fuelResult.DeltaMileage,
                        FuelEconomy = fuelResult.MilesPerGallon,
                        IncludeInAverage = fuelResult.IncludeInAverage
                    });
                }
            }
            return computedResults;
        }
        public List<GasRecordViewModel> GetAdditionalFuelViewModels(List<GasRecordViewModel> results, AdditionalFuelType fuelType)
        {
            //flattens an additional fuel type into view models so that it can be aggregated the same way as the primary fuel.
            return results.Where(x => x.AdditionalFuels.Any(y => y.FuelType == fuelType)).Select(x =>
            {
                var additionalFuel = x.AdditionalFuels.First(y => y.FuelType == fuelType);
                return new GasRecordViewModel
                {
                    Id = x.Id,
                    VehicleId = x.VehicleId,
                    MonthId = x.MonthId,
                    Date = x.Date,
                    Mileage = x.Mileage,
                    Gallons = additionalFuel.FuelConsumed,
                    Cost = additionalFuel.Cost,
                    DeltaMileage = additionalFuel.DeltaMileage,
                    MilesPerGallon = additionalFuel.FuelEconomy,
                    CostPerGallon = additionalFuel.CostPerUnit,
                    IsFillToFull = additionalFuel.IsFillToFull,
                    Notes = x.Notes,
                    Tags = x.Tags,
                    ExtraFields = x.ExtraFields,
                    Files = x.Files
                };
            }).ToList();
        }
        private static bool IsAdditionalFuelOnly(GasRecord gasRecord)
        {
            return gasRecord.Gallons == default && gasRecord.AdditionalFuels.Any(x => x.FuelConsumed > 0);
        }
        private List<GasRecordViewModel> CalculateFuelEconomy(List<GasRecord> result, bool useMPG, bool useUKMPG, bool isElectric)
        {
            var computedResults = new List<GasRecordViewModel>();
            int previousMileage = 0;
            int previousEndingSoc = 0;
            decimal unFactoredConsumption = 0.00M;
            int unFactoredMileage = 0;
            //perform computation.
            for (int i = 0; i < result.Count; i++)
            {
                var currentObject = result[i];
                if (isElectric)
                {
                    if (i > 0)
                    {
                        decimal convertedConsumption;
                        decimal convertedStartSoc = currentObject.StartingSoc / 100M;
                        decimal convertedEndSoc = currentObject.EndingSoc / 100M;
                        decimal convertedPreviousEndingSoc = previousEndingSoc / 100M;
                        try
                        {
                            decimal currentBatteryCapacity = currentObject.Gallons / (convertedEndSoc - convertedStartSoc);
                            convertedConsumption = convertedPreviousEndingSoc > 0.00M ? (convertedPreviousEndingSoc - convertedStartSoc) * currentBatteryCapacity : 0.00M;
                            if (convertedConsumption < 0.00M) //negative consumption values can be caused by start soc greater than previous ending soc
                            {
                                convertedConsumption = 0.00M;
                            }
                        }
                        catch
                        {
                            convertedConsumption = 0.00M;
                        }
                        var deltaMileage = currentObject.Mileage - previousMileage;
                        if (deltaMileage < 0)
                        {
                            deltaMileage = 0;
                        }
                        var gasRecordViewModel = new GasRecordViewModel()
                        {
                            Id = currentObject.Id,
                            VehicleId = currentObject.VehicleId,
                            MonthId = currentObject.Date.Month,
                            Date = currentObject.Date.ToShortDateString(),
                            Mileage = currentObject.Mileage,
                            Gallons = convertedConsumption,
                            Cost = currentObject.Cost,
                            DeltaMileage = deltaMileage,
                            CostPerGallon = currentObject.Gallons > 0.00M ? currentObject.Cost / currentObject.Gallons : 0,
                            StartingSoc = currentObject.StartingSoc,
                            EndingSoc = currentObject.EndingSoc,
                            IsFillToFull = currentObject.IsFillToFull,
                            MissedFuelUp = currentObject.MissedFuelUp,
                            Notes = currentObject.Notes,
                            Tags = currentObject.Tags,
                            ExtraFields = currentObject.ExtraFields,
                            Files = currentObject.Files
                        };
                        if (currentObject.MissedFuelUp)
                        {
                            //if they missed a fuel up, we skip MPG calculation.
                            gasRecordViewModel.MilesPerGallon = 0;
                        }
                        else if (currentObject.Mileage != default)
                        {
                            if (convertedConsumption > 0.00M && deltaMileage > 0)
                            {
                                try
                                {
                                    gasRecordViewModel.MilesPerGallon = useMPG ? (unFactoredMileage + deltaMileage) / (unFactoredConsumption + convertedConsumption) : 100 / ((unFactoredMileage + deltaMileage) / (unFactoredConsumption + convertedConsumption));
                                }
                                catch
                                {
                                    gasRecordViewModel.MilesPerGallon = 0;
                                }
                            }
                        }
                        computedResults.Add(gasRecordViewModel);
                    } 
                    else
                    {
                        computedResults.Add(new GasRecordViewModel()
                        {
                            Id = currentObject.Id,
                            VehicleId = currentObject.VehicleId,
                            MonthId = currentObject.Date.Month,
                            Date = currentObject.Date.ToShortDateString(),
                            Mileage = currentObject.Mileage,
                            Gallons = currentObject.Gallons,
                            Cost = currentObject.Cost,
                            DeltaMileage = 0,
                            MilesPerGallon = 0,
                            CostPerGallon = currentObject.Gallons > 0.00M ? currentObject.Cost / currentObject.Gallons : 0,
                            StartingSoc = currentObject.StartingSoc,
                            EndingSoc = currentObject.EndingSoc,
                            IsFillToFull = currentObject.IsFillToFull,
                            MissedFuelUp = currentObject.MissedFuelUp,
                            Notes = currentObject.Notes,
                            Tags = currentObject.Tags,
                            ExtraFields = currentObject.ExtraFields,
                            Files = currentObject.Files
                        });
                    }
                    if (currentObject.Mileage != default)
                    {
                        previousMileage = currentObject.Mileage;
                    }
                    if (currentObject.EndingSoc != default)
                    {
                        previousEndingSoc = currentObject.EndingSoc;
                    }
                } 
                else
                {
                    decimal convertedConsumption;
                    if (useUKMPG && useMPG)
                    {
                        //if we're using UK MPG and the user wants imperial calculation insteace of l/100km
                        //if UK MPG is selected then the gas consumption are stored in liters but need to convert into UK gallons for computation.
                        convertedConsumption = currentObject.Gallons / 4.546M;
                    }
                    else
                    {
                        convertedConsumption = currentObject.Gallons;
                    }
                    if (i > 0)
                    {
                        var deltaMileage = currentObject.Mileage - previousMileage;
                        if (deltaMileage < 0)
                        {
                            deltaMileage = 0;
                        }
                        var gasRecordViewModel = new GasRecordViewModel()
                        {
                            Id = currentObject.Id,
                            VehicleId = currentObject.VehicleId,
                            MonthId = currentObject.Date.Month,
                            Date = currentObject.Date.ToShortDateString(),
                            Mileage = currentObject.Mileage,
                            Gallons = convertedConsumption,
                            Cost = currentObject.Cost,
                            DeltaMileage = deltaMileage,
                            CostPerGallon = convertedConsumption > 0.00M ? currentObject.Cost / convertedConsumption : 0,
                            IsFillToFull = currentObject.IsFillToFull,
                            MissedFuelUp = currentObject.MissedFuelUp,
                            Notes = currentObject.Notes,
                            Tags = currentObject.Tags,
                            ExtraFields = currentObject.ExtraFields,
                            Files = currentObject.Files
                        };
                        if (currentObject.MissedFuelUp)
                        {
                            //if they missed a fuel up, we skip MPG calculation.
                            gasRecordViewModel.MilesPerGallon = 0;
                            //reset unFactored vars for missed fuel up because the numbers wont be reliable.
                            unFactoredConsumption = 0;
                            unFactoredMileage = 0;
                        }
                        else if (currentObject.IsFillToFull && currentObject.Mileage != default)
                        {
                            //if user filled to full and an odometer is provided, otherwise we will defer calculations
                            if (convertedConsumption > 0.00M && deltaMileage > 0)
                            {
                                try
                                {
                                    gasRecordViewModel.MilesPerGallon = useMPG ? (unFactoredMileage + deltaMileage) / (unFactoredConsumption + convertedConsumption) : 100 / ((unFactoredMileage + deltaMileage) / (unFactoredConsumption + convertedConsumption));
                                }
                                catch
                                {
                                    gasRecordViewModel.MilesPerGallon = 0;
                                }
                            }
                            //reset unFactored vars
                            unFactoredConsumption = 0;
                            unFactoredMileage = 0;
                        }
                        else
                        {
                            unFactoredConsumption += convertedConsumption;
                            unFactoredMileage += deltaMileage;
                            gasRecordViewModel.MilesPerGallon = 0;
                        }
                        computedResults.Add(gasRecordViewModel);
                    }
                    else
                    {
                        computedResults.Add(new GasRecordViewModel()
                        {
                            Id = currentObject.Id,
                            VehicleId = currentObject.VehicleId,
                            MonthId = currentObject.Date.Month,
                            Date = currentObject.Date.ToShortDateString(),
                            Mileage = currentObject.Mileage,
                            Gallons = convertedConsumption,
                            Cost = currentObject.Cost,
                            DeltaMileage = 0,
                            MilesPerGallon = 0,
                            CostPerGallon = convertedConsumption > 0.00M ? currentObject.Cost / convertedConsumption : 0,
                            IsFillToFull = currentObject.IsFillToFull,
                            MissedFuelUp = currentObject.MissedFuelUp,
                            Notes = currentObject.Notes,
                            Tags = currentObject.Tags,
                            ExtraFields = currentObject.ExtraFields,
                            Files = currentObject.Files
                        });
                    }
                    if (currentObject.Mileage != default)
                    {
                        previousMileage = currentObject.Mileage;
                    }
                }
            }
            return computedResults;
        }
    }
}

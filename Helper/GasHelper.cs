using CarCareTracker.Models;

namespace CarCareTracker.Helper
{
    public interface IGasHelper
    {
        List<GasRecordViewModel> GetGasRecordViewModels(List<GasRecord> result, bool useMPG, bool useUKMPG);
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
        public List<GasRecordViewModel> GetGasRecordViewModels(List<GasRecord> result, bool useMPG, bool useUKMPG)
        {
            //need to order by to get correct results
            result = result.OrderBy(x => x.Date).ThenBy(x => x.Mileage).ToList();
            var computedResults = new List<GasRecordViewModel>();
            int previousMileage = 0;
            decimal unFactoredConsumption = 0.00M;
            int unFactoredMileage = 0;

            // For non-EV (SoC == 100 only), this behaves exactly like the original fill-to-full logic.
            //
            // For EV (mixed SoC values), each SoC level gets its own independent segment tracker.
            // A segment runs from one SoC=X entry to the next SoC=X entry, accumulating all energy
            // charged in between (regardless of intermediate SoC values).
            //
            // IMPORTANT: when a SoC=X segment closes (i.e. we matched two consecutive SoC=X entries),
            // all *other* SoC trackers are cleared. This ensures that segments from different SoC levels
            // don't overlap. For example:
            //   60, 80, 60 → the 60-segment closes; 80-tracker is cleared.
            //   100, 100   → clean 100-segment.
            //   80         → starts a fresh 80-tracker (old one was cleared after 60-segment closed).
            //
            // Key = SoC value; Value = (accumulated consumption, mileage at last SoC-X entry)
            var socTracker = new Dictionary<int, (decimal accumulatedConsumption, int lastSoCMileage)>();

            //perform computation.
            for (int i = 0; i < result.Count; i++)
            {
                var currentObject = result[i];
                decimal convertedConsumption;
                if (useUKMPG && useMPG)
                {
                    //if we're using UK MPG and the user wants imperial calculation instead of l/100km
                    //if UK MPG is selected then the gas consumption is stored in liters but needs to convert into UK gallons for computation.
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
                        SoC = currentObject.SoC,
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
                        //reset all tracking state because numbers won't be reliable.
                        unFactoredConsumption = 0;
                        unFactoredMileage = 0;
                        socTracker.Clear();
                    }
                    else if (currentObject.SoC > 0 && currentObject.Mileage != default)
                    {
                        if (!socTracker.TryGetValue(currentObject.SoC, out var prevSoCData))
                        {
                            // First time we see this SoC level — open a new segment.
                            // Store mileage as the segment start. Consumption here is NOT included
                            // in the accumulator — it brought the battery to this SoC level, just
                            // like a fill-to-full entry starts the clock without counting its own kWh.
                            socTracker[currentObject.SoC] = (0, currentObject.Mileage);
                            gasRecordViewModel.MilesPerGallon = 0;
                        }
                        else
                        {
                            // We have a prior entry at this same SoC level — close the segment and calculate.
                            var distanceFromLastSoC = currentObject.Mileage - prevSoCData.lastSoCMileage;
                            // Total consumption = everything charged since the opening entry + current entry.
                            var totalConsumption = prevSoCData.accumulatedConsumption + convertedConsumption;

                            if (convertedConsumption > 0.00M && distanceFromLastSoC > 0 && totalConsumption > 0)
                            {
                                try
                                {
                                    gasRecordViewModel.MilesPerGallon = useMPG
                                        ? (decimal)distanceFromLastSoC / totalConsumption
                                        : 100 / ((decimal)distanceFromLastSoC / totalConsumption);
                                }
                                catch
                                {
                                    gasRecordViewModel.MilesPerGallon = 0;
                                }
                            }

                            // Segment closed: clear ALL other SoC trackers so they don't span across
                            // this segment boundary (e.g. 60,80,60 closes → 80-tracker is stale).
                            socTracker.Clear();
                            // Start a fresh segment from current entry (consumption = 0, same reason as above).
                            socTracker[currentObject.SoC] = (0, currentObject.Mileage);
                        }
                        // Reset untracked (SoC=0) accumulators — they've been folded in above.
                        unFactoredConsumption = 0;
                        unFactoredMileage = 0;
                    }
                    else
                    {
                        // SoC = 0: "not tracked" / legacy IsFillToFull=false.
                        // Accumulate into untracked pool; also add to all open SoC segment trackers
                        // so their accumulated consumption stays up to date.
                        unFactoredConsumption += convertedConsumption;
                        unFactoredMileage += deltaMileage;
                        foreach (var key in socTracker.Keys.ToList())
                        {
                            var t = socTracker[key];
                            socTracker[key] = (t.accumulatedConsumption + convertedConsumption, t.lastSoCMileage);
                        }
                        gasRecordViewModel.MilesPerGallon = 0;
                    }
                    computedResults.Add(gasRecordViewModel);
                }
                else
                {
                    // First record — no delta possible.
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
                        SoC = currentObject.SoC,
                        MissedFuelUp = currentObject.MissedFuelUp,
                        Notes = currentObject.Notes,
                        Tags = currentObject.Tags,
                        ExtraFields = currentObject.ExtraFields,
                        Files = currentObject.Files
                    });
                    // Initialise SoC tracker for first record.
                    // Consumption = 0: the opening entry doesn't count toward the segment's efficiency.
                    if (currentObject.SoC > 0 && currentObject.Mileage != default)
                    {
                        socTracker[currentObject.SoC] = (0, currentObject.Mileage);
                    }
                }
                if (currentObject.Mileage != default)
                {
                    previousMileage = currentObject.Mileage;
                }
            }
            return computedResults;
        }
    }
}

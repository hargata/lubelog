using CarCareTracker.Models;
using System.Globalization;
using System.Runtime.Intrinsics.X86;

namespace CarCareTracker.Helper
{
    public interface IReportHelper
    {
        List<CostForVehicleByMonth> InterpolateDistanceTraveled(List<CostForVehicleByMonth> inputData, int year, int previousYearMaxMileage);
        IEnumerable<CostForVehicleByMonth> GetOdometerRecordSum(List<OdometerRecord> odometerRecords, int year = 0);
        IEnumerable<CostForVehicleByMonth> GetServiceRecordSum(List<ServiceRecord> serviceRecords, int year = 0);
        IEnumerable<CostForVehicleByMonth> GetRepairRecordSum(List<CollisionRecord> repairRecords, int year = 0);
        IEnumerable<CostForVehicleByMonth> GetUpgradeRecordSum(List<UpgradeRecord> upgradeRecords, int year = 0);
        IEnumerable<CostForVehicleByMonth> GetGasRecordSum(List<GasRecord> gasRecords, int year = 0);
        IEnumerable<CostForVehicleByMonth> GetTaxRecordSum(List<TaxRecord> taxRecords, int year = 0);
    }
    public class ReportHelper: IReportHelper
    {
        public List<CostForVehicleByMonth> InterpolateDistanceTraveled(List<CostForVehicleByMonth> inputData, int year, int previousYearMaxMileage)
        {
            int previousMonthMaxMileage = 0;
            for (int i = 0; i < inputData.Count; i++)
            {
                var currentData = inputData[i];
                if (year != 0)
                {
                    //specific year
                    if (currentData.MonthId == 1) //January
                    {
                        if (currentData.MaxMileage == 0)
                        {
                            currentData.DistanceTraveled = 0;
                        } else
                        {
                            var trueMin = 0;
                            if (previousYearMaxMileage == 0 && currentData.MinMileage != 0)
                            {
                                trueMin = currentData.MinMileage;
                            }
                            else if (previousYearMaxMileage != 0 && currentData.MinMileage == 0)
                            {
                                trueMin = previousYearMaxMileage;
                            }
                            else if (previousYearMaxMileage != 0 && currentData.MinMileage != 0)
                            {
                                trueMin = Math.Min(currentData.MinMileage, previousYearMaxMileage);
                            }
                            currentData.DistanceTraveled = trueMin == 0 ? 0 : currentData.MaxMileage - trueMin;
                        }
                        
                    }
                    else
                    {
                        //any other month
                        if (currentData.MaxMileage == 0)
                        {
                            currentData.DistanceTraveled = 0;
                        } else
                        {
                            var trueMin = 0;
                            if (previousMonthMaxMileage == 0 && currentData.MinMileage != 0)
                            {
                                trueMin = currentData.MinMileage;
                            }
                            else if (previousMonthMaxMileage != 0 && currentData.MinMileage == 0)
                            {
                                trueMin = previousMonthMaxMileage;
                            }
                            else if (previousMonthMaxMileage != 0 && currentData.MinMileage != 0)
                            {
                                trueMin = Math.Min(currentData.MinMileage, previousMonthMaxMileage);
                            }
                            currentData.DistanceTraveled = trueMin == 0 ? 0 : currentData.MaxMileage - trueMin;
                        }
                    }
                }
                else
                {
                    currentData.DistanceTraveled = currentData.MinMileage == 0 ? 0 :currentData.MaxMileage - currentData.MinMileage;
                }
                previousMonthMaxMileage = currentData.MaxMileage;
            }
            return inputData;
        }
        public IEnumerable<CostForVehicleByMonth> GetOdometerRecordSum(List<OdometerRecord> odometerRecords, int year = 0)
        {
            if (year != default)
            {
                odometerRecords.RemoveAll(x => x.Date.Year != year);
            }
            return odometerRecords.GroupBy(x => x.Date.Month).OrderBy(x => x.Key).Select(x => new CostForVehicleByMonth
            {
                MonthId = x.Key,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key),
                Cost = 0,
                MaxMileage = x.Max(y => y.Mileage),
                MinMileage = x.Min(y => y.Mileage)
            });
        }
        public IEnumerable<CostForVehicleByMonth> GetServiceRecordSum(List<ServiceRecord> serviceRecords, int year = 0)
        {
            if (year != default)
            {
                serviceRecords.RemoveAll(x => x.Date.Year != year);
            }
            return serviceRecords.GroupBy(x => x.Date.Month).OrderBy(x => x.Key).Select(x => new CostForVehicleByMonth
            {
                MonthId = x.Key,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key),
                Cost = x.Sum(y => y.Cost),
                MaxMileage = x.Max(y=>y.Mileage),
                MinMileage = x.Min(y=>y.Mileage)
            });
        }
        public IEnumerable<CostForVehicleByMonth> GetRepairRecordSum(List<CollisionRecord> repairRecords, int year = 0)
        {
            if (year != default)
            {
                repairRecords.RemoveAll(x => x.Date.Year != year);
            }
            return repairRecords.GroupBy(x => x.Date.Month).OrderBy(x => x.Key).Select(x => new CostForVehicleByMonth
            {
                MonthId = x.Key,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key),
                Cost = x.Sum(y => y.Cost),
                MaxMileage = x.Max(y => y.Mileage),
                MinMileage = x.Min(y => y.Mileage)
            });
        }
        public IEnumerable<CostForVehicleByMonth> GetUpgradeRecordSum(List<UpgradeRecord> upgradeRecords, int year = 0)
        {
            if (year != default)
            {
                upgradeRecords.RemoveAll(x => x.Date.Year != year);
            }
            return upgradeRecords.GroupBy(x => x.Date.Month).OrderBy(x => x.Key).Select(x => new CostForVehicleByMonth
            {
                MonthId = x.Key,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key),
                Cost = x.Sum(y => y.Cost),
                MaxMileage = x.Max(y => y.Mileage),
                MinMileage = x.Min(y => y.Mileage)
            });
        }
        public IEnumerable<CostForVehicleByMonth> GetGasRecordSum(List<GasRecord> gasRecords, int year = 0)
        {
            if (year != default)
            {
                gasRecords.RemoveAll(x => x.Date.Year != year);
            }
            return gasRecords.GroupBy(x => x.Date.Month).OrderBy(x => x.Key).Select(x => new CostForVehicleByMonth
            {
                MonthId = x.Key,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key),
                Cost = x.Sum(y => y.Cost),
                MaxMileage = x.Max(y => y.Mileage),
                MinMileage = x.Min(y => y.Mileage)
            });
        }
        public IEnumerable<CostForVehicleByMonth> GetTaxRecordSum(List<TaxRecord> taxRecords, int year = 0)
        {
            if (year != default)
            {
                taxRecords.RemoveAll(x => x.Date.Year != year);
            }
            return taxRecords.GroupBy(x => x.Date.Month).OrderBy(x => x.Key).Select(x => new CostForVehicleByMonth
            {
                MonthId = x.Key,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key),
                Cost = x.Sum(y => y.Cost)
            });
        }
    }
}

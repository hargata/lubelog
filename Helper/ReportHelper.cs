using CarCareTracker.Models;
using System.Globalization;

namespace CarCareTracker.Helper
{
    public interface IReportHelper
    {
        IEnumerable<CostForVehicleByMonth> GetOdometerRecordSum(List<OdometerRecord> odometerRecords, int year = 0, bool sortIntoYear = false);
        IEnumerable<CostForVehicleByMonth> GetServiceRecordSum(List<ServiceRecord> serviceRecords, int year = 0, bool sortIntoYear = false);
        IEnumerable<CostForVehicleByMonth> GetRepairRecordSum(List<CollisionRecord> repairRecords, int year = 0, bool sortIntoYear = false);
        IEnumerable<CostForVehicleByMonth> GetUpgradeRecordSum(List<UpgradeRecord> upgradeRecords, int year = 0, bool sortIntoYear = false);
        IEnumerable<CostForVehicleByMonth> GetGasRecordSum(List<GasRecord> gasRecords, int year = 0, bool sortIntoYear = false);
        IEnumerable<CostForVehicleByMonth> GetTaxRecordSum(List<TaxRecord> taxRecords, int year = 0, bool sortIntoYear = false);
    }
    public class ReportHelper: IReportHelper
    {
        public IEnumerable<CostForVehicleByMonth> GetOdometerRecordSum(List<OdometerRecord> odometerRecords, int year = 0, bool sortIntoYear = false)
        {
            if (year != default)
            {
                odometerRecords.RemoveAll(x => x.Date.Year != year);
            }
            if (sortIntoYear)
            {
                return odometerRecords.GroupBy(x => new { x.Date.Month, x.Date.Year }).OrderBy(x => x.Key.Month).Select(x => new CostForVehicleByMonth
                {
                    MonthId = x.Key.Month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key.Month),
                    Year = x.Key.Year,
                    Cost = 0,
                    DistanceTraveled = x.Sum(y => y.DistanceTraveled)
                });
            } else
            {
                return odometerRecords.GroupBy(x => x.Date.Month).OrderBy(x => x.Key).Select(x => new CostForVehicleByMonth
                {
                    MonthId = x.Key,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key),
                    Cost = 0,
                    DistanceTraveled = x.Sum(y => y.DistanceTraveled)
                });
            }
        }
        public IEnumerable<CostForVehicleByMonth> GetServiceRecordSum(List<ServiceRecord> serviceRecords, int year = 0, bool sortIntoYear = false)
        {
            if (year != default)
            {
                serviceRecords.RemoveAll(x => x.Date.Year != year);
            }
            if (sortIntoYear)
            {
                return serviceRecords.GroupBy(x => new { x.Date.Month, x.Date.Year }).OrderBy(x => x.Key.Month).Select(x => new CostForVehicleByMonth
                {
                    MonthId = x.Key.Month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key.Month),
                    Year = x.Key.Year,
                    Cost = x.Sum(y => y.Cost)
                });
            } else
            {
                return serviceRecords.GroupBy(x => x.Date.Month).OrderBy(x => x.Key).Select(x => new CostForVehicleByMonth
                {
                    MonthId = x.Key,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key),
                    Cost = x.Sum(y => y.Cost)
                });
            }
        }
        public IEnumerable<CostForVehicleByMonth> GetRepairRecordSum(List<CollisionRecord> repairRecords, int year = 0, bool sortIntoYear = false)
        {
            if (year != default)
            {
                repairRecords.RemoveAll(x => x.Date.Year != year);
            }
            if (sortIntoYear)
            {
                return repairRecords.GroupBy(x => new { x.Date.Month, x.Date.Year }).OrderBy(x => x.Key.Month).Select(x => new CostForVehicleByMonth
                {
                    MonthId = x.Key.Month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key.Month),
                    Year = x.Key.Year,
                    Cost = x.Sum(y => y.Cost)
                });
            } else
            {
                return repairRecords.GroupBy(x => x.Date.Month).OrderBy(x => x.Key).Select(x => new CostForVehicleByMonth
                {
                    MonthId = x.Key,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key),
                    Cost = x.Sum(y => y.Cost)
                });
            }
        }
        public IEnumerable<CostForVehicleByMonth> GetUpgradeRecordSum(List<UpgradeRecord> upgradeRecords, int year = 0, bool sortIntoYear = false)
        {
            if (year != default)
            {
                upgradeRecords.RemoveAll(x => x.Date.Year != year);
            }
            if (sortIntoYear)
            {
                return upgradeRecords.GroupBy(x => new { x.Date.Month, x.Date.Year }).OrderBy(x => x.Key.Month).Select(x => new CostForVehicleByMonth
                {
                    MonthId = x.Key.Month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key.Month),
                    Year = x.Key.Year,
                    Cost = x.Sum(y => y.Cost)
                });
            } else
            {
                return upgradeRecords.GroupBy(x => x.Date.Month).OrderBy(x => x.Key).Select(x => new CostForVehicleByMonth
                {
                    MonthId = x.Key,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key),
                    Cost = x.Sum(y => y.Cost)
                });
            }
        }
        public IEnumerable<CostForVehicleByMonth> GetGasRecordSum(List<GasRecord> gasRecords, int year = 0, bool sortIntoYear = false)
        {
            if (year != default)
            {
                gasRecords.RemoveAll(x => x.Date.Year != year);
            }
            if (sortIntoYear)
            {
                return gasRecords.GroupBy(x => new { x.Date.Month, x.Date.Year }).OrderBy(x => x.Key.Month).Select(x => new CostForVehicleByMonth
                {
                    MonthId = x.Key.Month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key.Month),
                    Year = x.Key.Year,
                    Cost = x.Sum(y => y.Cost)
                });
            } else
            {
                return gasRecords.GroupBy(x => x.Date.Month).OrderBy(x => x.Key).Select(x => new CostForVehicleByMonth
                {
                    MonthId = x.Key,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key),
                    Cost = x.Sum(y => y.Cost)
                });
            }
        }
        public IEnumerable<CostForVehicleByMonth> GetTaxRecordSum(List<TaxRecord> taxRecords, int year = 0, bool sortIntoYear = false)
        {
            if (year != default)
            {
                taxRecords.RemoveAll(x => x.Date.Year != year);
            }
            if (sortIntoYear)
            {
                return taxRecords.GroupBy(x => new { x.Date.Month, x.Date.Year }).OrderBy(x => x.Key.Month).Select(x => new CostForVehicleByMonth
                {
                    MonthId = x.Key.Month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key.Month),
                    Year = x.Key.Year,
                    Cost = x.Sum(y => y.Cost)
                });
            } else
            {
                return taxRecords.GroupBy(x => x.Date.Month).OrderBy(x => x.Key).Select(x => new CostForVehicleByMonth
                {
                    MonthId = x.Key,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(x.Key),
                    Cost = x.Sum(y => y.Cost)
                });
            }
        }
    }
}

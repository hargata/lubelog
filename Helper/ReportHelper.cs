using CarCareTracker.Models;
using System.Globalization;

namespace CarCareTracker.Helper
{
    public interface IReportHelper
    {
        IEnumerable<CostForVehicleByMonth> GetOdometerRecordSum(List<OdometerRecord> odometerRecords, int year = 0);
        IEnumerable<CostForVehicleByMonth> GetServiceRecordSum(List<ServiceRecord> serviceRecords, int year = 0);
        IEnumerable<CostForVehicleByMonth> GetRepairRecordSum(List<CollisionRecord> repairRecords, int year = 0);
        IEnumerable<CostForVehicleByMonth> GetUpgradeRecordSum(List<UpgradeRecord> upgradeRecords, int year = 0);
        IEnumerable<CostForVehicleByMonth> GetGasRecordSum(List<GasRecord> gasRecords, int year = 0);
        IEnumerable<CostForVehicleByMonth> GetTaxRecordSum(List<TaxRecord> taxRecords, int year = 0);
    }
    public class ReportHelper: IReportHelper
    {
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

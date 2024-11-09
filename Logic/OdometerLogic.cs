using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;

namespace CarCareTracker.Logic
{
    public interface IOdometerLogic
    {
        int GetLastOdometerRecordMileage(int vehicleId, List<OdometerRecord> odometerRecords);
        void AutoInsertOdometerRecord(OdometerRecord odometer);
        List<OdometerRecord> AutoConvertOdometerRecord(List<OdometerRecord> odometerRecords);
    }
    public class OdometerLogic: IOdometerLogic
    {
        private readonly IOdometerRecordDataAccess _odometerRecordDataAccess;
        private readonly ILogger<IOdometerLogic> _logger;
        public OdometerLogic(IOdometerRecordDataAccess odometerRecordDataAccess, ILogger<IOdometerLogic> logger)
        {
            _odometerRecordDataAccess = odometerRecordDataAccess;
            _logger = logger;
        }
        public int GetLastOdometerRecordMileage(int vehicleId, List<OdometerRecord> odometerRecords)
        {
            if (odometerRecords.Count == 0)
            {
                odometerRecords = _odometerRecordDataAccess.GetOdometerRecordsByVehicleId(vehicleId);
            }
            if (odometerRecords.Count == 0)
            {
                //no existing odometer records for this vehicle.
                return 0;
            }
            return odometerRecords.Max(x => x.Mileage);
        }
        public void AutoInsertOdometerRecord(OdometerRecord odometer)
        {
            if (odometer.Mileage == default)
            {
                return;
            }
            var lastReportedMileage = GetLastOdometerRecordMileage(odometer.VehicleId, new List<OdometerRecord>());
            odometer.InitialMileage = lastReportedMileage != default ? lastReportedMileage : odometer.Mileage;

            _ = _odometerRecordDataAccess.SaveOdometerRecordToVehicle(odometer);
        }
        public List<OdometerRecord> AutoConvertOdometerRecord(List<OdometerRecord> odometerRecords)
        {
            //perform ordering
            odometerRecords = odometerRecords.OrderBy(x => x.Date).ThenBy(x => x.Mileage).ToList();
            int previousMileage = 0;
            foreach (var currentObject in odometerRecords)
            {
                if (previousMileage == default)
                {
                    //first record
                    currentObject.InitialMileage = currentObject.Mileage;
                }
                else
                {
                    //subsequent records
                    currentObject.InitialMileage = previousMileage;
                }
                //save to db.
                _odometerRecordDataAccess.SaveOdometerRecordToVehicle(currentObject);
                previousMileage = currentObject.Mileage;
            }
            return odometerRecords;
        }
    }
}

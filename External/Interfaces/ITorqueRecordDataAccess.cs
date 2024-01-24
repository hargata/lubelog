using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface ITorqueRecordDataAccess
    {
        public TorqueRecord GetTorqueRecordById(string torqueRecordId);
        public bool DeleteTorqueRecordById(int torqueRecordId);
        public bool SaveTorqueRecord(TorqueRecord torqueRecord);
    }
}

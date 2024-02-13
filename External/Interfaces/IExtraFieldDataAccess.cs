using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IExtraFieldDataAccess
    {
        public RecordExtraField GetExtraFieldsById(int importMode);
        public bool SaveExtraFields(RecordExtraField record);
    }
}

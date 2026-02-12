using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IExtraFieldDataAccess
    {
        public List<RecordExtraField> GetExtraFields();
        public RecordExtraField GetExtraFieldsById(int importMode);
        public bool SaveExtraFields(RecordExtraField record);
    }
}

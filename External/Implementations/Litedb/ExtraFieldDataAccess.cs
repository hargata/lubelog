using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;

namespace CarCareTracker.External.Implementations
{
    public class ExtraFieldDataAccess : IExtraFieldDataAccess
    {
        private ILiteDBHelper _liteDB { get; set; }
        private static string tableName = "extrafields";
        public ExtraFieldDataAccess(ILiteDBHelper liteDB)
        {
           _liteDB = liteDB;
        }
        public List<RecordExtraField> GetExtraFields()
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<RecordExtraField>(tableName);
            return table.FindAll().ToList();
        }
        public RecordExtraField GetExtraFieldsById(int importMode)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<RecordExtraField>(tableName);
            return table.FindById(importMode) ?? new RecordExtraField();
        }
        public bool SaveExtraFields(RecordExtraField record)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<RecordExtraField>(tableName);
            table.Upsert(record);
            db.Checkpoint();
            return true;
        }
    }
}

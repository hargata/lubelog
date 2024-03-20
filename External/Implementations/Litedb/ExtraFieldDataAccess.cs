using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class ExtraFieldDataAccess : IExtraFieldDataAccess
    {
        private ILiteDBInjection _liteDB { get; set; }
        private static string tableName = "extrafields";
        public ExtraFieldDataAccess(ILiteDBInjection liteDB)
        {
           _liteDB = liteDB;
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

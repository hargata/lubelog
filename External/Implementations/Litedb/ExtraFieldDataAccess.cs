using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class ExtraFieldDataAccess : IExtraFieldDataAccess
    {
        private LiteDatabase db { get; set; }
        private static string tableName = "extrafields";
        public ExtraFieldDataAccess(ILiteDBInjection liteDB)
        {
            db = liteDB.GetLiteDB();
        }
        public RecordExtraField GetExtraFieldsById(int importMode)
        {
            var table = db.GetCollection<RecordExtraField>(tableName);
            return table.FindById(importMode) ?? new RecordExtraField();
        }
        public bool SaveExtraFields(RecordExtraField record)
        {
            var table = db.GetCollection<RecordExtraField>(tableName);
            table.Upsert(record);
            db.Checkpoint();
            return true;
        }
    }
}

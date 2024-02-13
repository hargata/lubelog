using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class ExtraFieldDataAccess: IExtraFieldDataAccess
    {
        private static string dbName = StaticHelper.DbName;
        private static string tableName = "extrafields";
        public RecordExtraField GetExtraFieldsById(int importMode)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<RecordExtraField>(tableName);
                return table.FindById(importMode) ?? new RecordExtraField();
            };
        }
        public bool SaveExtraFields(RecordExtraField record)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<RecordExtraField>(tableName);
                table.Upsert(record);
                return true;
            };
        }
    }
}

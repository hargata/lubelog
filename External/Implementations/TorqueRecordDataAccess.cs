using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class TorqueRecordDataAccess: ITorqueRecordDataAccess
    {
        private static string dbName = StaticHelper.DbName;
        private static string tableName = "torquerecords";
        public TorqueRecord GetTorqueRecordById(string torqueRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<TorqueRecord>(tableName);
                return table.FindById(torqueRecordId);
            };
        }
        public bool DeleteTorqueRecordById(int torqueRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<TorqueRecord>(tableName);
                table.Delete(torqueRecordId);
                return true;
            };
        }
        public bool SaveTorqueRecord(TorqueRecord torqueRecord)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<TorqueRecord>(tableName);
                table.Upsert(torqueRecord);
                return true;
            };
        }
    }
}

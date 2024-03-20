using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class TaxRecordDataAccess : ITaxRecordDataAccess
    {
        private ILiteDBInjection _liteDB { get; set; }
        private static string tableName = "taxrecords";
        public TaxRecordDataAccess(ILiteDBInjection liteDB)
        {
           _liteDB = liteDB;
        }
        public List<TaxRecord> GetTaxRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<TaxRecord>(tableName);
            var taxRecords = table.Find(Query.EQ(nameof(TaxRecord.VehicleId), vehicleId));
            return taxRecords.ToList() ?? new List<TaxRecord>();
        }
        public TaxRecord GetTaxRecordById(int taxRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<TaxRecord>(tableName);
            return table.FindById(taxRecordId);
        }
        public bool DeleteTaxRecordById(int taxRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<TaxRecord>(tableName);
            table.Delete(taxRecordId);
            db.Checkpoint();
            return true;
        }
        public bool SaveTaxRecordToVehicle(TaxRecord taxRecord)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<TaxRecord>(tableName);
            table.Upsert(taxRecord);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAllTaxRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<TaxRecord>(tableName);
            var taxRecords = table.DeleteMany(Query.EQ(nameof(TaxRecord.VehicleId), vehicleId));
            db.Checkpoint();
            return true;
        }
    }
}

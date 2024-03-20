using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class TaxRecordDataAccess : ITaxRecordDataAccess
    {
        private LiteDatabase db { get; set; }
        private static string tableName = "taxrecords";
        public TaxRecordDataAccess(ILiteDBInjection liteDB)
        {
            db = liteDB.GetLiteDB();
        }
        public List<TaxRecord> GetTaxRecordsByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<TaxRecord>(tableName);
            var taxRecords = table.Find(Query.EQ(nameof(TaxRecord.VehicleId), vehicleId));
            return taxRecords.ToList() ?? new List<TaxRecord>();
        }
        public TaxRecord GetTaxRecordById(int taxRecordId)
        {
            var table = db.GetCollection<TaxRecord>(tableName);
            return table.FindById(taxRecordId);
        }
        public bool DeleteTaxRecordById(int taxRecordId)
        {
            var table = db.GetCollection<TaxRecord>(tableName);
            table.Delete(taxRecordId);
            return true;
        }
        public bool SaveTaxRecordToVehicle(TaxRecord taxRecord)
        {
            var table = db.GetCollection<TaxRecord>(tableName);
            table.Upsert(taxRecord);
            return true;
        }
        public bool DeleteAllTaxRecordsByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<TaxRecord>(tableName);
            var taxRecords = table.DeleteMany(Query.EQ(nameof(TaxRecord.VehicleId), vehicleId));
            return true;
        }
    }
}

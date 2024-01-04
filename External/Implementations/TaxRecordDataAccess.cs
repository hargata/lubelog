using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class TaxRecordDataAccess : ITaxRecordDataAccess
    {
        private static string dbName = "cartracker.db";
        private static string tableName = "taxrecords";
        public List<TaxRecord> GetTaxRecordsByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<TaxRecord>(tableName);
                var taxRecords = table.Find(Query.EQ(nameof(TaxRecord.VehicleId), vehicleId));
                return taxRecords.ToList() ?? new List<TaxRecord>();
            };
        }
        public TaxRecord GetTaxRecordById(int taxRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<TaxRecord>(tableName);
                return table.FindById(taxRecordId);
            };
        }
        public bool DeleteTaxRecordById(int taxRecordId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<TaxRecord>(tableName);
                table.Delete(taxRecordId);
                return true;
            };
        }
        public bool SaveTaxRecordToVehicle(TaxRecord taxRecord)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<TaxRecord>(tableName);
                table.Upsert(taxRecord);
                return true;
            };
        }
        public bool DeleteAllTaxRecordsByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<TaxRecord>(tableName);
                var taxRecords = table.DeleteMany(Query.EQ(nameof(TaxRecord.VehicleId), vehicleId));
                return true;
            };
        }
    }
}

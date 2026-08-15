using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using CarCareTracker.Helper;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class InsuranceRecordDataAccess : IInsuranceRecordDataAccess
    {
        private ILiteDBHelper _liteDB { get; set; }
        private static string tableName = "insurancerecords";
        public InsuranceRecordDataAccess(ILiteDBHelper liteDB)
        {
           _liteDB = liteDB;
        }
        public List<InsuranceRecord> GetInsuranceRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<InsuranceRecord>(tableName);
            var insuranceRecords = table.Find(Query.EQ(nameof(InsuranceRecord.VehicleId), vehicleId));
            return insuranceRecords.ToList() ?? new List<InsuranceRecord>();
        }
        public InsuranceRecord GetInsuranceRecordById(int insuranceRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<InsuranceRecord>(tableName);
            return table.FindById(insuranceRecordId);
        }
        public bool DeleteInsuranceRecordById(int insuranceRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<InsuranceRecord>(tableName);
            table.Delete(insuranceRecordId);
            db.Checkpoint();
            return true;
        }
        public bool SaveInsuranceRecordToVehicle(InsuranceRecord insuranceRecord)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<InsuranceRecord>(tableName);
            table.Upsert(insuranceRecord);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAllInsuranceRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<InsuranceRecord>(tableName);
            var insuranceRecords = table.DeleteMany(Query.EQ(nameof(InsuranceRecord.VehicleId), vehicleId));
            db.Checkpoint();
            return true;
        }
    }
}

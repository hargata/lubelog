using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using CarCareTracker.Helper;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class ServiceRecordDataAccess : IServiceRecordDataAccess
    {
        private ILiteDBHelper _liteDB { get; set; }
        private static string tableName = "servicerecords";
        public ServiceRecordDataAccess(ILiteDBHelper liteDB)
        {
           _liteDB = liteDB;
        }
        public List<ServiceRecord> GetServiceRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<ServiceRecord>(tableName);
            var serviceRecords = table.Find(Query.EQ(nameof(ServiceRecord.VehicleId), vehicleId));
            return serviceRecords.ToList() ?? new List<ServiceRecord>();
        }
        public ServiceRecord GetServiceRecordById(int serviceRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<ServiceRecord>(tableName);
            return table.FindById(serviceRecordId);
        }
        public bool DeleteServiceRecordById(int serviceRecordId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<ServiceRecord>(tableName);
            table.Delete(serviceRecordId);
            db.Checkpoint();
            return true;
        }
        public bool SaveServiceRecordToVehicle(ServiceRecord serviceRecord)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<ServiceRecord>(tableName);
            table.Upsert(serviceRecord);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAllServiceRecordsByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<ServiceRecord>(tableName);
            var serviceRecords = table.DeleteMany(Query.EQ(nameof(ServiceRecord.VehicleId), vehicleId));
            db.Checkpoint();
            return true;
        }
    }
}

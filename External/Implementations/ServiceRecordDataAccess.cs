using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class ServiceRecordDataAccess: IServiceRecordDataAccess
    {
        private static string dbName = "cartracker.db";
        private static string tableName = "servicerecords";
        public List<ServiceRecord> GetServiceRecordsByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<ServiceRecord>(tableName);
                var serviceRecords = table.Find(Query.EQ(nameof(ServiceRecord.VehicleId), vehicleId)).OrderBy(x=>x.Date);
                return serviceRecords.ToList() ?? new List<ServiceRecord>();
            };
        }
        public bool SaveServiceRecordToVehicle(ServiceRecord serviceRecord)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<ServiceRecord>(tableName);
                table.Upsert(serviceRecord);
                return true;
            };
        }
    }
}

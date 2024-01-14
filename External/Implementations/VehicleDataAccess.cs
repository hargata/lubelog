using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class VehicleDataAccess: IVehicleDataAccess
    {
        private static string dbName = StaticHelper.DbName;
        private static string tableName = "vehicles";
        public bool SaveVehicle(Vehicle vehicle)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Vehicle>(tableName);
                var result = table.Upsert(vehicle);
                return true;
            };
        }
        public bool DeleteVehicle(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Vehicle>(tableName);
                return table.Delete(vehicleId);
            };
        }
        public List<Vehicle> GetVehicles()
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Vehicle>(tableName);
                return table.FindAll().ToList();
            };
        }
        public Vehicle GetVehicleById(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Vehicle>(tableName);
                return table.FindById(vehicleId);
            };
        }
    }
}

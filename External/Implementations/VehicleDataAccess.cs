using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class VehicleDataAccess: IVehicleDataAccess
    {
        private static string dbName = "cartracker.db";
        private static string tableName = "vehicles";
        public bool AddVehicle(Vehicle newVehicle)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Vehicle>(tableName);
                table.Insert(newVehicle);
            };
            return true;
        }
        public bool DeleteVehicle(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Vehicle>(tableName);
                table.Delete(vehicleId);
            };
            return true;
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

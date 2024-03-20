using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class VehicleDataAccess : IVehicleDataAccess
    {
        private LiteDatabase db { get; set; }
        private static string tableName = "vehicles";
        public VehicleDataAccess(ILiteDBInjection liteDB)
        {
            db = liteDB.GetLiteDB();
        }
        public bool SaveVehicle(Vehicle vehicle)
        {
            var table = db.GetCollection<Vehicle>(tableName);
            var result = table.Upsert(vehicle);
            db.Checkpoint();
            return true;
        }
        public bool DeleteVehicle(int vehicleId)
        {
            var table = db.GetCollection<Vehicle>(tableName);
            var result = table.Delete(vehicleId);
            db.Checkpoint();
            return result;
        }
        public List<Vehicle> GetVehicles()
        {
            var table = db.GetCollection<Vehicle>(tableName);
            return table.FindAll().ToList();
        }
        public Vehicle GetVehicleById(int vehicleId)
        {
            var table = db.GetCollection<Vehicle>(tableName);
            return table.FindById(vehicleId);
        }
    }
}

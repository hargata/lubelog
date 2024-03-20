using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class VehicleDataAccess : IVehicleDataAccess
    {
        private ILiteDBInjection _liteDB { get; set; }
        private static string tableName = "vehicles";
        public VehicleDataAccess(ILiteDBInjection liteDB)
        {
           _liteDB = liteDB;
        }
        public bool SaveVehicle(Vehicle vehicle)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<Vehicle>(tableName);
            var result = table.Upsert(vehicle);
            db.Checkpoint();
            return true;
        }
        public bool DeleteVehicle(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<Vehicle>(tableName);
            var result = table.Delete(vehicleId);
            db.Checkpoint();
            return result;
        }
        public List<Vehicle> GetVehicles()
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<Vehicle>(tableName);
            return table.FindAll().ToList();
        }
        public Vehicle GetVehicleById(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<Vehicle>(tableName);
            return table.FindById(vehicleId);
        }
    }
}

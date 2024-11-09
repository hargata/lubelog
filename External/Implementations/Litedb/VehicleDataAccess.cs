using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;

namespace CarCareTracker.External.Implementations
{
    public class VehicleDataAccess : IVehicleDataAccess
    {
        private ILiteDBHelper _liteDB { get; }
        private static string tableName = "vehicles";
        public VehicleDataAccess(ILiteDBHelper liteDB)
        {
           _liteDB = liteDB;
        }
        public bool SaveVehicle(Vehicle vehicle)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<Vehicle>(tableName);
            table.Upsert(vehicle);
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

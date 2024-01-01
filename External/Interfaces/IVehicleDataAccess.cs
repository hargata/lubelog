using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IVehicleDataAccess
    {
        public bool AddVehicle(Vehicle newVehicle);
        public bool DeleteVehicle(int vehicleId);
        public List<Vehicle> GetVehicles();
        public Vehicle GetVehicleById(int vehicleId);
    }
}

using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface ITaxRecordDataAccess
    {
        public List<TaxRecord> GetTaxRecordsByVehicleId(int vehicleId);
        public TaxRecord GetTaxRecordById(int taxRecordId);
        public bool DeleteTaxRecordById(int taxRecordId);
        public bool SaveTaxRecordToVehicle(TaxRecord taxRecord);
        public bool DeleteAllTaxRecordsByVehicleId(int vehicleId);
    }
}

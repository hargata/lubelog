using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface INoteDataAccess
    {
        public Note GetNoteByVehicleId(int vehicleId);
        public bool SaveNoteToVehicleId(Note note);
        bool DeleteNoteByVehicleId(int vehicleId);
    }
}

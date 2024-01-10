using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface INoteDataAccess
    {
        public List<Note> GetNotesByVehicleId(int vehicleId);
        public Note GetNoteById(int noteId);
        public bool SaveNoteToVehicle(Note note);
        public bool DeleteNoteById(int noteId);
        public bool DeleteAllNotesByVehicleId(int vehicleId);
    }
}

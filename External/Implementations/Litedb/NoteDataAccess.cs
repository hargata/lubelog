using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class NoteDataAccess : INoteDataAccess
    {
        private LiteDatabase db { get; set; }
        private static string tableName = "notes";
        public NoteDataAccess(ILiteDBInjection liteDB)
        {
            db = liteDB.GetLiteDB();
        }
        public List<Note> GetNotesByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<Note>(tableName);
            var noteToReturn = table.Find(Query.EQ(nameof(Note.VehicleId), vehicleId));
            return noteToReturn.ToList() ?? new List<Note>();
        }
        public Note GetNoteById(int noteId)
        {
            var table = db.GetCollection<Note>(tableName);
            return table.FindById(noteId);
        }
        public bool SaveNoteToVehicle(Note note)
        {
            var table = db.GetCollection<Note>(tableName);
            table.Upsert(note);
            return true;
        }
        public bool DeleteNoteById(int noteId)
        {
            var table = db.GetCollection<Note>(tableName);
            table.Delete(noteId);
            return true;
        }
        public bool DeleteAllNotesByVehicleId(int vehicleId)
        {
            var table = db.GetCollection<Note>(tableName);
            var notes = table.DeleteMany(Query.EQ(nameof(Note.VehicleId), vehicleId));
            return true;
        }
    }
}

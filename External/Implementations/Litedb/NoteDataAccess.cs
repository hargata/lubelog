using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class NoteDataAccess : INoteDataAccess
    {
        private ILiteDBHelper _liteDB { get; }
        private static string tableName = "notes";
        public NoteDataAccess(ILiteDBHelper liteDB)
        {
           _liteDB = liteDB;
        }
        public List<Note> GetNotesByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<Note>(tableName);
            var noteToReturn = table.Find(Query.EQ(nameof(Note.VehicleId), vehicleId));
            return noteToReturn.ToList() ?? new List<Note>();
        }
        public Note GetNoteById(int noteId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<Note>(tableName);
            return table.FindById(noteId);
        }
        public bool SaveNoteToVehicle(Note note)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<Note>(tableName);
            table.Upsert(note);
            db.Checkpoint();
            return true;
        }
        public bool DeleteNoteById(int noteId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<Note>(tableName);
            table.Delete(noteId);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAllNotesByVehicleId(int vehicleId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<Note>(tableName);
            table.DeleteMany(Query.EQ(nameof(Note.VehicleId), vehicleId));
            db.Checkpoint();
            return true;
        }
    }
}

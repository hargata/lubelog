using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class NoteDataAccess: INoteDataAccess
    {
        private static string dbName = StaticHelper.DbName;
        private static string tableName = "notes";
        public List<Note> GetNotesByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Note>(tableName);
                var noteToReturn = table.Find(Query.EQ(nameof(Note.VehicleId), vehicleId));
                return noteToReturn.ToList() ?? new List<Note>();
            };
        }
        public Note GetNoteById(int noteId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Note>(tableName);
                return table.FindById(noteId);
            };
        }
        public bool SaveNoteToVehicle(Note note)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Note>(tableName);
                table.Upsert(note);
                return true;
            };
        }
        public bool DeleteNoteById(int noteId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Note>(tableName);
                table.Delete(noteId);
                return true;
            };
        }
        public bool DeleteAllNotesByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Note>(tableName);
                var notes = table.DeleteMany(Query.EQ(nameof(Note.VehicleId), vehicleId));
                return true;
            };
        }
    }
}

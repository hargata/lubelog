using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;

namespace CarCareTracker.External.Implementations
{
    public class NoteDataAccess: INoteDataAccess
    {
        private static string dbName = "data/cartracker.db";
        private static string tableName = "notes";
        public Note GetNoteByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Note>(tableName);
                var noteToReturn = table.FindOne(Query.EQ(nameof(Note.VehicleId), vehicleId));
                return noteToReturn ?? new Note();
            };
        }
        public bool SaveNoteToVehicleId(Note note)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Note>(tableName);
                table.Upsert(note);
                return true;
            };
        }
        public bool DeleteNoteByVehicleId(int vehicleId)
        {
            using (var db = new LiteDatabase(dbName))
            {
                var table = db.GetCollection<Note>(tableName);
                table.DeleteMany(Query.EQ(nameof(Note.VehicleId), vehicleId));
                return true;
            };
        }
    }
}

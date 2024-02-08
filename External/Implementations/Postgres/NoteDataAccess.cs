using CarCareTracker.External.Interfaces;
using CarCareTracker.Helper;
using CarCareTracker.Models;
using Npgsql;
using System.Text.Json;

namespace CarCareTracker.External.Implementations
{
    public class PGNoteDataAccess: INoteDataAccess
    {
        private NpgsqlConnection pgDataSource;
        public PGNoteDataAccess(IConfiguration config)
        {
            pgDataSource = new NpgsqlConnection(config["POSTGRES_CONNECTION"]);
            pgDataSource.Open();
            //create table if not exist.
            string initCMD = $"CREATE TABLE IF NOT EXISTS app.{tableName} (id INT GENERATED ALWAYS AS IDENTITY primary key, vehicleId INT not null, data jsonb not null)";
            using (var ctext = new NpgsqlCommand(initCMD, pgDataSource))
            {
                ctext.ExecuteNonQuery();
            }
        }
        private static string tableName = "notes";
        public List<Note> GetNotesByVehicleId(int vehicleId)
        {
            string cmd = $"SELECT data FROM app.{tableName} WHERE vehicleId = @vehicleId";
            var results = new List<Note>();
            using (var ctext = new NpgsqlCommand(cmd, pgDataSource))
            {
                ctext.Parameters.AddWithValue("vehicleId", vehicleId);
                using (NpgsqlDataReader reader = ctext.ExecuteReader())
                    while (reader.Read())
                    {
                        Note note = JsonSerializer.Deserialize<Note>(reader["data"] as string);
                        results.Add(note);
                    }
            }
            return results;
        }
        public Note GetNoteById(int noteId)
        {
            string cmd = $"SELECT data FROM app.{tableName} WHERE id = @id";
            var result = new Note();
            using (var ctext = new NpgsqlCommand(cmd, pgDataSource))
            {
                ctext.Parameters.AddWithValue("id", noteId);
                using (NpgsqlDataReader reader = ctext.ExecuteReader())
                    while (reader.Read())
                    {
                        Note note = JsonSerializer.Deserialize<Note>(reader["data"] as string);
                        result = note;
                    }
            }
            return result;
        }
        public bool SaveNoteToVehicle(Note note)
        {
            if (note.Id == default)
            {
                string cmd = $"INSERT INTO app.{tableName} (vehicleId, data) VALUES(@vehicleId, CAST(@data AS jsonb)) RETURNING id";
                using (var ctext = new NpgsqlCommand(cmd, pgDataSource))
                {
                    ctext.Parameters.AddWithValue("vehicleId", note.VehicleId);
                    ctext.Parameters.AddWithValue("data", "{}");
                    note.Id = Convert.ToInt32(ctext.ExecuteScalar());
                    //update json data
                    if (note.Id != default)
                    {
                        string cmdU = $"UPDATE app.{tableName} SET data = CAST(@data AS jsonb) WHERE id = @id";
                        using (var ctextU = new NpgsqlCommand(cmdU, pgDataSource))
                        {
                            var serializedData = JsonSerializer.Serialize(note);
                            ctextU.Parameters.AddWithValue("id", note.Id);
                            ctextU.Parameters.AddWithValue("data", serializedData);
                            return ctextU.ExecuteNonQuery() > 0;
                        }
                    }
                    return note.Id != default;
                }
            }
            else
            {
                string cmd = $"UPDATE app.{tableName} SET data = CAST(@data AS jsonb) WHERE id = @id";
                using (var ctext = new NpgsqlCommand(cmd, pgDataSource))
                {
                    var serializedData = JsonSerializer.Serialize(note);
                    ctext.Parameters.AddWithValue("id", note.Id);
                    ctext.Parameters.AddWithValue("data", serializedData);
                    return ctext.ExecuteNonQuery() > 0;
                }
            }
        }
        public bool DeleteNoteById(int noteId)
        {
            string cmd = $"DELETE FROM app.{tableName} WHERE id = @id";
            using (var ctext = new NpgsqlCommand(cmd, pgDataSource))
            {
                ctext.Parameters.AddWithValue("id", noteId);
                return ctext.ExecuteNonQuery() > 0;
            }
        }
        public bool DeleteAllNotesByVehicleId(int vehicleId)
        {
            string cmd = $"DELETE FROM app.{tableName} WHERE vehicleId = @id";
            using (var ctext = new NpgsqlCommand(cmd, pgDataSource))
            {
                ctext.Parameters.AddWithValue("id", vehicleId);
                return ctext.ExecuteNonQuery() > 0;
            }
        }
    }
}

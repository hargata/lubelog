using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using Npgsql;
using System.Text.Json;

namespace CarCareTracker.External.Implementations
{
    public class PGExtraFieldDataAccess : IExtraFieldDataAccess
    {
        private NpgsqlDataSource pgDataSource;
        private readonly ILogger<PGExtraFieldDataAccess> _logger;
        private static string tableName = "extrafields";
        public PGExtraFieldDataAccess(IConfiguration config, ILogger<PGExtraFieldDataAccess> logger)
        {
            pgDataSource = NpgsqlDataSource.Create(config["POSTGRES_CONNECTION"]);

            _logger = logger;
            try
            {
                //create table if not exist.
                string initCMD = $"CREATE SCHEMA IF NOT EXISTS app; CREATE TABLE IF NOT EXISTS app.{tableName} (id INT primary key, data jsonb not null)";
                using (var ctext = pgDataSource.CreateCommand(initCMD))
                {
                    ctext.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
        public List<RecordExtraField> GetExtraFields()
        {
            try
            {
                string cmd = $"SELECT data FROM app.{tableName}";
                var results = new List<RecordExtraField>();
                using (var ctext = pgDataSource.CreateCommand(cmd))
                {
                    using (NpgsqlDataReader reader = ctext.ExecuteReader())
                        while (reader.Read())
                        {
                            RecordExtraField result = JsonSerializer.Deserialize<RecordExtraField>(reader["data"] as string);
                            results.Add(result);
                        }
                }
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new List<RecordExtraField>();
            }
        }
        public RecordExtraField GetExtraFieldsById(int importMode)
        {
            try
            {
                string cmd = $"SELECT data FROM app.{tableName} WHERE id = @id";
                var results = new RecordExtraField();
                using (var ctext = pgDataSource.CreateCommand(cmd))
                {
                    ctext.Parameters.AddWithValue("id", importMode);
                    using (NpgsqlDataReader reader = ctext.ExecuteReader())
                        while (reader.Read())
                        {
                            RecordExtraField result = JsonSerializer.Deserialize<RecordExtraField>(reader["data"] as string);
                            results = result;
                        }
                }
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new RecordExtraField();
            }
        }
        public bool SaveExtraFields(RecordExtraField record)
        {
            try
            {
                var existingRecord = GetExtraFieldsById(record.Id);
                string cmd = $"INSERT INTO app.{tableName} (id, data) VALUES(@id, CAST(@data AS jsonb)) ON CONFLICT(id) DO UPDATE SET data = CAST(@data AS jsonb)";
                using (var ctext = pgDataSource.CreateCommand(cmd))
                {
                    ctext.Parameters.AddWithValue("id", record.Id);
                    ctext.Parameters.AddWithValue("data", JsonSerializer.Serialize(record));
                    return ctext.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }
    }
}

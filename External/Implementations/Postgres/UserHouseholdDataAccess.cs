using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using Npgsql;
using System.Text.Json;

namespace CarCareTracker.External.Implementations
{
    public class PGUserHouseholdDataAccess : IUserHouseholdDataAccess
    {
        private NpgsqlDataSource pgDataSource;
        private readonly ILogger<PGUserHouseholdDataAccess> _logger;
        private static string tableName = "userhouseholdrecords";
        public PGUserHouseholdDataAccess(IConfiguration config, ILogger<PGUserHouseholdDataAccess> logger)
        {
            pgDataSource = NpgsqlDataSource.Create(config["POSTGRES_CONNECTION"] ?? string.Empty);
            _logger = logger;
            try
            {
                //create table if not exist.
                string initCMD = $"CREATE SCHEMA IF NOT EXISTS app; CREATE TABLE IF NOT EXISTS app.{tableName} (parentUserId INT, childUserId INT, data jsonb not null, PRIMARY KEY(parentUserId, childUserId))";
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
        public List<UserHousehold> GetUserHouseholdByParentUserId(int parentUserId)
        {
            try
            {
                string cmd = $"SELECT data FROM app.{tableName} WHERE parentUserId = @parentUserId";
                var results = new List<UserHousehold>();
                using (var ctext = pgDataSource.CreateCommand(cmd))
                {
                    ctext.Parameters.AddWithValue("parentUserId", parentUserId);
                    using (NpgsqlDataReader reader = ctext.ExecuteReader())
                        while (reader.Read())
                        {
                            UserHousehold result = JsonSerializer.Deserialize<UserHousehold>(reader["data"] as string);
                            results.Add(result);
                        }
                }
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new List<UserHousehold>();
            }
        }
        public List<UserHousehold> GetUserHouseholdByChildUserId(int childUserId)
        {
            try
            {
                string cmd = $"SELECT data FROM app.{tableName} WHERE childUserId = @childUserId";
                var results = new List<UserHousehold>();
                using (var ctext = pgDataSource.CreateCommand(cmd))
                {
                    ctext.Parameters.AddWithValue("childUserId", childUserId);
                    using (NpgsqlDataReader reader = ctext.ExecuteReader())
                        while (reader.Read())
                        {
                            UserHousehold result = JsonSerializer.Deserialize<UserHousehold>(reader["data"] as string);
                            results.Add(result);
                        }
                }
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new List<UserHousehold>();
            }
        }
        public UserHousehold GetUserHouseholdByParentAndChildUserId(int parentUserId, int childUserId)
        {
            try
            {
                string cmd = $"SELECT data FROM app.{tableName} WHERE parentUserId = @parentUserId AND childUserId = @childUserId";
                UserHousehold result = null;
                using (var ctext = pgDataSource.CreateCommand(cmd))
                {
                    ctext.Parameters.AddWithValue("parentUserId", parentUserId);
                    ctext.Parameters.AddWithValue("childUserId", childUserId);
                    using (NpgsqlDataReader reader = ctext.ExecuteReader())
                        while (reader.Read())
                        {
                            result = JsonSerializer.Deserialize<UserHousehold>(reader["data"] as string);
                            return result;
                        }
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new UserHousehold();
            }
        }
        public bool SaveUserHousehold(UserHousehold userHousehold)
        {
            try
            {
                string cmd = $"INSERT INTO app.{tableName} (parentUserId, childUserId, data) VALUES(@parentUserId, @childUserId, CAST(@data AS jsonb)) ON CONFLICT(parentUserId, childUserId) DO UPDATE SET data = CAST(@data AS jsonb)";
                using (var ctext = pgDataSource.CreateCommand(cmd))
                {
                    var serializedData = JsonSerializer.Serialize(userHousehold);
                    ctext.Parameters.AddWithValue("parentUserId", userHousehold.Id.ParentUserId);
                    ctext.Parameters.AddWithValue("childUserId", userHousehold.Id.ChildUserId);
                    ctext.Parameters.AddWithValue("data", serializedData);
                    return ctext.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }
        public bool DeleteUserHousehold(int parentUserId, int childUserId)
        {
            try
            {
                string cmd = $"DELETE FROM app.{tableName} WHERE parentUserId = @parentUserId AND childUserId = @childUserId";
                using (var ctext = pgDataSource.CreateCommand(cmd))
                {
                    ctext.Parameters.AddWithValue("parentUserId", parentUserId);
                    ctext.Parameters.AddWithValue("childUserId", childUserId);
                    return ctext.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Delete all household records when a parent user is deleted.
        /// </summary>
        /// <param name="parentUserId"></param>
        /// <returns></returns>
        public bool DeleteAllHouseholdRecordsByParentUserId(int parentUserId)
        {
            try
            {
                string cmd = $"DELETE FROM app.{tableName} WHERE parentUserId = @parentUserId";
                using (var ctext = pgDataSource.CreateCommand(cmd))
                {
                    ctext.Parameters.AddWithValue("parentUserId", parentUserId);
                    ctext.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// Delete all household records when a child user is deleted.
        /// </summary>
        /// <param name="childUserId"></param>
        /// <returns></returns>
        public bool DeleteAllHouseholdRecordsByChildUserId(int childUserId)
        {
            try
            {
                string cmd = $"DELETE FROM app.{tableName} WHERE childUserId = @childUserId";
                using (var ctext = pgDataSource.CreateCommand(cmd))
                {
                    ctext.Parameters.AddWithValue("childUserId", childUserId);
                    ctext.ExecuteNonQuery();
                    return true;
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
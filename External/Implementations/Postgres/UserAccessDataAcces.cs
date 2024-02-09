using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using Npgsql;
using System.Net.Mail;

namespace CarCareTracker.External.Implementations
{
    public class PGUserAccessDataAccess : IUserAccessDataAccess
    {
        private NpgsqlConnection pgDataSource;
        private readonly ILogger<PGUserAccessDataAccess> _logger;
        private static string tableName = "useraccessrecords";
        public PGUserAccessDataAccess(IConfiguration config, ILogger<PGUserAccessDataAccess> logger)
        {
            pgDataSource = new NpgsqlConnection(config["POSTGRES_CONNECTION"]);
            _logger = logger;
            try
            {
                pgDataSource.Open();
                //create table if not exist.
                string initCMD = $"CREATE TABLE IF NOT EXISTS app.{tableName} (userId INT, vehicleId INT, PRIMARY KEY(userId, vehicleId))";
                using (var ctext = new NpgsqlCommand(initCMD, pgDataSource))
                {
                    ctext.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
        /// <summary>
        /// Gets a list of vehicles user have access to.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<UserAccess> GetUserAccessByUserId(int userId)
        {
            try
            {
                string cmd = $"SELECT userId, vehicleId FROM app.{tableName} WHERE userId = @userId";
                var results = new List<UserAccess>();
                using (var ctext = new NpgsqlCommand(cmd, pgDataSource))
                {
                    ctext.Parameters.AddWithValue("userId", userId);
                    using (NpgsqlDataReader reader = ctext.ExecuteReader())
                        while (reader.Read())
                        {
                            UserAccess result = new UserAccess()
                            {
                                Id = new UserVehicle
                                {
                                    UserId = int.Parse(reader["userId"].ToString()),
                                    VehicleId = int.Parse(reader["vehicleId"].ToString())
                                }
                            };
                            results.Add(result);
                        }
                }
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new List<UserAccess>();
            }
        }
        public UserAccess GetUserAccessByVehicleAndUserId(int userId, int vehicleId)
        {
            try
            {
                string cmd = $"SELECT userId, vehicleId FROM app.{tableName} WHERE userId = @userId AND vehicleId = @vehicleId";
                UserAccess result = null;
                using (var ctext = new NpgsqlCommand(cmd, pgDataSource))
                {
                    ctext.Parameters.AddWithValue("userId", userId);
                    ctext.Parameters.AddWithValue("vehicleId", vehicleId);
                    using (NpgsqlDataReader reader = ctext.ExecuteReader())
                        while (reader.Read())
                        {
                            result = new UserAccess()
                            {
                                Id = new UserVehicle
                                {
                                    UserId = int.Parse(reader["userId"].ToString()),
                                    VehicleId = int.Parse(reader["vehicleId"].ToString())
                                }
                            };
                            return result;
                        }
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new UserAccess();
            }
        }
        public List<UserAccess> GetUserAccessByVehicleId(int vehicleId)
        {
            try
            {
                string cmd = $"SELECT userId, vehicleId FROM app.{tableName} WHERE vehicleId = @vehicleId";
                var results = new List<UserAccess>();
                using (var ctext = new NpgsqlCommand(cmd, pgDataSource))
                {
                    ctext.Parameters.AddWithValue("vehicleId", vehicleId);
                    using (NpgsqlDataReader reader = ctext.ExecuteReader())
                        while (reader.Read())
                        {
                            UserAccess result = new UserAccess()
                            {
                                Id = new UserVehicle
                                {
                                    UserId = int.Parse(reader["userId"].ToString()),
                                    VehicleId = int.Parse(reader["vehicleId"].ToString())
                                }
                            };
                            results.Add(result);
                        }
                }
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new List<UserAccess>();
            }
        }
        public bool SaveUserAccess(UserAccess userAccess)
        {
            try
            {
                string cmd = $"INSERT INTO app.{tableName} (userId, vehicleId) VALUES(@userId, @vehicleId)";
                using (var ctext = new NpgsqlCommand(cmd, pgDataSource))
                {
                    ctext.Parameters.AddWithValue("userId", userAccess.Id.UserId);
                    ctext.Parameters.AddWithValue("vehicleId", userAccess.Id.VehicleId);
                    return ctext.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }
        public bool DeleteUserAccess(int userId, int vehicleId)
        {
            try
            {
                string cmd = $"DELETE FROM app.{tableName} WHERE userId = @userId AND vehicleId = @vehicleId";
                using (var ctext = new NpgsqlCommand(cmd, pgDataSource))
                {
                    ctext.Parameters.AddWithValue("userId", userId);
                    ctext.Parameters.AddWithValue("vehicleId", vehicleId);
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
        /// Delete all access records when a vehicle is deleted.
        /// </summary>
        /// <param name="vehicleId"></param>
        /// <returns></returns>
        public bool DeleteAllAccessRecordsByVehicleId(int vehicleId)
        {
            try
            {
                string cmd = $"DELETE FROM app.{tableName} WHERE vehicleId = @vehicleId";
                using (var ctext = new NpgsqlCommand(cmd, pgDataSource))
                {
                    ctext.Parameters.AddWithValue("vehicleId", vehicleId);
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
        /// Delee all access records when a user is deleted.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool DeleteAllAccessRecordsByUserId(int userId)
        {
            try
            {
                string cmd = $"DELETE FROM app.{tableName} WHERE userId = @userId";
                using (var ctext = new NpgsqlCommand(cmd, pgDataSource))
                {
                    ctext.Parameters.AddWithValue("userId", userId);
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
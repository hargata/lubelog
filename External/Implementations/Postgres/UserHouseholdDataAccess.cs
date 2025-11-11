using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using Npgsql;

namespace CarCareTracker.External.Implementations
{
    public class PGUserHouseholdDataAccess : IUserHouseholdDataAccess
    {
        private NpgsqlDataSource pgDataSource;
        private readonly ILogger<PGUserHouseholdDataAccess> _logger;
        private static string tableName = "userhouseholdrecords";
        public PGUserHouseholdDataAccess(IConfiguration config, ILogger<PGUserHouseholdDataAccess> logger)
        {
            pgDataSource = NpgsqlDataSource.Create(config["POSTGRES_CONNECTION"]);
            _logger = logger;
            try
            {
                //create table if not exist.
                string initCMD = $"CREATE SCHEMA IF NOT EXISTS app; CREATE TABLE IF NOT EXISTS app.{tableName} (parentUserId INT, childUserId INT, PRIMARY KEY(parentUserId, childUserId))";
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
                string cmd = $"SELECT parentUserId, childUserId FROM app.{tableName} WHERE parentUserId = @parentUserId";
                var results = new List<UserHousehold>();
                using (var ctext = pgDataSource.CreateCommand(cmd))
                {
                    ctext.Parameters.AddWithValue("parentUserId", parentUserId);
                    using (NpgsqlDataReader reader = ctext.ExecuteReader())
                        while (reader.Read())
                        {
                            UserHousehold result = new UserHousehold()
                            {
                                Id = new HouseholdAccess
                                {
                                    ParentUserId = int.Parse(reader["parentUserId"].ToString()),
                                    ChildUserId = int.Parse(reader["childUserId"].ToString())
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
                return new List<UserHousehold>();
            }
        }
        public List<UserHousehold> GetUserHouseholdByChildUserId(int childUserId)
        {
            try
            {
                string cmd = $"SELECT parentUserId, childUserId FROM app.{tableName} WHERE childUserId = @childUserId";
                var results = new List<UserHousehold>();
                using (var ctext = pgDataSource.CreateCommand(cmd))
                {
                    ctext.Parameters.AddWithValue("childUserId", childUserId);
                    using (NpgsqlDataReader reader = ctext.ExecuteReader())
                        while (reader.Read())
                        {
                            UserHousehold result = new UserHousehold()
                            {
                                Id = new HouseholdAccess
                                {
                                    ParentUserId = int.Parse(reader["parentUserId"].ToString()),
                                    ChildUserId = int.Parse(reader["childUserId"].ToString())
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
                return new List<UserHousehold>();
            }
        }
        public UserHousehold GetUserHouseholdByParentAndChildUserId(int parentUserId, int childUserId)
        {
            try
            {
                string cmd = $"SELECT parentUserId, childUserId FROM app.{tableName} WHERE parentUserId = @parentUserId AND childUserId = @childUserId";
                UserHousehold result = null;
                using (var ctext = pgDataSource.CreateCommand(cmd))
                {
                    ctext.Parameters.AddWithValue("parentUserId", parentUserId);
                    ctext.Parameters.AddWithValue("childUserId", childUserId);
                    using (NpgsqlDataReader reader = ctext.ExecuteReader())
                        while (reader.Read())
                        {
                            result = new UserHousehold()
                            {
                                Id = new HouseholdAccess
                                {
                                    ParentUserId = int.Parse(reader["parentUserId"].ToString()),
                                    ChildUserId = int.Parse(reader["childUserId"].ToString())
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
                return new UserHousehold();
            }
        }
        public bool SaveUserHousehold(UserHousehold userHousehold)
        {
            try
            {
                string cmd = $"INSERT INTO app.{tableName} (parentUserId, childUserId) VALUES(@parentUserId, @childUserId)";
                using (var ctext = pgDataSource.CreateCommand(cmd))
                {
                    ctext.Parameters.AddWithValue("parentUserId", userHousehold.Id.ParentUserId);
                    ctext.Parameters.AddWithValue("childUserId", userHousehold.Id.ChildUserId);
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
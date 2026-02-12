using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;
using CarCareTracker.Helper;

namespace CarCareTracker.External.Implementations
{
    public class ApiKeyRecordDataAccess : IApiKeyRecordDataAccess
    {
        private ILiteDBHelper _liteDB { get; set; }
        private static string tableName = "apikeyrecords";
        public ApiKeyRecordDataAccess(ILiteDBHelper liteDB)
        {
           _liteDB = liteDB;
        }
        public List<APIKey> GetAPIKeyRecordsByUserId(int userId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<APIKey>(tableName);
            var apiKeyRecords = table.Find(Query.EQ(nameof(APIKey.UserId), userId));
            return apiKeyRecords.ToList() ?? new List<APIKey>();
        }
        public APIKey GetAPIKeyById(int apiKeyId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<APIKey>(tableName);
            return table.FindById(apiKeyId);
        }
        public APIKey GetAPIKeyByKey(string hashedKey)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<APIKey>(tableName);
            var apiKeyRecord = table.FindOne(Query.EQ(nameof(APIKey.Key), hashedKey));
            return apiKeyRecord ?? new APIKey();
        }
        public bool SaveAPIKey(APIKey apiKey)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<APIKey>(tableName);
            table.Upsert(apiKey);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAPIKeyById(int apiKeyId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<APIKey>(tableName);
            table.Delete(apiKeyId);
            db.Checkpoint();
            return true;
        }
        public bool DeleteAllAPIKeysByUserId(int userId)
        {
            var db = _liteDB.GetLiteDB();
            var table = db.GetCollection<APIKey>(tableName);
            var apiKeyRecords = table.DeleteMany(Query.EQ(nameof(APIKey.UserId), userId));
            db.Checkpoint();
            return true;
        }
    }
}
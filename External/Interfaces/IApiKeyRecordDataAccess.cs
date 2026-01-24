using CarCareTracker.Models;

namespace CarCareTracker.External.Interfaces
{
    public interface IApiKeyRecordDataAccess
    {
        public List<APIKey> GetAPIKeyRecordsByUserId(int userId);
        public APIKey GetAPIKeyById(int apiKeyId);
        public APIKey GetAPIKeyByKey(string hashedKey);
        public bool SaveAPIKey(APIKey apiKey);
        public bool DeleteAPIKeyById(int apiKeyId);
        public bool DeleteAllAPIKeysByUserId(int userId);
    }
}
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace CarCareTracker.Helper
{
    public interface ITranslationHelper
    {
        string Translate(string userLanguage, string text);
    }
    public class TranslationHelper : ITranslationHelper
    {
        private readonly IFileHelper _fileHelper;
        private readonly IConfiguration _config;
        private IMemoryCache _cache;
        public TranslationHelper(IFileHelper fileHelper, IConfiguration config, IMemoryCache memoryCache)
        {
            _fileHelper = fileHelper;
            _config = config;
            _cache = memoryCache;
        }
        public string Translate(string userLanguage, string text)
        {
            bool create = bool.Parse(_config["LUBELOGGER_TRANSLATOR"] ?? "false");
            //transform input text into key.
            string translationKey = text.Replace(" ", "_");
            var translationFilePath = userLanguage == "en_US" ? _fileHelper.GetFullFilePath($"/defaults/en_US.json") : _fileHelper.GetFullFilePath($"/translations/{userLanguage}.json", false);
            var dictionary = _cache.GetOrCreate<Dictionary<string, string>>($"lang_{userLanguage}", entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(1);
                if (File.Exists(translationFilePath))
                {
                    try
                    {
                        var translationFile = File.ReadAllText(translationFilePath);
                        var translationDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(translationFile);
                        return translationDictionary ?? new Dictionary<string, string>();
                    } catch (Exception ex)
                    {
                        return new Dictionary<string, string>();
                    }
                }
                else
                {
                    return new Dictionary<string, string>();
                }
            });
            if (dictionary != null && dictionary.ContainsKey(translationKey))
            {
                return dictionary[translationKey];
            }
            else if (create && File.Exists(translationFilePath))
            {
                //create entry
                dictionary.Add(translationKey, text);
                File.WriteAllText(translationFilePath, JsonSerializer.Serialize(dictionary));
                return text;
            }
            return text;
        }
    }
}

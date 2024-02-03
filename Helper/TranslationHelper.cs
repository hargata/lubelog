using CarCareTracker.Models;
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
        public TranslationHelper(IFileHelper fileHelper, IConfiguration config)
        {
            _fileHelper = fileHelper;
            _config = config;
        }
        public string Translate(string userLanguage, string text)
        {
            bool create = bool.Parse(_config["LUBELOGGER_TRANSLATOR"] ?? "false");
            //transform input text into key.
            string translationKey = text.Replace(" ", "_");
            var translationFilePath = _fileHelper.GetFullFilePath($"/translations/{userLanguage}.json", false);
            if (File.Exists(translationFilePath))
            {
                var translationFile = File.ReadAllText(translationFilePath);
                var translationDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(translationFile);
                if (translationDictionary != null && translationDictionary.ContainsKey(translationKey))
                {
                    return translationDictionary[translationKey];
                } else if (create)
                {
                    //create entry
                    translationDictionary.Add(translationKey, text);
                    File.WriteAllText(translationFilePath, JsonSerializer.Serialize(translationDictionary));
                }
            }
            return create ? string.Empty : text;
        }
    }
}

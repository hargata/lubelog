using CarCareTracker.Models;
using System.IO.Compression;

namespace CarCareTracker.Helper
{
    public interface IFileHelper
    {
        string GetFullFilePath(string currentFilePath, bool mustExist = true);
        string MoveFileFromTemp(string currentFilePath, string newFolder);
        bool RenameFile(string currentFilePath, string newName);
        bool DeleteFile(string currentFilePath);
        string MakeBackup();
        bool RestoreBackup(string fileName, bool clearExisting = false);
        string MakeAttachmentsExport(List<GenericReportModel> exportData);
        List<string> GetLanguages();
        int ClearTempFolder();
        int ClearUnlinkedThumbnails(List<string> linkedImages);
        int ClearUnlinkedDocuments(List<string> linkedDocuments);
        string GetWidgets();
        bool WidgetsExist();
        bool SaveWidgets(string widgetsData);
        bool DeleteWidgets();
    }
    public class FileHelper : IFileHelper
    {
        private readonly IWebHostEnvironment _webEnv;
        private readonly ILogger<IFileHelper> _logger;
        private ILiteDBHelper _liteDB;
        public FileHelper(IWebHostEnvironment webEnv, ILogger<IFileHelper> logger, ILiteDBHelper liteDB)
        {
            _webEnv = webEnv;
            _logger = logger;
            _liteDB = liteDB;
        }
        public List<string> GetLanguages()
        {
            var languagePath = Path.Combine(_webEnv.ContentRootPath, "data", "translations");
            var defaultList = new List<string>() { "en_US" };
            if (Directory.Exists(languagePath))
            {
                var listOfLanguages = Directory.GetFiles(languagePath);
                if (listOfLanguages.Any())
                {
                    defaultList.AddRange(listOfLanguages.Select(x => Path.GetFileNameWithoutExtension(x)));
                }
            }
            return defaultList;
        }
        public bool RenameFile(string currentFilePath, string newName)
        {
            var fullFilePath = GetFullFilePath(currentFilePath);
            if (!string.IsNullOrWhiteSpace(fullFilePath))
            {
                try
                {
                    var originalFileName = Path.GetFileNameWithoutExtension(fullFilePath);
                    var newFilePath = fullFilePath.Replace(originalFileName, newName);
                    File.Move(fullFilePath, newFilePath);
                    return true;
                } 
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return false;
                }
            }
            return false;
        }
        public string GetFullFilePath(string currentFilePath, bool mustExist = true)
        {
            if (currentFilePath.StartsWith("/"))
            {
                currentFilePath = currentFilePath.Substring(1);
            }
            string oldFilePath = currentFilePath.StartsWith("defaults/") ? Path.Combine(_webEnv.WebRootPath, currentFilePath) : Path.Combine(_webEnv.ContentRootPath, "data", currentFilePath);
            if (File.Exists(oldFilePath))
            {
                return oldFilePath;
            }
            else if (!mustExist)
            {
                return oldFilePath;
            }
            {
                return string.Empty;
            }
        }
        public bool RestoreBackup(string fileName, bool clearExisting = false)
        {
            var fullFilePath = GetFullFilePath(fileName);
            if (string.IsNullOrWhiteSpace(fullFilePath))
            {
                return false;
            }
            try
            {
                var tempPath = Path.Combine(_webEnv.ContentRootPath, "data", $"temp/{Guid.NewGuid()}");
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);
                //extract zip file
                ZipFile.ExtractToDirectory(fullFilePath, tempPath);
                //copy over images and documents.
                var imagePath = Path.Combine(tempPath, "images");
                var documentPath = Path.Combine(tempPath, "documents");
                var translationPath = Path.Combine(tempPath, "translations");
                var dataPath = Path.Combine(tempPath, StaticHelper.DbName);
                var widgetPath = Path.Combine(tempPath, StaticHelper.AdditionalWidgetsPath);
                var configPath = Path.Combine(tempPath, StaticHelper.LegacyUserConfigPath);
                if (Directory.Exists(imagePath))
                {
                    var existingPath = Path.Combine(_webEnv.ContentRootPath, "data", "images");
                    if (!Directory.Exists(existingPath))
                    {
                        Directory.CreateDirectory(existingPath);
                    }
                    else if (clearExisting)
                    {
                        var filesToDelete = Directory.GetFiles(existingPath);
                        foreach (string file in filesToDelete)
                        {
                            File.Delete(file);
                        }
                    }
                    //copy each files from temp folder to newPath
                    var filesToUpload = Directory.GetFiles(imagePath);
                    foreach (string file in filesToUpload)
                    {
                        File.Copy(file, $"{existingPath}/{Path.GetFileName(file)}", true);
                    }
                }
                if (Directory.Exists(documentPath))
                {
                    var existingPath = Path.Combine(_webEnv.ContentRootPath, "data", "documents");
                    if (!Directory.Exists(existingPath))
                    {
                        Directory.CreateDirectory(existingPath);
                    }
                    else if (clearExisting)
                    {
                        var filesToDelete = Directory.GetFiles(existingPath);
                        foreach (string file in filesToDelete)
                        {
                            File.Delete(file);
                        }
                    }
                    //copy each files from temp folder to newPath
                    var filesToUpload = Directory.GetFiles(documentPath);
                    foreach (string file in filesToUpload)
                    {
                        File.Copy(file, $"{existingPath}/{Path.GetFileName(file)}", true);
                    }
                }
                if (Directory.Exists(translationPath))
                {
                    var existingPath = Path.Combine(_webEnv.ContentRootPath, "data", "translations");
                    if (!Directory.Exists(existingPath))
                    {
                        Directory.CreateDirectory(existingPath);
                    }
                    else if (clearExisting)
                    {
                        var filesToDelete = Directory.GetFiles(existingPath);
                        foreach (string file in filesToDelete)
                        {
                            File.Delete(file);
                        }
                    }
                    //copy each files from temp folder to newPath
                    var filesToUpload = Directory.GetFiles(translationPath);
                    foreach (string file in filesToUpload)
                    {
                        File.Copy(file, $"{existingPath}/{Path.GetFileName(file)}", true);
                    }
                }
                if (File.Exists(dataPath))
                {
                    //Relinquish current DB file lock
                    _liteDB.DisposeLiteDB();
                    //data path will always exist as it is created on startup if not.
                    File.Move(dataPath, StaticHelper.DbName, true);
                }
                if (File.Exists(widgetPath))
                {
                    File.Move(widgetPath, StaticHelper.AdditionalWidgetsPath, true);
                }
                if (File.Exists(configPath))
                {
                    //check if config folder exists.
                    if (!Directory.Exists("data/config"))
                    {
                        Directory.CreateDirectory("data/config");
                    }
                    File.Move(configPath, StaticHelper.UserConfigPath, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error Restoring Database Backup: {ex.Message}");
                return false;
            }
        }
        public string MakeAttachmentsExport(List<GenericReportModel> exportData)
        {
            var folderName = Guid.NewGuid();
            var tempPath = Path.Combine(_webEnv.ContentRootPath, "data", $"temp/{folderName}");
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);
            int fileIndex = 0;
            foreach (GenericReportModel reportModel in exportData)
            {
                foreach (UploadedFiles file in reportModel.Files)
                {
                    var fileToCopy = GetFullFilePath(file.Location);
                    var destFileName = $"{tempPath}/{fileIndex}_{reportModel.DataType}_{reportModel.Date.ToString("yyyy-MM-dd")}_{file.Name}{Path.GetExtension(file.Location)}";
                    File.Copy(fileToCopy, destFileName);
                    fileIndex++;
                }
            }
            var destFilePath = $"{tempPath}.zip";
            ZipFile.CreateFromDirectory(tempPath, destFilePath);
            //delete temp directory
            Directory.Delete(tempPath, true);
            var zipFileName = $"/temp/{folderName}.zip";
            return zipFileName;
        }
        public string MakeBackup()
        {
            var folderName = $"db_backup_{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}";
            var tempPath = Path.Combine(_webEnv.ContentRootPath, "data", $"temp/{folderName}");
            var imagePath = Path.Combine(_webEnv.ContentRootPath, "data", "images");
            var documentPath = Path.Combine(_webEnv.ContentRootPath, "data", "documents");
            var translationPath = Path.Combine(_webEnv.ContentRootPath, "data", "translations");
            var dataPath = StaticHelper.DbName;
            var widgetPath = StaticHelper.AdditionalWidgetsPath;
            var configPath = StaticHelper.UserConfigPath;
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);
            if (Directory.Exists(imagePath))
            {
                var files = Directory.GetFiles(imagePath);
                foreach (var file in files)
                {
                    var newPath = Path.Combine(tempPath, "images");
                    Directory.CreateDirectory(newPath);
                    File.Copy(file, $"{newPath}/{Path.GetFileName(file)}");
                }
            }
            if (Directory.Exists(documentPath))
            {
                var files = Directory.GetFiles(documentPath);
                foreach (var file in files)
                {
                    var newPath = Path.Combine(tempPath, "documents");
                    Directory.CreateDirectory(newPath);
                    File.Copy(file, $"{newPath}/{Path.GetFileName(file)}");
                }
            }
            if (Directory.Exists(translationPath))
            {
                var files = Directory.GetFiles(translationPath);
                foreach(var file in files)
                {
                    var newPath = Path.Combine(tempPath, "translations");
                    Directory.CreateDirectory(newPath);
                    File.Copy(file, $"{newPath}/{Path.GetFileName(file)}");
                }
            }
            if (File.Exists(dataPath))
            {
                var newPath = Path.Combine(tempPath, "data");
                Directory.CreateDirectory(newPath);
                File.Copy(dataPath, $"{newPath}/{Path.GetFileName(dataPath)}");
            }
            if (File.Exists(widgetPath))
            {
                var newPath = Path.Combine(tempPath, "data");
                Directory.CreateDirectory(newPath);
                File.Copy(widgetPath, $"{newPath}/{Path.GetFileName(widgetPath)}");
            }
            if (File.Exists(configPath))
            {
                var newPath = Path.Combine(tempPath, "config");
                Directory.CreateDirectory(newPath);
                File.Copy(configPath, $"{newPath}/{Path.GetFileName(configPath)}");
            }
            var destFilePath = $"{tempPath}.zip";
            ZipFile.CreateFromDirectory(tempPath, destFilePath);
            //delete temp directory
            Directory.Delete(tempPath, true);
            return $"/temp/{folderName}.zip";
        }
        public string MoveFileFromTemp(string currentFilePath, string newFolder)
        {
            string tempPath = "temp/";
            if (string.IsNullOrWhiteSpace(currentFilePath) || !currentFilePath.StartsWith("/temp/")) //file is not in temp directory.
            {
                return currentFilePath;
            }
            if (currentFilePath.StartsWith("/"))
            {
                currentFilePath = currentFilePath.Substring(1);
            }
            string uploadPath = Path.Combine(_webEnv.ContentRootPath, "data", newFolder);
            string oldFilePath = Path.Combine(_webEnv.ContentRootPath, "data", currentFilePath);
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);
            string newFileUploadPath = oldFilePath.Replace(tempPath, newFolder);
            if (File.Exists(oldFilePath))
            {
                File.Move(oldFilePath, newFileUploadPath);
            }
            string newFilePathToReturn = "/" + currentFilePath.Replace(tempPath, newFolder);
            return newFilePathToReturn;
        }
        public bool DeleteFile(string currentFilePath)
        {
            if (currentFilePath.StartsWith("/"))
            {
                currentFilePath = currentFilePath.Substring(1);
            }
            string filePath = Path.Combine(_webEnv.ContentRootPath, "data", currentFilePath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            if (!File.Exists(filePath)) //verify file no longer exists.
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public int ClearTempFolder()
        {
            int filesDeleted = 0;
            var tempPath = GetFullFilePath("temp", false);
            if (Directory.Exists(tempPath))
            {
                //delete files
                var files = Directory.GetFiles(tempPath);
                foreach (var file in files)
                {
                    File.Delete(file);
                    filesDeleted++;
                }
                //delete folders
                var folders = Directory.GetDirectories(tempPath);
                foreach(var folder in folders)
                {
                    Directory.Delete(folder, true);
                    filesDeleted++;
                }
            }
            return filesDeleted;
        }
        public int ClearUnlinkedThumbnails(List<string> linkedImages)
        {
            int filesDeleted = 0;
            var imagePath = GetFullFilePath("images", false);
            if (Directory.Exists(imagePath))
            {
                var files = Directory.GetFiles(imagePath);
                foreach(var file in files)
                {
                    if (!linkedImages.Contains(Path.GetFileName(file)))
                    {
                        File.Delete(file);
                        filesDeleted++;
                    }
                }
            }
            return filesDeleted;
        }
        public int ClearUnlinkedDocuments(List<string> linkedDocuments)
        {
            int filesDeleted = 0;
            var documentPath = GetFullFilePath("documents", false);
            if (Directory.Exists(documentPath))
            {
                var files = Directory.GetFiles(documentPath);
                foreach (var file in files)
                {
                    if (!linkedDocuments.Contains(Path.GetFileName(file)))
                    {
                        File.Delete(file);
                        filesDeleted++;
                    }
                }
            }
            return filesDeleted;
        }
        public string GetWidgets()
        {
            if (File.Exists(StaticHelper.AdditionalWidgetsPath))
            {
                try
                {
                    //read file
                    var widgets = File.ReadAllText(StaticHelper.AdditionalWidgetsPath);
                    return widgets;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    return string.Empty;
                }
            }
            return string.Empty;
        }
        public bool WidgetsExist()
        {
            return File.Exists(StaticHelper.AdditionalWidgetsPath);
        }
        public bool SaveWidgets(string widgetsData)
        {
            try
            {
                //Delete Widgets if exists
                DeleteWidgets();
                File.WriteAllText(StaticHelper.AdditionalWidgetsPath, widgetsData);
                return true;
            } catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            } 
        }
        public bool DeleteWidgets()
        {
            try
            {
                if (File.Exists(StaticHelper.AdditionalWidgetsPath))
                {
                    File.Delete(StaticHelper.AdditionalWidgetsPath);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }
    }
}

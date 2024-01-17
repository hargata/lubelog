using System.IO.Compression;

namespace CarCareTracker.Helper
{
    public interface IFileHelper
    {
        string GetFullFilePath(string currentFilePath, bool mustExist = true);
        string MoveFileFromTemp(string currentFilePath, string newFolder);
        bool DeleteFile(string currentFilePath);
        string MakeBackup();
        bool RestoreBackup(string fileName);
    }
    public class FileHelper : IFileHelper
    {
        private readonly IWebHostEnvironment _webEnv;
        private readonly ILogger<IFileHelper> _logger;
        public FileHelper(IWebHostEnvironment webEnv, ILogger<IFileHelper> logger)
        {
            _webEnv = webEnv;
            _logger = logger;
        }
        public string GetFullFilePath(string currentFilePath, bool mustExist = true)
        {
            if (currentFilePath.StartsWith("/"))
            {
                currentFilePath = currentFilePath.Substring(1);
            }
            string oldFilePath = Path.Combine(_webEnv.WebRootPath, currentFilePath);
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
        public bool RestoreBackup(string fileName)
        {
            var fullFilePath = GetFullFilePath(fileName);
            if (string.IsNullOrWhiteSpace(fullFilePath))
            {
                return false;
            }
            try
            {
                var tempPath = Path.Combine(_webEnv.WebRootPath, $"temp/{Guid.NewGuid()}");
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);
                //extract zip file
                ZipFile.ExtractToDirectory(fullFilePath, tempPath);
                //copy over images and documents.
                var imagePath = Path.Combine(tempPath, "images");
                var documentPath = Path.Combine(tempPath, "documents");
                var dataPath = Path.Combine(tempPath, StaticHelper.DbName);
                var configPath = Path.Combine(tempPath, StaticHelper.UserConfigPath);
                if (Directory.Exists(imagePath))
                {
                    var existingPath = Path.Combine(_webEnv.WebRootPath, "images");
                    if (!Directory.Exists(existingPath))
                    {
                        Directory.CreateDirectory(existingPath);
                    }
                    //copy each files from temp folder to newPath
                    var filesToUpload = Directory.GetFiles(imagePath);
                    foreach(string file in filesToUpload)
                    {
                        File.Copy(file, $"{existingPath}/{Path.GetFileName(file)}");
                    }
                }
                if (Directory.Exists(documentPath))
                {
                    var existingPath = Path.Combine(_webEnv.WebRootPath, "documents");
                    if (!Directory.Exists(existingPath))
                    {
                        Directory.CreateDirectory(existingPath);
                    }
                    //copy each files from temp folder to newPath
                    var filesToUpload = Directory.GetFiles(documentPath);
                    foreach (string file in filesToUpload)
                    {
                        File.Copy(file, $"{existingPath}/{Path.GetFileName(file)}");
                    }
                }
                if (File.Exists(dataPath))
                {
                    //data path will always exist as it is created on startup if not.
                    File.Move(dataPath, StaticHelper.DbName, true);
                }
                if (File.Exists(configPath))
                {
                    //check if config folder exists.
                    if (!Directory.Exists("config/"))
                    {
                        Directory.CreateDirectory("config/");
                    }
                    File.Move(configPath, StaticHelper.UserConfigPath, true);
                }
                return true;
            } catch (Exception ex)
            {
                _logger.LogError(ex, $"Error Restoring Database Backup: {ex.Message}");
                return false;
            }
        }
        public string MakeBackup()
        {
            var folderName = $"db_backup_{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}";
            var tempPath = Path.Combine(_webEnv.WebRootPath, $"temp/{folderName}");
            var imagePath = Path.Combine(_webEnv.WebRootPath, "images");
            var documentPath = Path.Combine(_webEnv.WebRootPath, "documents");
            var dataPath = StaticHelper.DbName;
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
            if (File.Exists(dataPath))
            {
                var newPath = Path.Combine(tempPath, "data");
                Directory.CreateDirectory(newPath);
                File.Copy(dataPath, $"{newPath}/{Path.GetFileName(dataPath)}");
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
            string uploadPath = Path.Combine(_webEnv.WebRootPath, newFolder);
            string oldFilePath = Path.Combine(_webEnv.WebRootPath, currentFilePath);
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
            string filePath = Path.Combine(_webEnv.WebRootPath, currentFilePath);
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
    }
}

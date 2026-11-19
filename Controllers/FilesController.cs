using CarCareTracker.Helper;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CarCareTracker.Controllers
{
    [Authorize]
    public class FilesController : Controller
    {
        private readonly ILogger<FilesController> _logger;
        private readonly IWebHostEnvironment _webEnv;
        private readonly IFileHelper _fileHelper;
        private readonly IConfigHelper _config;
        private readonly FileExtensionContentTypeProvider _mimeTypeProvider;

        public FilesController(ILogger<FilesController> logger, IFileHelper fileHelper, IConfigHelper config, IWebHostEnvironment webEnv)
        {
            _logger = logger;
            _webEnv = webEnv;
            _fileHelper = fileHelper;
            _config = config;
            _mimeTypeProvider = new FileExtensionContentTypeProvider();
        }

        [HttpPost]
        public IActionResult HandleFileUpload(IFormFile file)
        {
            var fileName = UploadFile(file);
            return Json(fileName);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpPost]
        public IActionResult HandleTranslationFileUpload(IFormFile file)
        {
            var originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
            if (originalFileName == "en_US")
            {
                return Json(OperationResponse.Failed("The translation file name en_US is reserved."));
            }
            var fileName = UploadFile(file);
            //move file from temp to translation folder.
            var uploadedFilePath = _fileHelper.MoveFileFromTemp(fileName, "translations/");
            //rename uploaded file so that it preserves original name.
            if (!string.IsNullOrWhiteSpace(uploadedFilePath))
            {
                var result = _fileHelper.RenameFile(uploadedFilePath, originalFileName);
                return Json(OperationResponse.Conditional(result));
            }
            return Json(OperationResponse.Failed());
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpPost]
        public IActionResult HandleThemeFileUpload(IFormFile file)
        {
            //check if extension is css
            if (Path.GetExtension(file.FileName) != ".css")
            {
                return Json(OperationResponse.Failed());
            }
            var originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
            var fileName = UploadFile(file);
            //move file from temp to themes folder.
            var uploadedFilePath = _fileHelper.MoveFileFromTemp(fileName, "themes/");
            //rename uploaded file so that it preserves original name.
            if (!string.IsNullOrWhiteSpace(uploadedFilePath))
            {
                var result = _fileHelper.RenameFile(uploadedFilePath, originalFileName);
                return Json(OperationResponse.Conditional(result));
            }
            return Json(OperationResponse.Failed());
        }

        [HttpPost]
        public IActionResult HandleMultipleFileUpload(List<IFormFile> file)
        {
            List<UploadedFiles> uploadedFiles = new List<UploadedFiles>();
            string allowedFileExtensions = _config.GetAllowedFileUploadExtensions();
            List<string> allowedFileExtensionsArray = new List<string>();
            if (!string.IsNullOrWhiteSpace(allowedFileExtensions) && allowedFileExtensions.Trim() != "*") {
                allowedFileExtensionsArray.AddRange(allowedFileExtensions.Split(",").Select(x => x.Trim()));
            }
            foreach (IFormFile fileToUpload in file)
            {
                //sanitize against list of allowed extensions
                if (allowedFileExtensionsArray.Any() && !allowedFileExtensions.Contains(Path.GetExtension(fileToUpload.FileName)))
                {
                    _logger.LogWarning($"User tried to upload attachment with unapproved extension {Path.GetExtension(fileToUpload.FileName)}");
                    continue;
                }
                var fileName = UploadFile(fileToUpload);
                uploadedFiles.Add(new UploadedFiles { Name = fileToUpload.FileName, Location = fileName, IsPending = true});
            }
            return Json(uploadedFiles);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpPost]
        public IActionResult DeleteFiles(string fileLocation)
        {
            var result = _fileHelper.DeleteFile(fileLocation);
            return Json(result);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpGet]
        public IActionResult MakeBackup()
        {
            var result = _fileHelper.MakeBackup();
            return Json(result);
        }
        [Authorize(Roles = nameof(UserData.IsRootUser))]
        [HttpPost]
        public IActionResult RestoreBackup(string fileName)
        {
            var result = _fileHelper.RestoreBackup(fileName);
            return Json(result);
        }
        private string UploadFile(IFormFile fileToUpload)
        {
            string uploadDirectory = "temp/";
            string uploadPath = Path.Combine(_webEnv.ContentRootPath, "data", uploadDirectory);
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);
            string fileName = Guid.NewGuid() + Path.GetExtension(fileToUpload.FileName);
            string filePath = Path.Combine(uploadPath, fileName);
            using (var stream = System.IO.File.Create(filePath))
            {
                fileToUpload.CopyTo(stream);
            }
            return Path.Combine("/", uploadDirectory, fileName);
        }
        public IActionResult UploadCoordinates(List<string> coordinates)
        {
            string uploadDirectory = "temp/";
            string uploadPath = Path.Combine(_webEnv.ContentRootPath, "data", uploadDirectory);
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);
            string fileName = Guid.NewGuid() + ".csv";
            string filePath = Path.Combine(uploadPath, fileName);
            string fileData = string.Join("\r\n", coordinates);
            System.IO.File.WriteAllText(filePath, fileData);
            var uploadedFile = new UploadedFiles { Name = "coordinates.csv", Location = Path.Combine("/", uploadDirectory, fileName), IsPending = true };
            return Json(uploadedFile);
        }
        public IActionResult PreviewFile(string fileName, string fileLocation)
        {
            var viewModel = new UploadedFiles { Name = fileName, Location = fileLocation };
            return PartialView("_AttachmentPreview", viewModel);
        }
        [Route("/images/{fileName}")]
        [Route("/documents/{fileName}")]
        [Route("/translations/{fileName}")]
        [Route("/temp/{fileName}")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> GetStaticFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return NotFound();
            }
            var fullFilePath = _fileHelper.GetFullFilePath(Request.Path);
            if (!string.IsNullOrWhiteSpace(fullFilePath))
            {
                var fileBytes = await _fileHelper.GetFileBytesAsync(fullFilePath);
                if (_mimeTypeProvider.TryGetContentType(fileName, out string? contentType))
                {
                    return File(fileBytes, contentType);
                }
                return File(fileBytes, "application/octet-stream");
            }
            return NotFound();
        }
    }
}
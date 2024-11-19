using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;
using CarCareTracker.Helper;
using Microsoft.AspNetCore.Authorization;

namespace CarCareTracker.Controllers
{
    [Authorize]
    public class FilesController : Controller
    {
        private readonly ILogger<FilesController> _logger;
        private readonly IWebHostEnvironment _webEnv;
        private readonly IFileHelper _fileHelper;

        public FilesController(ILogger<FilesController> logger, IFileHelper fileHelper, IWebHostEnvironment webEnv)
        {
            _logger = logger;
            _webEnv = webEnv;
            _fileHelper = fileHelper;
        }

        [HttpPost]
        public IActionResult HandleFileUpload(IFormFile file)
        {
            var fileName = UploadFile(file);
            return Json(fileName);
        }

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
                return Json(new OperationResponse { Success = result, Message = string.Empty });
            }
            return Json(OperationResponse.Failed());
        }

        [HttpPost]
        public IActionResult HandleMultipleFileUpload(List<IFormFile> file)
        {
            List<UploadedFiles> uploadedFiles = new List<UploadedFiles>();
            foreach (IFormFile fileToUpload in file)
            {
                var fileName = UploadFile(fileToUpload);
                uploadedFiles.Add(new UploadedFiles { Name = fileToUpload.FileName, Location = fileName});
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
            string uploadPath = Path.Combine(_webEnv.WebRootPath, uploadDirectory);
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
            string uploadPath = Path.Combine(_webEnv.WebRootPath, uploadDirectory);
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);
            string fileName = Guid.NewGuid() + ".csv";
            string filePath = Path.Combine(uploadPath, fileName);
            string fileData = string.Join("\r\n", coordinates);
            System.IO.File.WriteAllText(filePath, fileData);
            var uploadedFile = new UploadedFiles { Name = "coordinates.csv", Location = Path.Combine("/", uploadDirectory, fileName) };
            return Json(uploadedFile);
        }
    }
}

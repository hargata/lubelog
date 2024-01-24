using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
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
    }
}

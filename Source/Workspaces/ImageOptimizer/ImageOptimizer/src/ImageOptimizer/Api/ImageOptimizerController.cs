using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ImageOptimizer.Data;
using ImageOptimizer.Models;
using ImageOptimizer.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace ImageOptimizer.Api
{
    [Route("api/[controller]")]
    public class ImageOptimizerController : Controller
    {
        private const string TempFolderName = "temp";

        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserManagementService _userManagementService;
        private readonly Services.ImageOptimizer _imageOptimizer;

        public ImageOptimizerController(
            IHostingEnvironment hostingEnvironment,
            ApplicationDbContext applicationDbContext,
            UserManager<ApplicationUser> userManager,
            UserManagementService userManagementService)
        {
            _hostingEnvironment = hostingEnvironment;
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
            _userManagementService = userManagementService;
            _imageOptimizer = new Services.ImageOptimizer(_hostingEnvironment);
        }

        // POST api/values
        /*[HttpPost]
        public async Task<ActionResult> PostJson([FromBody] ImageViewModel imageServiceModel)
        {
            var image = Convert.FromBase64String(imageServiceModel.Image);
            System.IO.File.WriteAllBytes("D:\\image.jpg", image);
            return new ObjectResult(imageServiceModel);
        }*/

        // POST api/values
        [HttpPost(Name = "PostMultipartFormData")]
        public async Task<ActionResult> PostMultipartFormData(ImageApiModel imageResult)
        {
            if (!Request.HasFormContentType)
            {
                imageResult.Message = "The request doesn't contain multipart/form-data.";
                return BadRequest(imageResult);
            }

            if (!ModelState.IsValid)
            {
                imageResult.Message = "Bad request.";
                return BadRequest(imageResult);
            }

            var userApiKey = await _applicationDbContext.ApiKeys.FirstOrDefaultAsync(x => x.Key == imageResult.ApiKey);
            if (userApiKey == null)
            {
                imageResult.Message = "Wrong API Key.";
                return BadRequest(imageResult);
            }
            
            var userPermissions = await _userManagementService.GetUserPermissionsByIdAsync(userApiKey.ApplicationUserId);
            imageResult.OptimizationLevel = userPermissions.OptimizationLevel;

            // Check monthly optimization limit
            var userMonthlyOptimizedImages = await _userManagementService.GetUserMonthlyOptimizationsByIdAsync(userApiKey.ApplicationUserId);
            if (userMonthlyOptimizedImages.MonthlyOptimizedImages >= userPermissions.ImageLimitPerMonth)
            {
                imageResult.Message = "You have reached your monthly optimization limit.";
                return BadRequest(imageResult);
            }

            // Generate temp upload folder path
            var guid = Guid.NewGuid().ToString();
            var tempUploadFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, TempFolderName, guid);
            var isTempUploadFolderCreated = false;

            // Iterate files
            var processedFiles = new List<ImageApiModel>();
            foreach (var file in Request.Form.Files)
            {
                imageResult.Name = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                imageResult.FileType = file.ContentType;
                imageResult.FileExtension = Path.GetExtension(imageResult.Name);
                imageResult.OriginalSize = file.Length;

                // Check file type
                if (!userPermissions.AllowedFileTypes.Contains(file.ContentType))
                {
                    imageResult.Message = "Not supported file format!";
                    processedFiles.Add(imageResult);
                    continue;
                }

                // Check file size
                if (file.Length >= userPermissions.AllowedImageSize || file.Length <= 0)
                {
                    imageResult.Message = $"The file size limit is {new FormattingService().AsReadableSize(userPermissions.AllowedImageSize)}";
                    processedFiles.Add(imageResult);
                    continue;
                }

                // Create temp directory and save the file
                isTempUploadFolderCreated = true;
                await SaveImageFile(tempUploadFolderPath, imageResult, file);

                // Process image
                _imageOptimizer.OptimizeImage(imageResult);
                ConvertImagesToBase64String(imageResult);

                imageResult.Message = "High five! :)";
                imageResult.Succeeded = true;
                processedFiles.Add(imageResult);

                // Saving information to database
                await UpdateDatabase(userApiKey.ApplicationUserId, imageResult, userMonthlyOptimizedImages);
            }

            // Delete the temporary upload folder and the image file
            if (isTempUploadFolderCreated)
                Directory.Delete(tempUploadFolderPath, true);

            return new ObjectResult(processedFiles);
        }

        #region Helpers
        private static async Task SaveImageFile(string tempUploadFolderPath, ImageApiModel imageResult, IFormFile file)
        {
            Directory.CreateDirectory(tempUploadFolderPath);
            imageResult.FilePath = Path.Combine(tempUploadFolderPath, imageResult.Name);
            using (var fileStream = new FileStream(imageResult.FilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            {
                await file.CopyToAsync(fileStream);
                fileStream.Close();
            }
        }

        private static void ConvertImagesToBase64String(ImageApiModel imageResult)
        {
            imageResult.Image = Convert.ToBase64String(System.IO.File.ReadAllBytes(imageResult.FilePath));

            if (imageResult.ConvertedImages == null)
                return;

            foreach (var convertedImage in imageResult.ConvertedImages)
            {
                convertedImage.Image = Convert.ToBase64String(System.IO.File.ReadAllBytes(convertedImage.FilePath));
            }
        }

        private async Task UpdateDatabase(string applicationUserId, ImageApiModel imageResult, UserMonthlyOptimization userMonthlyOptimizedImages)
        {
            var image = new Image(applicationUserId, imageResult);
            _applicationDbContext.Images.Add(image);

            userMonthlyOptimizedImages.MonthlyOptimizedImages++;
            if (userMonthlyOptimizedImages.MonthlyOptimizedImages == 1)
            {
                // Add new monthly data
                _applicationDbContext.UserMonthlyOptimizations.Add(userMonthlyOptimizedImages);
            }
            else
            {
                // Update monthly data
                _applicationDbContext.UserMonthlyOptimizations.Attach(userMonthlyOptimizedImages);
                var entry = _applicationDbContext.Entry(userMonthlyOptimizedImages);
                entry.Property(x => x.MonthlyOptimizedImages).IsModified = true;
            }

            await _applicationDbContext.SaveChangesAsync();
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _applicationDbContext?.Dispose();
                _userManager?.Dispose();
                _userManagementService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

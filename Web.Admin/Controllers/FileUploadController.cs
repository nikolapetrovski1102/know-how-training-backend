using Microsoft.AspNetCore.Mvc;

namespace Web.Admin.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadController> _logger;
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10 MB
        private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
        private readonly string[] _allowedDocExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };

        public FileUploadController(IWebHostEnvironment environment, ILogger<FileUploadController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        [HttpPost("image")]
        [RequestSizeLimit(10_485_760)] // 10 MB
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "No file uploaded" });

                if (file.Length > _maxFileSize)
                    return BadRequest(new { error = $"File size exceeds {_maxFileSize / 1024 / 1024} MB" });

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedImageExtensions.Contains(extension))
                    return BadRequest(new { error = $"Invalid file type: {extension}" });

                // Create uploads/images folder if it doesn't exist
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "images");
                Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var fileUrl = $"/uploads/images/{uniqueFileName}";
                _logger.LogInformation("Image uploaded: {FileName} -> {FileUrl}", file.FileName, fileUrl);

                return Ok(new
                {
                    success = true,
                    fileUrl = fileUrl,
                    fileName = uniqueFileName,
                    originalFileName = file.FileName,
                    fileSize = file.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, new { error = "Failed to upload image" });
            }
        }

        [HttpPost("document")]
        public async Task<IActionResult> UploadDocument(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "No file uploaded" });

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedDocExtensions.Contains(extension))
                    return BadRequest(new { error = $"Invalid file type: {extension}" });

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "documents");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var fileUrl = $"/uploads/documents/{uniqueFileName}";

                return Ok(new
                {
                    success = true,
                    fileUrl = fileUrl,
                    fileName = uniqueFileName,
                    originalFileName = file.FileName,
                    fileType = extension.TrimStart('.').ToUpper()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                return StatusCode(500, new { error = "Failed to upload document" });
            }
        }

        [HttpDelete]
        public IActionResult DeleteFile([FromQuery] string fileUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileUrl) || !fileUrl.StartsWith("/uploads/"))
                    return BadRequest(new { error = "Invalid file URL" });

                var filePath = Path.Combine(_environment.WebRootPath,
                    fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { error = "File not found" });

                System.IO.File.Delete(filePath);
                _logger.LogInformation("File deleted: {FileUrl}", fileUrl);

                return Ok(new { success = true, message = "File deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file");
                return StatusCode(500, new { error = "Failed to delete file" });
            }
        }
    }
}

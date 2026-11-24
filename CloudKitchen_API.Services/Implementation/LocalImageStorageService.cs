using CloudKitchen_API.Services.Dto;
using CloudKitchen_API.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CloudKitchen_API.Services.Implementation
{
    public class LocalImageStorageService : IImageStorageService
    {
        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        private const long MaxBytes = 5 * 1024 * 1024;   // 5 MB

        private readonly string _uploadsRoot;
        private readonly string _publicBaseUrl;
        private readonly ILogger<LocalImageStorageService> _log;

        public LocalImageStorageService(
            IConfiguration config,
            IHostEnvironment env,
            ILogger<LocalImageStorageService> log)
        {
            var relative = config["Storage:UploadsRoot"] ?? "wwwroot/uploads";
            _uploadsRoot = Path.Combine(env.ContentRootPath, relative);
            _publicBaseUrl = (config["Storage:PublicBaseUrl"] ?? "").TrimEnd('/');
            _log = log;

            Directory.CreateDirectory(_uploadsRoot);
        }

        public async Task<string> SaveAsync(Stream fileStream, string originalFileName, string contentType, string folder)
        {
            if (fileStream == null || fileStream.Length == 0)
                throw new ApiException("File is empty.");
            if (fileStream.Length > MaxBytes)
                throw new ApiException("File exceeds the 5 MB limit.");

            var ext = Path.GetExtension(originalFileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                throw new ApiException("Only jpg, jpeg, png, webp, gif are allowed.");

            if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new ApiException("File is not an image.");

            var safeFolder = folder.Trim().ToLowerInvariant();
            if (safeFolder is not ("menu" or "category" or "restaurant"))
                throw new ApiException("Invalid folder.");

            var targetDir = Path.Combine(_uploadsRoot, safeFolder);
            Directory.CreateDirectory(targetDir);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(targetDir, fileName);

            await using (var fs = File.Create(fullPath))
            {
                await fileStream.CopyToAsync(fs);
            }

            _log.LogInformation("Saved image to {Path}", fullPath);

            return $"{_publicBaseUrl}/uploads/{safeFolder}/{fileName}";
        }

        public Task DeleteAsync(string publicUrl)
        {
            if (string.IsNullOrWhiteSpace(publicUrl)) return Task.CompletedTask;

            // Only delete files this service owns — never delete external URLs
            var marker = "/uploads/";
            var idx = publicUrl.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return Task.CompletedTask;

            var relative = publicUrl[(idx + marker.Length)..];
            var fullPath = Path.Combine(_uploadsRoot, relative.Replace('/', Path.DirectorySeparatorChar));

            try
            {
                if (File.Exists(fullPath)) File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Could not delete image {Url}", publicUrl);
            }

            return Task.CompletedTask;
        }
    }
}

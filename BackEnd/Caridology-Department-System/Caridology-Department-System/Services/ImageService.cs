using System.IO;
using System.Threading.Tasks;

namespace Caridology_Department_System.Services
{
    public interface IImageService
    {
        Task<string> SaveImageAsync(IFormFile imageStream);
        string GetImageUrl(string imagePath);
        string GetImageBase64(string imagePath);
        bool DeleteImage(string imagePath);
    }

    public class ImageService : IImageService
    {
        public async Task<string> SaveImageAsync(IFormFile imageStream)
        {
            try
            {
                // Validate file extension
                var extension = Path.GetExtension(imageStream.FileName).ToLower();
                if (!IsValidImageExtension(extension))
                    throw new ArgumentException("Invalid image format");

                // Create upload directory if it doesn't exist
                var uploadPath = Path.Combine("wwwroot", "uploads");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadPath, uniqueFileName);

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageStream.CopyToAsync(fileStream);
                }

                return Path.Combine(uploadPath, uniqueFileName).Replace("\\", "/");
            }
            catch(Exception ex)
            {
                throw new Exception("Image upload failed", ex);
            }
        }

        public string GetImageUrl(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return null;

            var fullPath = Path.Combine("wwwroot", imagePath.TrimStart('/'));
            return File.Exists(fullPath) ? $"/{imagePath.TrimStart('/')}" : null;
        }

        public string GetImageBase64(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return null;

            var fullPath = Path.Combine("wwwroot", imagePath.TrimStart('/'));
            if (!File.Exists(fullPath))
                return null;

            var imageBytes = File.ReadAllBytes(fullPath);
            var contentType = GetContentType(fullPath);
            return $"data:{contentType};base64,{Convert.ToBase64String(imageBytes)}";
        }
        public bool DeleteImage(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return false;

            var fullPath = Path.Combine("wwwroot", imagePath.TrimStart('/'));
            if (!File.Exists(fullPath))
                return false;

            File.Delete(fullPath);
            return true;
        }

        private bool IsValidImageExtension(string extension)
        {
            string[] validExtensions = { ".jpg", ".jpeg", ".png"};
            return validExtensions.Contains(extension.ToLower());
        }

        private string GetContentType(string path)
        {
            return Path.GetExtension(path).ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }
    }
}
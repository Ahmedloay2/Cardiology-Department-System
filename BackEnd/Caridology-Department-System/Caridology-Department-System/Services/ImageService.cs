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
        /// <summary>
        /// Saves an uploaded image to a unique path under the "wwwroot/uploads" directory.
        /// </summary>
        /// <param name="imageStream">The uploaded image file to save.</param>
        /// <returns>The relative path to the saved image file.</returns>
        /// <exception cref="ArgumentException">Thrown if the image file extension is not valid.</exception>
        /// <exception cref="Exception">Thrown if the image could not be saved.</exception>
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
        /// <summary>
        /// Gets the public URL of the image if it exists on disk.
        /// </summary>
        /// <param name="imagePath">The relative image path.</param>
        /// <returns>The image URL as a string, or <c>null</c> if the file does not exist.</returns>
        public string GetImageUrl(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return null;

            var fullPath = Path.Combine("wwwroot", imagePath.TrimStart('/'));
            return File.Exists(fullPath) ? $"/{imagePath.TrimStart('/')}" : null;
        }
        /// <summary>
        /// Converts an image file to a Base64 data URL suitable for embedding in HTML.
        /// </summary>
        /// <param name="imagePath">The relative path to the image file.</param>
        /// <returns>The Base64-encoded image string, or <c>null</c> if the file is not found.</returns>
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
        /// <summary>
        /// Deletes an image file from disk.
        /// </summary>
        /// <param name="imagePath">The relative path to the image to delete.</param>
        /// <returns><c>true</c> if the image was deleted; otherwise, <c>false</c>.</returns>
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
        /// <summary>
        /// Checks whether the given file extension is a supported image format.
        /// </summary>
        /// <param name="extension">The file extension to check (including the dot, e.g., ".jpg").</param>
        /// <returns><c>true</c> if the extension is valid; otherwise, <c>false</c>.</returns>
        private bool IsValidImageExtension(string extension)
        {
            string[] validExtensions = { ".jpg", ".jpeg", ".png"};
            return validExtensions.Contains(extension.ToLower());
        }
        /// <summary>
        /// Gets the MIME content type based on the image file extension.
        /// </summary>
        /// <param name="path">The image file path.</param>
        /// <returns>The content type string.</returns>
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
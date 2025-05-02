using System.ComponentModel.DataAnnotations;

namespace Caridology_Department_System.ValdiationAttributes
{
    public class AllowedImageExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions = { ".jpg", ".jpeg", ".png" };

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (string.IsNullOrEmpty(extension) || !_extensions.Contains(extension))
                {
                    return new ValidationResult($"Allowed extensions: {string.Join(", ", _extensions)}");
                }

            }

            return ValidationResult.Success;
        }
    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Caridology_Department_System.ValdiationAttributes;

namespace Caridology_Department_System.Requests
{
    public class AdminRequest
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be 2-50 characters")]
        public string FName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be 2-50 characters")]
        public string LName { get; set; }

        [Required(ErrorMessage = "Birth date is required")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,100}$",
            ErrorMessage = "Password must contain 8-100 characters with at least one uppercase, lowercase, number, and special character")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(
    @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
    ErrorMessage = "Please enter a valid email address"
)]
        public string Email { get; set; }

        [Required(ErrorMessage = "At least one phone number is required")]
        [PhoneListValidation]
        public List<string> PhoneNumbers { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }
        [DataType(DataType.Upload)]
        [AllowedImageExtensions]
        public IFormFile? Photo { get; set; }
        [Required]
        public string Gender { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using Caridology_Department_System.ValdiationAttributes;

namespace Caridology_Department_System.Requests.Doctor
{
    public class DoctorRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LName { get; set; }
        [Required]
        public DateTime BirthDate { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
    ErrorMessage = "Password must contain at least one uppercase, one lowercase, one number and one special character")]
        public string Password { get; set; }

        [Required]
        [StringLength(100)]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
        [Required]
        [StringLength(50)]
        public string Position { get; set; }
        [Range(1, 50, ErrorMessage = "Experience must be 1-50 years")]
        public int YearsOfExperience { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required(ErrorMessage = "At least one phone number is required")]
        [PhoneListValidation]
        public List<string> PhoneNumbers { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }
        [DataType(DataType.Upload)]
        [AllowedImageExtensions]
        public IFormFile? Photo { get; set; }
        [Required]
        [Range(6000, 50000, ErrorMessage = "Salary must be lower than 50k and higher than 6k")]
        public float Salary { get; set; }
    }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace Caridology_Department_System.Requests.Doctor
{
    public class DoctorUpdateRequest
    {       
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string? FName { get; set; }

        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string? LName { get; set; }
        public DateTime? BirthDate { get; set; }
        [StringLength(100)]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
        [StringLength(50)]
        public string? Position { get; set; }
        [Range(1, 50, ErrorMessage = "Experience must be 1-50 years")]
        public int? YearsOfExperience { get; set; }
        public string? Gender { get; set; }
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }
        [DefaultValue(null)]
        [SwaggerSchema(
        Description = "List of admin phone numbers",
        Nullable = true
        )]
        [PhoneListValidation]
        public List<string>? PhoneNumbers { get; set; }
        [DefaultValue(null)]
        public IFormFile? PhotoData { get; set; }
        [Range(6000, 50000, ErrorMessage = "Salary must be lower than 50k and higher than 6k")]
        public float? Salary { get; set; }
    }
}

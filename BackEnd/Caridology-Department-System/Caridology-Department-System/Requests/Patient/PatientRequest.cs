using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Caridology_Department_System.ValdiationAttributes;
using Swashbuckle.AspNetCore.Annotations;

public class PatientRequest
{
    // Personal Information
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2-50 characters")]
    [DefaultValue("")]
    public string FName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2-50 characters")]
    [DefaultValue("")]
    public string LName { get; set; }

    [Required(ErrorMessage = "Birth date is required")]
    public DateTime BirthDate { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    [DefaultValue("")]
    public string Gender { get; set; }

    // Authentication
    [DefaultValue("")]
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
        ErrorMessage = "Password must contain uppercase, lowercase, number and special character")]
    public string Password { get; set; }
    [DefaultValue("")]
    [Required(ErrorMessage = "Email is required")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [DataType(DataType.EmailAddress)]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    // Contact Information
    [DefaultValue("")]
    [Required(ErrorMessage = "Address is required")]
    public string Address { get; set; }

    [Phone(ErrorMessage = "Invalid landline number")]
    [DefaultValue("")]
    public string? LandLine { get; set; }

    // Emergency Contacts
    [Required(ErrorMessage = "Emergency contact name is required")]
    [StringLength(100, ErrorMessage = "Emergency contact name cannot exceed 100 characters")]
    [DefaultValue("")]
    public string EmergencyContactName { get; set; }

    [Required(ErrorMessage = "Emergency contact phone is required")]
    [RegularExpression("^(?:\\+20|0)?1[0125]\\d{8}$",
        ErrorMessage = "Invalid Egyptian phone number format")]
    [DefaultValue("")]
    public string EmergencyContactPhone { get; set; }

    // Phone Numbers
    [Required(ErrorMessage = "At least one phone number is required")]
    [PhoneListValidation(ErrorMessage = "Invalid phone number in list")]
    public List<string> PhoneNumbers { get; set; }

    // Family Information
    [Required(ErrorMessage = "Parent name is required")]
    [DefaultValue("")]
    public string ParentName { get; set; }
    [DefaultValue("")]
    public string? SpouseName { get; set; }

    // Medical Information (Optional)
    [DefaultValue("")]
    public string? BloodType { get; set; }
    [DefaultValue("")]
    public string? Allergies { get; set; }
    [DefaultValue("")]
    public string? ChronicConditions { get; set; }
    [DefaultValue("")]
    public string? PreviousSurgeries { get; set; }
    [DefaultValue("")]
    public string? CurrentMedications { get; set; }

    // Insurance Information (Optional)
    [DefaultValue("")]
    public string? PolicyNumber { get; set; }
    public string? InsuranceProvider { get; set; }
    [DefaultValue("")]
    public DateTime? PolicyValidDate { get; set; }

    [Required(ErrorMessage = "Link is required")]
    [DefaultValue("")]
    public string Link { get; set; }
    [DataType(DataType.Upload)]
    [AllowedImageExtensions]
    public IFormFile? Photo { get; set; }
}
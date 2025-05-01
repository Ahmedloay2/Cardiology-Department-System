using System.ComponentModel.DataAnnotations;

public class PatientRequest
{
    // Personal Information
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2-50 characters")]
    public string FName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2-50 characters")]
    public string LName { get; set; }

    [Required(ErrorMessage = "Birth date is required")]
    public DateTime BirthDate { get; set; }

    [Required(ErrorMessage = "Gender is required")]
    public string Gender { get; set; }

    // Authentication
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
        ErrorMessage = "Password must contain uppercase, lowercase, number and special character")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [DataType(DataType.EmailAddress)]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    // Contact Information
    [Required(ErrorMessage = "Address is required")]
    public string Address { get; set; }

    [Phone(ErrorMessage = "Invalid landline number")]
    public string? LandLine { get; set; }

    // Emergency Contacts
    [Required(ErrorMessage = "Emergency contact name is required")]
    [StringLength(100, ErrorMessage = "Emergency contact name cannot exceed 100 characters")]
    public string EmergencyContactName { get; set; }

    [Required(ErrorMessage = "Emergency contact phone is required")]
    [RegularExpression("^(?:\\+20|0)?1[0125]\\d{8}$",
        ErrorMessage = "Invalid Egyptian phone number format")]
    public string EmergencyContactPhone { get; set; }

    // Phone Numbers
    [Required(ErrorMessage = "At least one phone number is required")]
    [PhoneListValidation(ErrorMessage = "Invalid phone number in list")]
    public List<string> PhoneNumbers { get; set; }

    // Family Information
    [Required(ErrorMessage = "Parent name is required")]
    public string ParentName { get; set; }

    public string? SpouseName { get; set; }

    // Medical Information (Optional)
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? PreviousSurgeries { get; set; }
    public string? CurrentMedications { get; set; }

    // Insurance Information (Optional)
    public string? PolicyNumber { get; set; }
    public string? InsuranceProvider { get; set; }
    public DateTime? PolicyValidDate { get; set; }

    // System/Additional Fields
    public string? PhotoPath { get; set; }

    [Required(ErrorMessage = "Link is required")]
    public string Link { get; set; }
}
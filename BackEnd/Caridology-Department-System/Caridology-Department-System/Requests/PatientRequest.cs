using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class PatientRequest
{
    public PatientRequest() { }

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
    [RegularExpression(
    @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
    ErrorMessage = "Please enter a valid email address"
)]
    public string Email { get; set; }

    [StringLength(255)]
    public string? PhotoPath { get; set; }

    [StringLength(5)]
    [RegularExpression(@"^(A|B|AB|O)[+-]$",
         ErrorMessage = "Invalid blood type format (e.g., A+, O-)")]
    public string? BloodType { get; set; }

    //[StringLength(255)]
    //[Column(TypeName = "text")]
    //public string? MedicalHistory { get; set; }

    //[StringLength(255)]
    //[Column(TypeName = "text")]
    //public string? HealthInsuranceNumber { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string EmergencyContactName { get; set; }

    [Required]
    [RegularExpression("^(?:\\+20|0)?1[0125]\\d{8}$", ErrorMessage = "Invalid phone number format.")]
    public string EmergencyContactPhone { get; set; }

    [Required]
    [PhoneListValidation]
    public List<string> PhoneNumbers { get; set; }

    // Add these required fields to match PatientModel
    [Required]
    public string Gender { get; set; }

    [Required]
    public string Link { get; set; }

    [Required]
    public string ParentName { get; set; }

    [Required]
    public string Address { get; set; }

    // Optional fields for completeness (add if needed)
    public string? SpouseName { get; set; }
    public string? LandLine { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? PreviousSurgeries { get; set; }
    public string? CurrentMedications { get; set; }
    public string? PolicyNumber { get; set; } = string.Empty;
    public string? insuranceProvider { get; set; } = string.Empty;
    public DateTime? PolicyValidDate
    {get; set;}
    }

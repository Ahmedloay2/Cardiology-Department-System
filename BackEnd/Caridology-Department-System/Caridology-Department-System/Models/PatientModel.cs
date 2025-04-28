using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Caridology_Department_System.Models
{
    [Table("Patients")]
    public class PatientModel : UserModel
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LName { get; set; }

        [NotMapped]
        public string FullName => $"{FName} {LName}";
        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        [ForeignKey(nameof(Role))]
        public int RoleID { get; set; }
        public RoleModel Role { get; set; }

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

        [Required]
        [ForeignKey(nameof(Status))]
        public int StatusID { get; set; }
        public StatusModel Status { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        [StringLength(5)]
        [RegularExpression(@"^(A|B|AB|O)[+-]$",
             ErrorMessage = "Invalid blood type format (e.g., A+, O-)")]
        public string? BloodType { get; set; }
        //[StringLength(255)]
        //[Column(TypeName = "text")]
        //public string? MedicalHistory { get; set; }
        [StringLength(255)]
        [Column(TypeName = "text")]
        //public string? HealthInsuranceNumber { get; set; }  
        public ICollection<AppointmentModel> Appointments { get; set; } = new List<AppointmentModel>();
        public ICollection<MessageModel> Messages { get; set; } = new List<MessageModel>();
        public ICollection<ReportModel> Reports { get; set; } = new List<ReportModel>(); 
        public ICollection<PatientPhoneNumberModel> PhoneNumbers { get; set; } = new List<PatientPhoneNumberModel>();
        [StringLength(100)]
        public string? EmergencyContactName { get; set; }

        [StringLength(15)]
        [Phone]
        public string? EmergencyContactPhone { get; set; }
        [Required]
        public string Gender { get; set; } = string.Empty;
        [Required]
        public string Link { get; set; } = string.Empty; [Required]
        public string ParentName { get; set; } = string.Empty;
        public string? SpouseName { get; set; } = string.Empty;
        public string? LandLine { get; set; } = string.Empty;
        [Required]
        public string Address { get; set; } = string.Empty;
        public string? Allergies { get; set; } = string.Empty;
        public string? ChronicConditions { get; set; } = string.Empty;
        public string? PreviousSurgeries { get; set; } = string.Empty;
        public string? CurrentMedications { get; set; } = string.Empty;
        public string? PolicyNumber { get; set; } = string.Empty;
        public string? insuranceProvider { get; set; } = string.Empty;
        public DateTime ?PolicyValidDate { get; set; }
    }
}
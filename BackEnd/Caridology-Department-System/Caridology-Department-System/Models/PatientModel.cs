using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Caridology_Department_System.Models
{
    [Table("Patients")]
    public class PatientModel : UserModel
    {
        [Key]
        public int ID { get; set; }

        // Personal Information
        [Required, StringLength(50, MinimumLength = 2)]
        public string FName { get; set; }

        [Required, StringLength(50, MinimumLength = 2)]
        public string LName { get; set; }

        [NotMapped]
        public string FullName => $"{FName} {LName}";

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public string Gender { get; set; }

        // Authentication & Security
        [Required, StringLength(100)]
        public string Password { get; set; }

        [Required, StringLength(100), DataType(DataType.EmailAddress)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
        public string Email { get; set; }

        // Contact Information
        [Required]
        public string Address { get; set; }

        [StringLength(15)]
        public string? LandLine { get; set; }

        // Medical Information
        [StringLength(5)]
        [RegularExpression(@"^(A|B|AB|O)[+-]$")]
        public string? BloodType { get; set; }

        [Column(TypeName = "text")]
        public string? Allergies { get; set; }

        [Column(TypeName = "text")]
        public string? ChronicConditions { get; set; }

        [Column(TypeName = "text")]
        public string? PreviousSurgeries { get; set; }

        [Column(TypeName = "text")]
        public string? CurrentMedications { get; set; }

        // Emergency Contacts
        [StringLength(100)]
        public string? EmergencyContactName { get; set; }

        [StringLength(15), Phone]
        public string? EmergencyContactPhone { get; set; }

        // Family Information
        [Required]
        public string ParentName { get; set; }

        public string? SpouseName { get; set; }

        // Insurance Information
        public string? PolicyNumber { get; set; }

        public string? InsuranceProvider { get; set; }

        public DateTime? PolicyValidDate { get; set; }

        // System Fields
        [Required, ForeignKey(nameof(Role))]
        public int RoleID { get; set; }
        public RoleModel Role { get; set; }

        [Required, ForeignKey(nameof(Status))]
        public int StatusID { get; set; }
        public StatusModel Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public ICollection<AppointmentModel> Appointments { get; set; } = new List<AppointmentModel>();
        public ICollection<MessageModel> Messages { get; set; } = new List<MessageModel>();
        public ICollection<ReportModel> Reports { get; set; } = new List<ReportModel>();
        public ICollection<PatientPhoneNumberModel> PhoneNumbers { get; set; } = new List<PatientPhoneNumberModel>();

        // Optional Fields
        [StringLength(255)]
        public string? PhotoPath { get; set; }

        [Required]
        public string Link { get; set; }
    }
}
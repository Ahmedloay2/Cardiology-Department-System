using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Caridology_Department_System.Models
{
    [Table("Doctors")]
    public class DoctorModel : UserModel
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
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            ErrorMessage = "Invalid email format")]
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
        [Required]
        [StringLength(50)]
        public string Position { get; set; }
        public ICollection<AppointmentModel> Appointments { get; set; } = new List<AppointmentModel>();
        public ICollection<MessageModel> Messages { get; set; } = new List<MessageModel>();
        public ICollection<ReportModel> AuthoredReports { get; set; } = new List<ReportModel>();
        public ICollection<DoctorPhoneNumberModel> PhoneNumbers { get; set; }
        = new List<DoctorPhoneNumberModel>();
        [Range(1, 50, ErrorMessage = "Experience must be 1-50 years")]
        public int YearsOfExperience { get; set; }

    }
}

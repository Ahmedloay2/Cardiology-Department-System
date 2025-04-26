using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Caridology_Department_System.Models
{
    public class PatientModel : UserModel
    {
        [StringLength(5)]
        [RegularExpression(@"^(A|B|AB|O)[+-]$",
             ErrorMessage = "Invalid blood type format (e.g., A+, O-)")]
        public string? BloodType { get; set; }
        [StringLength(255)]
        [Column(TypeName = "nvarchar(255)")]
        public string? HealthInsuranceNumber { get; set; }  
        public ICollection<AppointmentModel> Appointments { get; set; } = new List<AppointmentModel>();
        public ICollection<MessageModel> Messages { get; set; } = new List<MessageModel>();
        public ICollection<ReportModel> Reports { get; set; } = new List<ReportModel>(); 
        public ICollection<PatientPhoneNumberModel> PhoneNumbers { get; set; } = new List<PatientPhoneNumberModel>();
        [StringLength(100)]
        public string? EmergencyContactName { get; set; }

        [StringLength(15)]
        [Phone]
        public string? EmergencyContactPhone { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace Caridology_Department_System.Models
{
    public class DoctorModel : UserModel
    {
        [Required]
        [StringLength(50)]
        public string Position { get; set; }
        public ICollection<AppointmentModel> Appointments { get; set; } = new List<AppointmentModel>();
        public ICollection<MessageModel> Messages { get; set; } = new List<MessageModel>();
        public ICollection<ReportModel> AuthoredReports { get; set; } = new List<ReportModel>();
        [Range(1, 50, ErrorMessage = "Experience must be 1-50 years")]
        public int YearsOfExperience { get; set; }

    }
}

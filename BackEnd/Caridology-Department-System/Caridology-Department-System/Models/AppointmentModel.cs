using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Caridology_Department_System.Models
{
    public class AppointmentModel
    {
        [Key]   
        public int APPID { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        [ForeignKey(nameof(Doctor))]
        public int DoctorID { get; set; }
        public DoctorModel Doctor { get; set; }
        [Required]
        [ForeignKey(nameof(Patient))]
        public int PatientID { get; set; }
        public PatientModel Patient { get; set; }
        [Required]
        [ForeignKey(nameof(Status))]
        public int StatusID { get; set; }
        public StatusModel Status { get; set; }
    }
}

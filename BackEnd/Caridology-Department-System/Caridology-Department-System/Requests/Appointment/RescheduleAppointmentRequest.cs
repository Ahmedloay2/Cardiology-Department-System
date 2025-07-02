using Caridology_Department_System.ValdiationAttributes;
using System.ComponentModel.DataAnnotations;

namespace Caridology_Department_System.Requests.Appointment
{
    public class RescheduleAppointmentRequest
    {
        [Required(ErrorMessage = "Appointment date is required.")]
        [ValidAppointmentDate]
        public DateTime AppDate { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Doctor ID must be a valid positive number.")]
        public int DoctorID { get; set; }
        [Required(ErrorMessage = "Appointment date is required.")]
        [ValidAppointmentDate]
        public DateTime NewDate { get; set; }
    }
}

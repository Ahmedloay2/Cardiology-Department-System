using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Caridology_Department_System.Models
{
    public class DoctorPhoneNumberModel : PhoneNumberModel
    {
        [ForeignKey(nameof(Doctor))]
        public int DoctorID { get; set; }
        public DoctorModel Doctor { get; set; }
    }
}

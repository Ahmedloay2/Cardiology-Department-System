using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Caridology_Department_System.Models
{
    public class PatientPhoneNumberModel: PhoneNumberModel
    {
        [ForeignKey("Patient")]
        public int PatientID { get; set; }
        public PatientModel Patient { get; set; } 

    }
}

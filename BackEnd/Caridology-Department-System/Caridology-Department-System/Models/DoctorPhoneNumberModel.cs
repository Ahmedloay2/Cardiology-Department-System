using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Caridology_Department_System.Models
{
    [Table("DoctorPhoneNumbers")]
    public class DoctorPhoneNumberModel : PhoneNumberModel
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression(@"^\+?[0-9\s\-\(\)]{5,20}$",
            ErrorMessage = "Invalid phone number format")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Required]
        [ForeignKey(nameof(Status))]
        public int StatusID { get; set; }
        public StatusModel Status { get; set; }
        [ForeignKey(nameof(Doctor))]
        public int DoctorID { get; set; }
        public DoctorModel Doctor { get; set; }
    }
}

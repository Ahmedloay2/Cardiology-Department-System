using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Caridology_Department_System.Models
{
    [Table("AdminPhoneNumbers")]
    public class AdminPhoneNumberModel: PhoneNumberModel
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
        [ForeignKey(nameof(Admin))]
        public int AdminID { get; set; }
        public AdminModel Admin {  get; set; }
    }
}

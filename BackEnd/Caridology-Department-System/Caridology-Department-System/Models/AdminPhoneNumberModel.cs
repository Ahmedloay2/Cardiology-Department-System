using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Caridology_Department_System.Models
{
    public class AdminPhoneNumberModel: PhoneNumberModel
    {
        [ForeignKey(nameof(Admin))]
        public int AdminID { get; set; }
        public AdminModel Admin {  get; set; }
    }
}

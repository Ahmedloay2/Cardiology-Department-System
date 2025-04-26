namespace Caridology_Department_System.Models
{
    public class AdminModel : UserModel
    {
        public ICollection<AdminPhoneNumberModel> PhoneNumbers { get; set; }
           = new List<AdminPhoneNumberModel>();
    }
}

using Caridology_Department_System.Models;

namespace Caridology_Department_System.Services
{
    public class AdminPhoneNumberSL
    {
        DBContext DBContext { get; set; }
        public AdminPhoneNumberSL()
        {
            DBContext = new DBContext();
        }
        public void AddPhoneNumbers(List<string> PhoneNumbers,int AdminID)
        {
            if (PhoneNumbers == null || !PhoneNumbers.Any())
                throw new ArgumentException("Phone numbers list is empty.");
            List<AdminPhoneNumberModel> adminPhones = PhoneNumbers.Select(Phone => new AdminPhoneNumberModel
            {
                PhoneNumber = Phone,
                ID = AdminID,
                StatusID=1
            }).ToList();
            DBContext.AdminPhoneNumbers.AddRange(adminPhones);
        }

    }
}

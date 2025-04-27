using Caridology_Department_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Caridology_Department_System.Services
{
    public class PatientPhoneNumberSL
    {
        private readonly DBContext dbContext;
        public PatientPhoneNumberSL()
        {
            dbContext = new DBContext();
        }
        public void AddPhoneNumbers(List<string> phoneNumbers, int patientId)
        {
            if (phoneNumbers == null || !phoneNumbers.Any())
                throw new ArgumentException("Phone numbers list is empty.");

            List<PatientPhoneNumberModel> entities = phoneNumbers.Select(phone => new PatientPhoneNumberModel
            {
                PhoneNumber = phone,
                PatientID = patientId,
                StatusID = 1 
            }).ToList();
            dbContext.PatientPhoneNumbers.AddRange(entities);
        }
    }

}

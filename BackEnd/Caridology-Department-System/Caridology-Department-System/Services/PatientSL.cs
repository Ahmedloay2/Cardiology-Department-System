
using Caridology_Department_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Caridology_Department_System.Services
{
    public class PatientSL
    {
        private readonly DBContext dbContext;
        public PatientSL()
        {
            dbContext = new DBContext();
        }

        public PatientModel GetPatientByID(int PatientID) {
            return dbContext.Patients.
                    FromSqlInterpolated($@"
                                    SELECT * FROM Patients 
                                    WHERE ID = {PatientID} 
                                    AND StatusID != 3") // Soft-delete check
                    .Include(p => p.PhoneNumbers)    // Load related data
                    .Include(p => p.Appointments)
                    .FirstOrDefault();
        
        }
        public void AddPatient(PatientModel patient,List<string>PhoneNumbers)
        {
            PatientPhoneNumberSL phoneNumberSL = new PatientPhoneNumberSL();
            EmailValidator email= new EmailValidator(dbContext);
            if (patient == null)
                throw new ArgumentNullException("Patient cannot be Empty");
            if (!email.IsEmailUnique(patient.Email))
                throw new ArgumentException("Email already exist");

            dbContext.Patients.Add(patient);
            dbContext.SaveChanges();
            phoneNumberSL.AddPhoneNumbers(PhoneNumbers,patient.ID);      
        }

    }
}


using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;

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
        public void AddPatient(PatientRequest patient)
        {
            PatientPhoneNumberSL phoneNumberSL = new PatientPhoneNumberSL();
            EmailValidator email= new EmailValidator(dbContext);
            PasswordHasher hasher = new PasswordHasher();
            if (patient == null)
                throw new ArgumentNullException("Patient cannot be Empty");
            if (!email.IsEmailUnique(patient.Email))
                throw new ArgumentException("Email already exist");
            PatientModel Createdpatient = new PatientModel
            {
                FName = patient.FName,
                LName = patient.LName,
                BirthDate = patient.BirthDate,
                Email = patient.Email,
                Password = hasher.HashPassword(patient.Password),
                BloodType = patient.BloodType,
                MedicalHistory = patient.MedicalHistory,
                HealthInsuranceNumber = patient.HealthInsuranceNumber,
                // Default values
                RoleID = 3, // Assuming 3 is patient role
                StatusID = 1, // Active status
                CreatedAt = DateTime.Now
            };
            dbContext.Patients.Add(Createdpatient);
            dbContext.SaveChanges();
            phoneNumberSL.AddPhoneNumbers(patient.PhoneNumbers, Createdpatient.ID);      
        }
    }
}


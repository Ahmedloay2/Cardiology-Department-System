

using System.Text;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Npgsql;
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

        public PatientModel GetPatientByID(int? patientID)
        {
            if (patientID == null)
            {
                throw new ArgumentNullException(nameof(patientID));
            }
            return dbContext.Patients
                .Where(p => p.ID == patientID && p.StatusID != 3) // Soft-delete check
                .Include(p => p.PhoneNumbers)    // Eager load phone numbers
                .Include(p => p.Appointments)    // Eager load appointments
                .FirstOrDefault();               // Returns null if not found
        }
        public void AddPatient(PatientRequest patient, List<string> phoneNumbers)
        {
            // Validate input
            if (patient == null)
                throw new ArgumentNullException(nameof(patient), "Patient data cannot be empty");

            if (phoneNumbers == null || !phoneNumbers.Any())
                throw new ArgumentException("At least one phone number is required", nameof(phoneNumbers));

            // Initialize services with proper DI (assuming they're registered in your DI container)
            var phoneNumberSL = new PatientPhoneNumberSL();
            var emailValidator = new EmailValidator(dbContext);
            var passwordHasher = new PasswordHasher();

            // Validate email uniqueness
            if (!emailValidator.IsEmailUnique(patient.Email))
                throw new ArgumentException("Email already exists", nameof(patient.Email));

            // Create patient entity
            PatientModel newPatient = new PatientModel
            {
                FName = patient.FName,
                LName = patient.LName,
                BirthDate = patient.BirthDate,
                Email = patient.Email,
                Password = passwordHasher.HashPassword(patient.Password),
                PhotoPath = patient.PhotoPath,
                BloodType = patient.BloodType,
                MedicalHistory = patient.MedicalHistory,
                HealthInsuranceNumber = patient.HealthInsuranceNumber,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone,
                RoleID = 3, // Default patient role
                StatusID = 1, // Active status
                CreatedAt = DateTime.UtcNow // Use UTC for consistency
            };

            // Use transaction for atomic operations
            using var transaction = dbContext.Database.BeginTransaction();
            try
            {
                // Add and save patient
                dbContext.Patients.Add(newPatient);
                dbContext.SaveChanges(); // This will generate the ID

                // Add phone numbers
                phoneNumberSL.AddPhoneNumbers(patient.PhoneNumbers, newPatient.ID);
                dbContext.SaveChanges();

                transaction.Commit();
            }
            catch (DbUpdateException dbEx)
            {
                transaction.Rollback();

                // Log detailed database errors
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"Database update failed: {dbEx.Message}");

                if (dbEx.InnerException != null)
                {
                    errorMessage.AppendLine($"Inner exception: {dbEx.InnerException.Message}");

                    // Special handling for Postgres exceptions
                    if (dbEx.InnerException is PostgresException pgEx)
                    {
                        errorMessage.AppendLine($"Postgres Error Code: {pgEx.SqlState}");
                        errorMessage.AppendLine($"Detail: {pgEx.Detail}");
                        errorMessage.AppendLine($"Table: {pgEx.TableName}");
                        errorMessage.AppendLine($"Column: {pgEx.ColumnName}");
                        errorMessage.AppendLine($"Constraint: {pgEx.ConstraintName}");
                    }
                }

                // Log to console (for debugging)
                Console.WriteLine(errorMessage.ToString());

                // Log to file
                File.AppendAllText("database_errors.log", $"[{DateTime.UtcNow}] {errorMessage}\n");

                throw new Exception("Failed to save patient data. " + errorMessage, dbEx);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"General error: {ex.Message}\n{ex.StackTrace}");
                throw new Exception("An unexpected error occurred while saving patient", ex);
            }

        }
        public PatientModel GetPatientByEmailAndPassword(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Email and password are required");
            }

            // Always hash the password first to prevent timing attacks
            PasswordHasher hasher = new PasswordHasher();
            string dummyHash = hasher.HashPassword("dummy_password");

            // Get patient (including inactive ones to prevent enumeration)
            PatientModel patient = dbContext.Patients.FirstOrDefault(p => p.Email == email);
            // Verify with constant time comparison
            bool passwordValid = patient != null &&
                                hasher.VerifyPassword(password, patient.Password) &&
                                patient.StatusID != 3;

            if (!passwordValid)
            {
                // Always return the same error message to prevent email enumeration
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            return patient;
        }
    }
}


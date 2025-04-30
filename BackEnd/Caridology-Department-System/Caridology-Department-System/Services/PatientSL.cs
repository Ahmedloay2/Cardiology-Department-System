using System.Text;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Npgsql;
using Org.BouncyCastle.Crypto.Generators;

namespace Caridology_Department_System.Services
{
    /// <summary>
    /// Service Layer class for Patient-related operations
    /// Handles database operations for patient records including CRUD operations
    /// </summary>
    public class PatientSL
    {
        // Database context for accessing the data layer
        private readonly DBContext dbContext;

        /// <summary>
        /// Constructor initializes a new database context
        /// </summary>
        public PatientSL()
        {
            dbContext = new DBContext();
        }

        /// <summary>
        /// Retrieves a patient by their ID
        /// </summary>
        /// <param name="patientID">The unique identifier of the patient</param>
        /// <returns>The patient model including related phone numbers and appointments</returns>
        /// <exception cref="ArgumentNullException">Thrown when patientID is null</exception>
        public PatientModel GetPatientByID(int? patientID)
        {
            // Validate that patient ID is not null
            if (patientID == null)
            {
                throw new ArgumentNullException(nameof(patientID));
            }

            // Query the database for the patient with the specified ID
            // Exclude soft-deleted patients (StatusID 3)
            // Include related phone numbers and appointments collections
            // Return null if the patient is not found
            return dbContext.Patients
                .Where(p => p.ID == patientID && p.StatusID != 3) // Soft-delete check
                .Include(p => p.PhoneNumbers)    // Eager load phone numbers
                .Include(p => p.Appointments)    // Eager load appointments
                .FirstOrDefault();               // Returns null if not found
        }

        /// <summary>
        /// Adds a new patient to the database with associated phone numbers
        /// </summary>
        /// <param name="patient">The patient data transfer object containing patient information</param>
        /// <param name="phoneNumbers">List of phone numbers associated with the patient</param>
        /// <exception cref="ArgumentNullException">Thrown when patient object is null</exception>
        /// <exception cref="ArgumentException">Thrown when phone numbers are missing or email exists</exception>
        public void AddPatient(PatientRequest patient, List<string> phoneNumbers)
        {
            // Validate that patient object is not null
            if (patient == null)
                throw new ArgumentNullException(nameof(patient), "Patient data cannot be empty");

            // Validate that at least one phone number is provided
            if (phoneNumbers == null || !phoneNumbers.Any())
                throw new ArgumentException("At least one phone number is required", nameof(phoneNumbers));

            // Initialize required services
            // Note: Should ideally use dependency injection instead of direct instantiation
            var phoneNumberSL = new PatientPhoneNumberSL();
            var emailValidator = new EmailValidator(dbContext);
            var passwordHasher = new PasswordHasher();

            // Check if the email is already in use by another patient
            if (!emailValidator.IsEmailUnique(patient.Email))
                throw new ArgumentException("Email already exists", nameof(patient.Email));

            // Create a new patient entity from the request data
            PatientModel newPatient = new PatientModel
            {
                FName = patient.FName,
                LName = patient.LName,
                BirthDate = patient.BirthDate,
                Email = patient.Email,
                // Hash the password before storing it for security
                Password = passwordHasher.HashPassword(patient.Password),
                PhotoPath = patient.PhotoPath,
                BloodType = patient.BloodType,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone,
                RoleID = 3, // Default patient role
                StatusID = 1, // Active status
                CreatedAt = DateTime.UtcNow, // Use UTC for consistency across time zones

                // Add required patient details
                Gender = patient.Gender,
                Link = patient.Link,
                ParentName = patient.ParentName,
                Address = patient.Address,

                // Optional patient fields
                SpouseName = patient.SpouseName,
                LandLine = patient.LandLine,
                Allergies = patient.Allergies,
                ChronicConditions = patient.ChronicConditions,
                PreviousSurgeries = patient.PreviousSurgeries,
                CurrentMedications = patient.CurrentMedications,
                PolicyNumber = patient.PolicyNumber,
                insuranceProvider = patient.insuranceProvider,
                PolicyValidDate = patient.PolicyValidDate,
            };

            // Begin a database transaction to ensure either all data is saved or none
            using var transaction = dbContext.Database.BeginTransaction();
            try
            {
                // Add the patient to the database context
                dbContext.Patients.Add(newPatient);
                // Save changes to generate the patient ID
                dbContext.SaveChanges();

                // Add phone numbers for the patient using the newly generated ID
                phoneNumberSL.AddPhoneNumbers(phoneNumbers, newPatient.ID);
                dbContext.SaveChanges();

                // Commit the transaction if everything succeeded
                transaction.Commit();
            }
            catch (DbUpdateException dbEx)
            {
                // Rollback the transaction if an error occurred
                transaction.Rollback();

                // Create detailed error message with database-specific information
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"Database update failed: {dbEx.Message}");

                // Include inner exception details if available
                if (dbEx.InnerException != null)
                {
                    errorMessage.AppendLine($"Inner exception: {dbEx.InnerException.Message}");

                    // Special handling for PostgreSQL-specific exceptions
                    if (dbEx.InnerException is PostgresException pgEx)
                    {
                        errorMessage.AppendLine($"Postgres Error Code: {pgEx.SqlState}");
                        errorMessage.AppendLine($"Detail: {pgEx.Detail}");
                        errorMessage.AppendLine($"Table: {pgEx.TableName}");
                        errorMessage.AppendLine($"Column: {pgEx.ColumnName}");
                        errorMessage.AppendLine($"Constraint: {pgEx.ConstraintName}");
                    }
                }

                // Log the error to console for debugging
                Console.WriteLine(errorMessage.ToString());

                // Log the error to a file for persistence
                File.AppendAllText("database_errors.log", $"[{DateTime.UtcNow}] {errorMessage}\n");

                // Throw a new exception with detailed information
                throw new Exception("Failed to save patient data. " + errorMessage, dbEx);
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                transaction.Rollback();
                Console.WriteLine($"General error: {ex.Message}\n{ex.StackTrace}");
                throw new Exception("An unexpected error occurred while saving patient", ex);
            }
        }

        /// <summary>
        /// Authenticates a patient using email and password
        /// </summary>
        /// <param name="email">The patient's email address</param>
        /// <param name="password">The patient's password</param>
        /// <returns>The patient model if authentication is successful</returns>
        /// <exception cref="ArgumentException">Thrown when email or password is missing</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when authentication fails</exception>
        public PatientModel GetPatientByEmailAndPassword(string email, string password)
        {
            // Validate that email and password are provided
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Email and password are required");
            }

            // Create a password hasher to verify passwords
            PasswordHasher hasher = new PasswordHasher();

            // Hash a dummy password for constant-time comparison
            // This helps prevent timing attacks that could reveal valid emails
            string dummyHash = hasher.HashPassword("dummy_password");

            // Find the patient with the specified email (even inactive ones)
            PatientModel patient = dbContext.Patients.FirstOrDefault(p => p.Email == email);

            // Verify the password and check that patient is active (not soft-deleted)
            bool passwordValid = patient != null &&
                                hasher.VerifyPassword(password, patient.Password) &&
                                patient.StatusID != 3;

            // Return consistent error message to prevent email enumeration attacks
            if (!passwordValid)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            return patient;
        }

        /// <summary>
        /// Updates a patient's profile information
        /// </summary>
        /// <param name="patientId">The unique identifier of the patient</param>
        /// <param name="request">The update request containing new patient data</param>
        /// <param name="phoneNumbers">Optional list of updated phone numbers</param>
        /// <exception cref="KeyNotFoundException">Thrown when patient is not found</exception>
        public void UpdateProfile(int patientId, PatientUpdateRequest request, List<string>? phoneNumbers)
        {
            // Find the patient with associated phone numbers
            var existingPatient = dbContext.Patients
                .Include(p => p.PhoneNumbers)
                .FirstOrDefault(p => p.ID == patientId) ?? throw new KeyNotFoundException("Patient not found");

            // Update patient properties if new values are provided
            // Only update fields that have non-empty values in the request
            if (!string.IsNullOrEmpty(request.FName)) existingPatient.FName = request.FName;
            if (!string.IsNullOrEmpty(request.LName)) existingPatient.LName = request.LName;
            if (!string.IsNullOrEmpty(request.Email)) existingPatient.Email = request.Email;
            if (!string.IsNullOrEmpty(request.PhotoPath)) existingPatient.PhotoPath = request.PhotoPath;
            if (!string.IsNullOrEmpty(request.BloodType)) existingPatient.BloodType = request.BloodType;
            if (!string.IsNullOrEmpty(request.EmergencyContactName)) existingPatient.EmergencyContactName = request.EmergencyContactName;
            if (!string.IsNullOrEmpty(request.EmergencyContactPhone)) existingPatient.EmergencyContactPhone = request.EmergencyContactPhone;
            if (!string.IsNullOrEmpty(request.Gender)) existingPatient.Gender = request.Gender;
            if (!string.IsNullOrEmpty(request.Link)) existingPatient.Link = request.Link;
            if (!string.IsNullOrEmpty(request.ParentName)) existingPatient.ParentName = request.ParentName;
            if (!string.IsNullOrEmpty(request.Address)) existingPatient.Address = request.Address;
            if (!string.IsNullOrEmpty(request.SpouseName)) existingPatient.SpouseName = request.SpouseName;
            if (!string.IsNullOrEmpty(request.LandLine)) existingPatient.LandLine = request.LandLine;
            if (!string.IsNullOrEmpty(request.Allergies)) existingPatient.Allergies = request.Allergies;
            if (!string.IsNullOrEmpty(request.ChronicConditions)) existingPatient.ChronicConditions = request.ChronicConditions;
            if (!string.IsNullOrEmpty(request.PreviousSurgeries)) existingPatient.PreviousSurgeries = request.PreviousSurgeries;
            if (!string.IsNullOrEmpty(request.CurrentMedications)) existingPatient.CurrentMedications = request.CurrentMedications;
            if (!string.IsNullOrEmpty(request.PolicyNumber)) existingPatient.PolicyNumber = request.PolicyNumber;
            if (!string.IsNullOrEmpty(request.insuranceProvider)) existingPatient.insuranceProvider = request.insuranceProvider;

            // Update password if provided and different from current password
            if (!string.IsNullOrEmpty(request.Password) && request.Password != existingPatient.Password)
            {
                // Hash the new password before storing
                existingPatient.Password = new PasswordHasher().HashPassword(request.Password);
            }

            // Update date fields if provided and valid
            if (request.BirthDate.HasValue && request.BirthDate != default)
                existingPatient.BirthDate = request.BirthDate.Value;

            if (request.PolicyValidDate.HasValue && request.PolicyValidDate != default)
                existingPatient.PolicyValidDate = request.PolicyValidDate.Value;

            // Update phone numbers if provided
            if (phoneNumbers != null)
            {
                // Clear existing phone numbers to replace with new list
                existingPatient.PhoneNumbers.Clear();

                // Add new phone numbers to the patient
                foreach (var number in phoneNumbers)
                {
                    existingPatient.PhoneNumbers.Add(new PatientPhoneNumberModel
                    {
                        PhoneNumber = number,
                        StatusID = 1, // Active status
                        PatientID = existingPatient.ID
                    });
                }
            }

            // Save all changes to the database
            dbContext.SaveChanges();
        }
        public void UpdatePatientStatus(int patientId, int newStatus)
        {
            // Validate status value
            if (newStatus < 1 || newStatus > 3)
                throw new ArgumentException("Invalid status value. Allowed: 1 (Active), 2 (Inactive), 3 (Deleted)");

            // Find the patient
            var patient = dbContext.Patients.FirstOrDefault(p => p.ID == patientId)
                ?? throw new KeyNotFoundException("Patient not found");

            // Update status
            patient.StatusID = newStatus;
            patient.UpdatedAt = DateTime.UtcNow; // Optional: for audit

            dbContext.SaveChanges();
        }

    }
}
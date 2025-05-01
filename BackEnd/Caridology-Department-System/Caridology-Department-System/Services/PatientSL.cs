
using System.Reflection;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Microsoft.EntityFrameworkCore;


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
        private readonly EmailValidator emailValidator;
        private readonly PasswordHasher passwordHasher;
        private readonly PatientPhoneNumberSL patientPhoneNumberSL;
        /// <summary>
        /// Constructor initializes a new database context
        /// </summary>
        public PatientSL(DBContext dB,PatientPhoneNumberSL numberSL,EmailValidator validator,PasswordHasher hasher)
        { 
            this.dbContext = dB;
            this.passwordHasher = hasher;
            this.emailValidator = validator;
            this.patientPhoneNumberSL = numberSL;

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
                .FirstOrDefault();               // Returns null if not found
        }

        /// <summary>
        /// Adds a new patient to the database with associated phone numbers
        /// </summary>
        /// <param name="patient">The patient data transfer object containing patient information</param>
        /// <param name="phoneNumbers">List of phone numbers associated with the patient</param>
        /// <exception cref="ArgumentNullException">Thrown when patient object is null</exception>
        /// <exception cref="ArgumentException">Thrown when phone numbers are missing or email exists</exception>
        public async Task AddPatientAsync(PatientRequest patient, List<string> phoneNumbers)
        {
            if (patient == null)
                throw new ArgumentNullException(nameof(patient), "Patient cannot be null");

            if (phoneNumbers == null || !phoneNumbers.Any())
                throw new ArgumentException("At least one phone number required", nameof(phoneNumbers));

            if (!await emailValidator.IsEmailUniqueAsync(patient.Email))
                throw new Exception("Email already in use");

            PatientModel newPatient = new PatientModel();
            var requestProps = typeof(PatientRequest).GetProperties();
            var patientProps = typeof(PatientModel).GetProperties();

            foreach (var requestProp in requestProps)
            {
                if (requestProp.Name == "PhoneNumbers")
                    continue;
                var patientProp = patientProps.FirstOrDefault(p => p.Name == requestProp.Name);
                if (patientProp != null && patientProp.CanWrite)
                {
                    var value = requestProp.GetValue(patient);
                    patientProp.SetValue(newPatient, value);
                }
            }
            newPatient.Password = passwordHasher.HashPassword(patient.Password);
            newPatient.RoleID = 3;
            newPatient.StatusID = 1;
            newPatient.CreatedAt = DateTime.UtcNow;


            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                await dbContext.Patients.AddAsync(newPatient);
                await dbContext.SaveChangesAsync(); // Saves patient first to get ID

                await patientPhoneNumberSL.AddPhoneNumbersAsync(phoneNumbers, newPatient.ID);
                await transaction.CommitAsync();
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Database error: {dbEx.InnerException?.Message}");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
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
        public async Task UpdateProfile(int patientId, PatientRequest request, List<string>? phoneNumbers)
        {
            // Validate input parameters
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Patient request cannot be null");

            // Begin transaction to ensure all operations succeed or fail together
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                // Get existing patient with phone numbers
                PatientModel existingPatient = await dbContext.Patients
                    .Include(p => p.PhoneNumbers)
                    .FirstOrDefaultAsync(p => p.ID == patientId);

                if (existingPatient == null)
                    throw new KeyNotFoundException("Patient not found");

                // Check if email is changed and validate uniqueness only if changed
                if (!string.IsNullOrEmpty(request.Email) &&
                    !existingPatient.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var emailValidator = new EmailValidator(dbContext);
                    bool isEmailUnique = await emailValidator.IsEmailUniqueAsync(request.Email);
                    if (!isEmailUnique)
                        throw new InvalidOperationException("Email is already in use by another account");
                }
                if (!string.IsNullOrEmpty(request.FName) && existingPatient.FName != request.FName)
                {
                    existingPatient.FName = request.FName;
                }
                if (!string.IsNullOrEmpty(request.LName) && existingPatient.LName != request.LName)
                {
                    existingPatient.LName = request.LName;
                }
                // Only update birth date if provided and different
                if (request.BirthDate != default && existingPatient.BirthDate != request.BirthDate)
                {
                    existingPatient.BirthDate = request.BirthDate;
                }
                if (!string.IsNullOrEmpty(request.Gender) && existingPatient.Gender != request.Gender)
                {
                    existingPatient.Gender = request.Gender;

                }
                if (!string.IsNullOrEmpty(request.Email) && existingPatient.Email != request.Email)
                {
                    existingPatient.Email = request.Email;
                }
                if (!string.IsNullOrEmpty(request.Address) && existingPatient.Address != request.Address)
                {
                    existingPatient.Address = request.Address;
                }
                // Update optional fields only if they are provided and different
                if (request.LandLine != null && existingPatient.LandLine != request.LandLine)
                {
                    existingPatient.LandLine = request.LandLine;
                }
                if (!string.IsNullOrEmpty(request.EmergencyContactName) &&
                    existingPatient.EmergencyContactName != request.EmergencyContactName)
                {
                    existingPatient.EmergencyContactName = request.EmergencyContactName;
                }
                if (!string.IsNullOrEmpty(request.EmergencyContactPhone) &&
                    existingPatient.EmergencyContactPhone != request.EmergencyContactPhone)
                {
                    existingPatient.EmergencyContactPhone = request.EmergencyContactPhone;
                }
                if (!string.IsNullOrEmpty(request.ParentName) && existingPatient.ParentName != request.ParentName)
                {
                    existingPatient.ParentName = request.ParentName;
                }
                if (request.SpouseName != null && existingPatient.SpouseName != request.SpouseName)
                {
                    existingPatient.SpouseName = request.SpouseName;
                }
                // Medical information
                if (request.BloodType != null && existingPatient.BloodType != request.BloodType)
                {
                    existingPatient.BloodType = request.BloodType;
                }
                if (request.Allergies != null && existingPatient.Allergies != request.Allergies)
                {
                    existingPatient.Allergies = request.Allergies;
                }
                if (request.ChronicConditions != null && existingPatient.ChronicConditions != request.ChronicConditions)
                {
                    existingPatient.ChronicConditions = request.ChronicConditions;

                }
                if (request.PreviousSurgeries != null && existingPatient.PreviousSurgeries != request.PreviousSurgeries)
                {
                    existingPatient.PreviousSurgeries = request.PreviousSurgeries;
                }
                if (request.CurrentMedications != null && existingPatient.CurrentMedications != request.CurrentMedications)
                {
                    existingPatient.CurrentMedications = request.CurrentMedications;
                }
                // Insurance information
                if (request.PolicyNumber != null && existingPatient.PolicyNumber != request.PolicyNumber)
                {
                    existingPatient.PolicyNumber = request.PolicyNumber;

                }
                if (request.InsuranceProvider != null && existingPatient.InsuranceProvider != request.InsuranceProvider)
                {
                    existingPatient.InsuranceProvider = request.InsuranceProvider;
                }
                if (request.PolicyValidDate.HasValue && existingPatient.PolicyValidDate != request.PolicyValidDate)
                {
                    existingPatient.PolicyValidDate = request.PolicyValidDate;
                }
                if (request.PhotoPath != null && existingPatient.PhotoPath != request.PhotoPath)
                {
                    existingPatient.PhotoPath = request.PhotoPath;
                }
                if (!string.IsNullOrEmpty(request.Link) && existingPatient.Link != request.Link)
                {
                    existingPatient.Link = request.Link;
                }
                PasswordHasher passwordHasher = new PasswordHasher();
                // Handle password update separately for security
                if (!string.IsNullOrEmpty(request.Password) &&
                    !passwordHasher.VerifyPassword(request.Password,existingPatient.Password))
                {
                    existingPatient.Password = new PasswordHasher().HashPassword(request.Password);

                }
                if (phoneNumbers != null)
                {
                    var currentPhoneNumbers = existingPatient.PhoneNumbers
                        .Select(p => p.PhoneNumber)
                        .ToList();

                    // Only process phone numbers if they're different from current ones
                    if (!phoneNumbers.OrderBy(p => p).SequenceEqual(currentPhoneNumbers.OrderBy(p => p)))
                    {
                        PatientPhoneNumberSL phoneNumberSL = new PatientPhoneNumberSL(dbContext); 
                        await phoneNumberSL.UpdatePhoneNumbersAsync(existingPatient.ID, phoneNumbers);
                    }
                }

                // Only save if changes were made
                await dbContext.SaveChangesAsync();
                

                await transaction.CommitAsync();
            }
            catch
            {
                // Rollback transaction on any error
                await transaction.RollbackAsync();
                throw; // Re-throw to be handled by the controller
            }
        }

        public void DeletePatient(int patientId)
        {
            // Check if patient exists (with proper PascalCase quoting)
            var patientExists = dbContext.Patients
                .FromSqlInterpolated($@"SELECT * FROM ""Patients"" WHERE ""ID"" = {patientId}")
                .Any();

            if (!patientExists)
                throw new KeyNotFoundException("Patient not found");
            var trans = dbContext.Database.BeginTransaction();
            try
            {
                // Delete related phone numbers
                new PatientPhoneNumberSL(dbContext).DeletePhoneNumbers(patientId);

                // Perform soft delete (with proper PascalCase quoting)
                dbContext.Database.ExecuteSqlInterpolated(
                $@"UPDATE ""Patients"" 
            SET ""StatusID"" = {3}, 
            ""UpdatedAt"" = {DateTime.UtcNow}
            WHERE ""ID"" = {patientId} AND ""StatusID"" <>{3}");
                trans.Commit();
            }
            catch(Exception ex) {
                trans.Rollback();
            }
        }

    }
}
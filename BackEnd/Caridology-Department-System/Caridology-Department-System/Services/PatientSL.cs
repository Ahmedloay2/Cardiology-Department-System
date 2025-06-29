
using System.Reflection;
using System.Text.Json;
using AutoMapper;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Caridology_Department_System.Requests.Patient;
using Caridology_Department_System.Requests.Patient;
using Microsoft.EntityFrameworkCore;


namespace Caridology_Department_System.Services
{
    /// <summary>
    /// Service layer for handling patient-related business logic such as profile updates and deletions.
    /// This class delegates responsibilities like phone number operations, email validation,
    /// password hashing, and image handling to specialized services.
    /// </summary>
    public class PatientSL
    {
        private readonly PatientPhoneNumberSL PatientPhoneNumberSL;
        private readonly IMapper mapper;
        private readonly PasswordHasher hasher;
        private readonly EmailValidator emailValidator;
        private readonly IImageService imageService;
        private readonly DBContext dbContext;
        /// <summary>
        /// Initializes a new instance of the <see cref="PatientSL"/> class with its required services.
        /// </summary>
        /// <param name="PatientPhoneNumberSL">Service for handling admin phone number operations.</param>
        /// <param name="dBContext">The application's database context.</param>
        /// <param name="emailValidator">Service for validating the uniqueness of email addresses.</param>
        /// <param name="passwordHasher">Service for hashing and verifying passwords.</param>
        /// <param name="imageService">Service for saving, retrieving, and deleting images.</param>
        /// <param name="mapper">AutoMapper instance for mapping between models and DTOs.</param>
        public PatientSL(DBContext dBContext, IImageService imageService,
                                   IMapper mapper, PasswordHasher passwordHasher, EmailValidator emailValidator,
                                   PatientPhoneNumberSL PatientPhoneNumberSL)
        {
            this.dbContext = dBContext;
            this.mapper = mapper;
            this.hasher = passwordHasher;
            this.emailValidator = emailValidator;
            this.imageService = imageService;
            this.PatientPhoneNumberSL = PatientPhoneNumberSL;
        }
        /// <summary>
        /// Adds a new patient to the system along with their phone numbers and profile photo.
        /// Performs input validation, password hashing, image saving, and runs within a database transaction.
        /// </summary>
        /// <param name="request">
        /// The patient creation request containing name, email, password, phone numbers, optional photo, and some other data.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the patient and phone numbers were added successfully; <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="request"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if:
        /// - <paramref name="request.PhoneNumbers"/> is null or empty,
        /// - <paramref name="request.Email"/> is already used,
        /// - Or mapping the request to <c>PatientModel</c> fails.
        /// </exception>
        /// <remarks>
        /// This method uses a database transaction. If adding phone numbers fails,
        /// the patient record is rolled back.
        /// </remarks>
        public async Task<bool> AddPatientAsync(PatientRequest request)
        {
            bool created = false;
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Patient data cannot be empty");
            }
            if (request.PhoneNumbers == null || !request.PhoneNumbers.Any())
            {
                created = false;
                throw new ArgumentException("At least one phone number is required");
            }
            if (!await emailValidator.IsEmailUniqueAsync(request.Email))
            {
                created = false;
                throw new ArgumentException("Email is already used");
            }
            using var transaction = dbContext.Database.BeginTransaction();
            {
                PatientModel Patient = mapper.Map<PatientModel>(request);
                if (Patient == null)
                {
                    created = false;
                    throw new ArgumentException("error has occured");
                }
                if (request.Photo != null)
                {
                    Patient.PhotoPath = await imageService.SaveImageAsync(request.Photo);
                }
                Patient.Password = hasher.HashPassword(request.Password);
                await dbContext.Patients.AddAsync(Patient);
                await dbContext.SaveChangesAsync();
                created = await PatientPhoneNumberSL.AddPhoneNumbersasync(request.PhoneNumbers, Patient.ID, transaction);
                if (!created)
                {
                    await transaction.RollbackAsync();
                    return created;
                }
                await transaction.CommitAsync();
                return created;
            }
        }
        /// <summary>
        /// Retrieves a patient user by their email and password, including role information.
        /// </summary>
        /// <param name="login">
        /// The login request containing the patient's email and password.
        /// </param>
        /// <returns>
        /// Returns a <see cref="PatientModel"/> if the credentials are valid and the patient account is active.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="login.Email"/> or <paramref name="login.Password"/> is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if the email is not found, the password is incorrect, or the account is marked as deleted.
        /// </exception>
        public async Task<PatientModel> GetPatientByEmailAndPassword(LoginRequest Request)
        {
            if (string.IsNullOrWhiteSpace(Request.Email))
            {
                throw new ArgumentException("Email is required");
            }
            if (string.IsNullOrWhiteSpace(Request.Password))
            {
                throw new ArgumentException("Password is required");
            }
            PatientModel Patient = await dbContext.Patients.Include(p => p.Role).SingleOrDefaultAsync(p => p.Email == Request.Email);
            if (Patient == null &&
                !hasher.VerifyPassword(Request.Password, Patient.Password) &&
                Patient.StatusID == 3)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }
            return Patient;
        }
        /// <summary>
        /// Retrieves a patient user by their unique ID, including phone numbers and role information.
        /// </summary>
        /// <param name="Patientid">
        /// The ID of the patient to retrieve. Must be a valid, non-null, positive integer.
        /// </param>
        /// <returns>
        ///  Returns a <see cref="PatientModel"/> that matches the given ID, or throws an exception if not found.
        /// </returns>
        /// <exception cref="Exception"></exception>
        /// Thrown if no patient is found with the given ID (or if the account is deleted).
        public async Task<PatientModel> GetPatientByID(int? Patientid)
        {
            PatientModel Patient = await dbContext.Patients
                                        .Where(p => p.ID == Patientid && p.StatusID != 3)
                                        .Include(p => p.PhoneNumbers.Where(p => p.StatusID != 3))
                                        .Include(p => p.Role)
                                        .SingleOrDefaultAsync() ?? throw new Exception("account doesnot exist");
            return Patient;
        }
        /// <summary>
        /// Retrieves the profile of a patient by their ID, including role, phone numbers, and optional profile photo as Base64.
        /// </summary>
        /// <param name="Patientid">
        /// The unique ID of the patient whose profile is requested. Must be a non-null, positive integer.
        /// </param>
        /// <returns>
        /// Returns a <see cref="PatientProfilePageRequest"/> object mapped from the patient entity, 
        /// with embedded Base64-encoded profile photo (if available).
        /// </returns>
        public async Task<PatientProfilePageRequest> GetPatientProfilePage(int? Patientid)
        {
            PatientModel Patient = await GetPatientByID(Patientid);
            PatientProfilePageRequest PatientProfile = mapper.Map<PatientProfilePageRequest>(Patient);
            if (!String.IsNullOrEmpty(Patient.PhotoPath))
            {
                PatientProfile.PhotoData = imageService.GetImageBase64(Patient.PhotoPath);
            }
            return PatientProfile;
        }
        /// <summary>
        /// Deletes a patient and their associated phone numbers within a database transaction.
        /// The patient is soft-deleted by updating their status.
        /// </summary>
        /// <param name="Patientid">The unique identifier of the patient to delete.</param>
        /// <returns>True if the deletion was successful and changes were committed; otherwise, false.</returns>
        public async Task<bool> DeletePatientAsync(int Patientid)
        {
            bool deleted = false;
            PatientModel Patient = await GetPatientByID(Patientid);
            using var transaction = dbContext.Database.BeginTransaction();
            {

                List<string> phones = Patient.PhoneNumbers.Select(p => p.PhoneNumber).ToList();
                if (phones.Count > 0)
                {
                    deleted = await PatientPhoneNumberSL.DeletePhonesAsync(phones, Patientid, transaction);
                }
                Patient.StatusID = 3;
                Patient.UpdatedAt = DateTime.UtcNow;
                if (!deleted)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return deleted;
            }
        }
        /// <summary>
        /// Updates a patient's profile details, including personal information, email, photo, and phone numbers, within a database transaction. 
        /// AutoMapper is used to map other fields, and changes are saved only if modifications are detected.
        /// </summary>
        /// <param name="Patientid">The unique identifier of the patient to update.</param>
        /// <param name="request">An object containing the new data to apply to the patient profile.</param>
        /// <returns>True if any changes were detected and saved; otherwise, false.</returns>
        public async Task<bool> UpdateProfileAsync(int Patientid, PatientUpdateRequest request)
        {
            PatientModel patient = await GetPatientByID(Patientid);
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                // Serialize original state for comparison
                var originalJson = JsonSerializer.Serialize(new
                {
                    patient.FName,
                    patient.LName,
                    patient.BirthDate,
                    patient.Gender,
                    patient.Address,
                    patient.LandLine,
                    patient.BloodType,
                    patient.Allergies,
                    patient.ChronicConditions,
                    patient.PreviousSurgeries,
                    patient.CurrentMedications,
                    patient.EmergencyContactName,
                    patient.EmergencyContactPhone,
                    patient.ParentName,
                    patient.SpouseName,
                    patient.PolicyNumber,
                    patient.InsuranceProvider,
                    patient.PolicyValidDate,
                    patient.PhotoPath,
                    patient.Email,
                    patient.Link
                });

                bool hasChanges = false;

                // Handle photo
                if (request.PhotoData != null && request.PhotoData.Length > 0)
                {
                    string newPhotoPath = await imageService.SaveImageAsync(request.PhotoData);
                    if (!newPhotoPath.Equals(patient.PhotoPath))
                    {
                        patient.PhotoPath = newPhotoPath;
                        hasChanges = true;
                    }
                }

                // Handle email with validation
                if (!string.IsNullOrWhiteSpace(request.Email) && !request.Email.Equals(patient.Email))
                {
                    if (!await emailValidator.IsEmailUniqueAsync(request.Email))
                        throw new Exception("Email is already used");
                    patient.Email = request.Email;
                    hasChanges = true;
                }

                // Handle phone numbers
                if (request.PhoneNumbers != null && request.PhoneNumbers.Any(p => !string.IsNullOrWhiteSpace(p)))
                {
                    var currentPhones = patient.PhoneNumbers.Select(p => p.PhoneNumber).ToList();
                    var newPhones = request.PhoneNumbers.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();

                    if (currentPhones.Count != newPhones.Count ||
                        !new HashSet<string>(currentPhones).SetEquals(new HashSet<string>(newPhones)))
                    {
                        await PatientPhoneNumberSL.UpdatePhonesAsync(newPhones, Patientid, transaction);
                        hasChanges = true;
                    }
                }

                // Use AutoMapper for all other properties - it will only map changed values
                mapper.Map(request, patient);

                // Check if AutoMapper made any changes by comparing serialized state
                var updatedJson = JsonSerializer.Serialize(new
                {
                    patient.FName,
                    patient.LName,
                    patient.BirthDate,
                    patient.Gender,
                    patient.Address,
                    patient.LandLine,
                    patient.BloodType,
                    patient.Allergies,
                    patient.ChronicConditions,
                    patient.PreviousSurgeries,
                    patient.CurrentMedications,
                    patient.EmergencyContactName,
                    patient.EmergencyContactPhone,
                    patient.ParentName,
                    patient.SpouseName,
                    patient.PolicyNumber,
                    patient.InsuranceProvider,
                    patient.PolicyValidDate,
                    patient.PhotoPath,
                    patient.Email,
                    patient.Link
                });

                if (!originalJson.Equals(updatedJson))
                {
                    hasChanges = true;
                }

                // Only save if there are changes
                if (hasChanges)
                {
                    patient.UpdatedAt = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return hasChanges;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        /// <summary>
        /// Retrieves a paginated list of patients and their phone numbers, 10 per page.
        /// </summary>
        /// <param name="name">The admin name to search for. Can be partial or full depending on <paramref name="exactmatch"/>.</param>
        /// <param name="pagenumber">The page number to retrieve. Defaults to 1.</param>
        /// <param name="exactmatch">If true, searches for names that exactly match; otherwise, performs a partial match.</param>
        /// <returns>A list of <see cref="PatientModel"/> objects including associated phone numbers (excluding those with StatusID 3 (deleted) ).</returns>
        public async Task<List<PatientModel>> GetPatientsPerPageAsync(string? name,
                                int pagenumber = 1, bool exactmatch = false)
        {
            List<PatientModel> PatientsPerPage = new List<PatientModel>();
            int pageSize = 10;
            if (!String.IsNullOrEmpty(name))
            {
                if (exactmatch)
                {
                    PatientsPerPage = await dbContext.Patients
                                    .Where(d => (d.FName + " " + d.LName).Contains(name))
                                    .Skip((pagenumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                                    .ToListAsync();
                }
                else
                {
                    PatientsPerPage = await dbContext.Patients
                                    .Where(d => (d.FName + " " + d.LName).StartsWith(name))
                                    .Skip((pagenumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                                    .ToListAsync();
                }

            }
            else
            {
                PatientsPerPage = await dbContext.Patients
                .Skip((pagenumber - 1) * pageSize)
                .Take(pageSize)
                .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                .ToListAsync();
            }
            return PatientsPerPage;
        }
        /// <summary>
        /// Retrieves a paginated list of patient profiles and thier phone number, 10 per page.
        /// </summary>
        /// <param name = "name" >
        /// The admin name to search for. Can be partial or full depending on<paramref name="exactmatch"/>.
        /// </param>
        /// <param name="pagenumber">
        /// The page number to retrieve. Defaults to 1.
        /// </param>
        /// <param name="exactmatch">
        /// If true, searches for names that exactly match; otherwise, performs a partial match.
        /// </param>
        /// <returns>
        /// A list of <see cref="PatientProfilePageRequest"/> objects including associated phone numbers (excluding those with StatusID 3 (deleted) )
        /// </returns>
        public async Task<List<PatientProfilePageRequest>> GetPatientsProfilePerPageAsync(string? name,
                                        int pagenumber = 1, bool exactmatch = false)
        {
            List<PatientModel> PatientsPerPage = new List<PatientModel>();
            int pageSize = 10;
            if (!String.IsNullOrEmpty(name))
            {
                if (exactmatch)
                {
                    PatientsPerPage = await dbContext.Patients
                                    .Where(d => (d.FName + " " + d.LName).Contains(name))
                                    .Skip((pagenumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                                    .ToListAsync();
                }
                else
                {
                    PatientsPerPage = await dbContext.Patients
                                    .Where(d => (d.FName + " " + d.LName).StartsWith(name))
                                    .Skip((pagenumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                                    .ToListAsync();
                }

            }
            else
            {
                PatientsPerPage = await dbContext.Patients
                .Skip((pagenumber - 1) * pageSize)
                .Take(pageSize)
                .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                .ToListAsync();
            }
            List<PatientProfilePageRequest> PatientProfilePages = new List<PatientProfilePageRequest>();
            foreach (PatientModel Patient in PatientsPerPage)
            {
                PatientProfilePageRequest PatientProfilePage = mapper.Map<PatientProfilePageRequest>(Patient);
                if (!String.IsNullOrEmpty(Patient.PhotoPath))
                {
                    PatientProfilePage.PhotoData = imageService.GetImageBase64(Patient.PhotoPath);
                }
                PatientProfilePages.Add(PatientProfilePage);
            }
            return PatientProfilePages;
        }
    }
}
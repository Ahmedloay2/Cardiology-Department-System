
using System.Reflection;
using System.Text.Json;
using AutoMapper;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Caridology_Department_System.Requests.Patient;
using Microsoft.EntityFrameworkCore;


namespace Caridology_Department_System.Services
{
    /// <summary>
    /// Service Layer class for Patient-related operations
    /// Handles database operations for patient records including CRUD operations
    /// </summary>
    public class PatientSL
    {
        private readonly PatientPhoneNumberSL PatientPhoneNumberSL;
        private readonly IMapper mapper;
        private readonly PasswordHasher hasher;
        private readonly EmailValidator emailValidator;
        private readonly IImageService imageService;
        private readonly DBContext dbContext;
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
        public async Task<bool> AddPatientAsync(PatientRequest request)
        {
            bool created = false;
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Admin data cannot be empty");
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
            PatientModel Patient = await dbContext.Patients.SingleOrDefaultAsync(d => d.Email == Request.Email);
            bool passwordValid = Patient != null &&
                hasher.VerifyPassword(Request.Password, Patient.Password) &&
                Patient.StatusID != 3;
            if (!passwordValid)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }
            return Patient;
        }
        public async Task<PatientModel> GetPatientByID(int? Patientid)
        {
            PatientModel Patient = await dbContext.Patients
                                        .Where(a => a.ID == Patientid && a.StatusID != 3)
                                        .Include(a => a.PhoneNumbers.Where(p => p.StatusID != 3))
                                        .SingleOrDefaultAsync() ?? throw new Exception("account doesnot exist");
            return Patient;
        }
        public async Task<PatientProfilePageRequest> GetPatientProfilePage(int? Patientid)
        {
            PatientModel Patient = await GetPatientByID(Patientid) ?? throw new Exception("account doesnot exist");
            PatientProfilePageRequest PatientProfile = mapper.Map<PatientProfilePageRequest>(Patient);
            if (!String.IsNullOrEmpty(Patient.PhotoPath))
            {
                PatientProfile.PhotoData = imageService.GetImageBase64(Patient.PhotoPath);
            }
            return PatientProfile;
        }
        public async Task<bool> DeletePatientAsync(int? Patientid)
        {
            bool deleted = false;
            PatientModel Patient = await GetPatientByID(Patientid);
            using var transaction = dbContext.Database.BeginTransaction();
            {

                List<string> phones = Patient.PhoneNumbers.Select(p => p.PhoneNumber).ToList();
                if (phones.Count > 0)
                {
                    deleted = await PatientPhoneNumberSL.DeletePhonesAsync(phones, Patient.ID, transaction);
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
        public async Task<bool> UpdateProfileAsync(int Patientid, PatientUpdateRequest request)
        {
            PatientModel patient = await GetPatientByID(Patientid) ??
                throw new Exception("User not found");

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
    }
}
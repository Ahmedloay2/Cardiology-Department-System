using AutoMapper;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Caridology_Department_System.Requests.Doctor;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Caridology_Department_System.Services
{
    /// <summary>
    /// Service layer for handling doctor-related business logic such as profile updates and deletions.
    /// This class delegates responsibilities like phone number operations, email validation,
    /// password hashing, and image handling to specialized services.
    /// </summary>
    public class DoctorSL
    {
        private readonly DoctorPhoneNumberSL doctorPhoneNumberSL;
        private readonly IMapper mapper;
        private readonly PasswordHasher hasher;
        private readonly EmailValidator emailValidator;
        private readonly IImageService imageService;
        private readonly DBContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DoctorSL"/> class with its required services.
        /// </summary>
        /// <param name="doctorPhoneNumberSL">Service for handling admin phone number operations.</param>
        /// <param name="dBContext">The application's database context.</param>
        /// <param name="emailValidator">Service for validating the uniqueness of email addresses.</param>
        /// <param name="passwordHasher">Service for hashing and verifying passwords.</param>
        /// <param name="imageService">Service for saving, retrieving, and deleting images.</param>
        /// <param name="mapper">AutoMapper instance for mapping between models and DTOs.</param>
        public DoctorSL(DBContext dBContext, IImageService imageService,
                                   IMapper mapper, PasswordHasher passwordHasher, EmailValidator emailValidator,
                                   DoctorPhoneNumberSL doctorPhoneNumberSL)
        {
            this.dbContext = dBContext;
            this.mapper = mapper;
            this.hasher = passwordHasher;
            this.emailValidator = emailValidator;
            this.imageService = imageService;
            this.doctorPhoneNumberSL = doctorPhoneNumberSL;
        }

        /// <summary>
        /// Adds a new doctor to the system along with their phone numbers and profile photo.
        /// Performs input validation, password hashing, image saving, and runs within a database transaction.
        /// </summary>
        /// <param name="request">
        /// The doctor creation request containing name, email, password, phone numbers, optional photo, and some other data.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the doctor and phone numbers were added successfully; <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="request"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if:
        /// - <paramref name="request.PhoneNumbers"/> is null or empty,
        /// - <paramref name="request.Email"/> is already used,
        /// - Or mapping the request to <c>DoctorModel</c> fails.
        /// </exception>
        /// <remarks>
        /// This method uses a database transaction. If adding phone numbers fails,
        /// the doctor record is rolled back.
        /// </remarks>
        public async Task<bool> AddDoctorAsync(DoctorRequest request)
        {
            bool created=false;
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Doctor data cannot be empty");
            }
            if (request.PhoneNumbers == null || request.PhoneNumbers.Count == 0)
            {
                created = false;
                throw new ArgumentException("At least one phone number is required", nameof(request.PhoneNumbers));
            }
            if (!await emailValidator.IsEmailUniqueAsync(request.Email))
            {
                created = false;
                throw new ArgumentException("Email is already used");
            }
            using var transaction =dbContext.Database.BeginTransaction();
            {
                DoctorModel doctor = mapper.Map<DoctorModel>(request);
                if (doctor == null)
                {
                    created = false;
                    throw new ArgumentException("error has occured");
                }
                if (request.Photo != null)
                {
                    doctor.PhotoPath = await imageService.SaveImageAsync(request.Photo);
                }
                doctor.Password= hasher.HashPassword(request.Password);
                await dbContext.Doctors.AddAsync(doctor);
                await dbContext.SaveChangesAsync();
                created = await doctorPhoneNumberSL.AddPhoneNumbersasync(request.PhoneNumbers,doctor.ID,transaction);
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
        /// Retrieves an doctor user by their email and password, including role information.
        /// </summary>
        /// <param name="login">
        /// The login request containing the doctor's email and password.
        /// </param>
        /// <returns>
        /// Returns an <see cref="DoctorModel"/> if the credentials are valid and the doctor account is active.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="login.Email"/> or <paramref name="login.Password"/> is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if the email is not found, the password is incorrect, or the account is marked as deleted.
        /// </exception>
        public async Task<DoctorModel> GetDoctorByEmailAndPassword(LoginRequest Request)
        {
            if (string.IsNullOrWhiteSpace(Request.Email))
            {
                throw new ArgumentException("Email is required");
            }
            if (string.IsNullOrWhiteSpace(Request.Password))
            {
                throw new ArgumentException("Password is required");
            }
            DoctorModel doctor = await dbContext.Doctors
                                .Include(d => d.Role)
                                .SingleOrDefaultAsync(d => d.Email == Request.Email);
            if (doctor == null &&
                !hasher.VerifyPassword(Request.Password, doctor.Password) &&
                doctor.StatusID == 3)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }
            return doctor;
        }

        /// <summary>
        /// Retrieves an doctor user by their unique ID, including phone numbers and role information.
        /// </summary>
        /// <param name="doctorid">
        /// The ID of the doctor to retrieve. Must be a valid, non-null, positive integer.
        /// </param>
        /// <returns>
        ///  Returns an <see cref="DoctorModel"/> that matches the given ID, or throws an exception if not found.
        /// </returns>
        /// <exception cref="Exception"></exception>
        /// Thrown if no doctor is found with the given ID (or if the account is deleted).
        public async Task<DoctorModel> GetDoctorByID(int? doctorid)
        {
            DoctorModel doctor = await dbContext.Doctors
                                        .Where(d => d.ID == doctorid && d.StatusID != 3)
                                        .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                                        .Include(d => d.Role)
                                        .SingleOrDefaultAsync();
            if (doctor == null)
            {
                throw new Exception("Doctor account doesnot exist");
            }
            return doctor;
        }

        /// <summary>
        /// Retrieves the profile of an patient by their ID, including role, phone numbers, and optional profile photo as Base64.
        /// </summary>
        /// <param name="Patientid">
        /// The unique ID of the patient whose profile is requested. Must be a non-null, positive integer.
        /// </param>
        /// <returns>
        /// Returns an <see cref="PatientProfilePageRequest"/> object mapped from the patient entity, 
        /// with embedded Base64-encoded profile photo (if available).
        /// </returns>
        public async Task<DoctorProfilePageRequest> GetDoctorProfilePage(int? doctorid)
        {
            DoctorModel doctor = await GetDoctorByID(doctorid);
            DoctorProfilePageRequest DoctorProfile = mapper.Map<DoctorProfilePageRequest>(doctor);
            if (!String.IsNullOrEmpty(doctor.PhotoPath))
            {
                DoctorProfile.PhotoData = imageService.GetImageBase64(doctor.PhotoPath);
            }
            return DoctorProfile ;
        }

        /// <summary>
        /// Deletes a doctor and their associated phone numbers within a database transaction.
        /// The doctor is soft-deleted by updating their status.
        /// </summary>
        /// <param name="doctorid">The unique identifier of the doctor to delete.</param>
        /// <returns>True if the deletion was successful and changes were committed; otherwise, false.</returns>
        public async Task<bool> DeleteDoctorAsync(int? doctorid)
        {
            bool deleted = false;
            DoctorModel doctor = await GetDoctorByID(doctorid);
            using var transaction = dbContext.Database.BeginTransaction();
            {
                
                List<string> phones = doctor.PhoneNumbers.Select(p => p.PhoneNumber).ToList();
                if (phones.Count > 0 && phones.Any())
                {
                    deleted = await doctorPhoneNumberSL.DeletePhonesAsync(phones,doctor.ID,transaction);
                }
                doctor.StatusID = 3;
                doctor.UpdatedAt = DateTime.UtcNow;
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
        /// Updates a doctor's profile details, including personal info, email, photo, and phone numbers, within a database transaction.
        /// </summary>
        /// <param name="doctorid">The unique identifier of the doctor to update.</param>
        /// <param name="request">The new data to apply to the doctor profile.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateProfileAsync(int doctorid, DoctorUpdateRequest request)
        {
            DoctorModel doctor = await GetDoctorByID(doctorid);

            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                bool hasChanges = false;

                if (request.PhotoData != null && request.PhotoData.Length > 0)
                {
                    string temp = await imageService.SaveImageAsync(request.PhotoData);
                    if (!temp.Equals(doctor.PhotoPath))
                    {
                        doctor.PhotoPath = temp;
                        hasChanges = true;
                    }
                }

                if (!String.IsNullOrEmpty(request.Address) && !request.Address.Equals(doctor.Address))
                {
                    doctor.Address = request.Address;
                    hasChanges = true;
                }
                if (!String.IsNullOrEmpty(request.Position) && !request.Position.Equals(doctor.Position))
                {
                    doctor.Position = request.Position;
                    hasChanges = true;
                }
                if (request.YearsOfExperience.HasValue && request.YearsOfExperience.Value>=0 && request.YearsOfExperience.Value!=doctor.YearsOfExperience)
                {
                    doctor.YearsOfExperience = request.YearsOfExperience.Value;
                    hasChanges = true;
                }
                if (request.Salary.HasValue && request.Salary.Value >= 0 && request.Salary.Value != doctor.Salary)
                {
                    doctor.Salary= request.Salary.Value;
                    hasChanges = true;
                }
                if (!String.IsNullOrEmpty(request.FName) && !request.FName.Equals(doctor.FName))
                {
                    doctor.FName = request.FName;
                    hasChanges = true;
                }

                if (!String.IsNullOrEmpty(request.LName) && !request.LName.Equals(doctor.LName))
                {
                    doctor.LName = request.LName;
                    hasChanges = true;
                }
                if (!String.IsNullOrEmpty(request.Gender) && !request.Gender.Equals(doctor.Gender))
                {
                    doctor.Gender = request.Gender;
                    hasChanges = true;
                }

                if (request.BirthDate.HasValue && request.BirthDate.Value > DateTime.MinValue && request.BirthDate != null)
                {
                    // Check if they're actually different
                    if (doctor.BirthDate != request.BirthDate.Value)
                    {
                        doctor.BirthDate = request.BirthDate.Value;
                        hasChanges = true;
                    }
                }
                if (!string.IsNullOrWhiteSpace(request.Email) && !(doctor.Email.Equals(request.Email)))
                {
                    if (!await emailValidator.IsEmailUniqueAsync(request.Email))
                    {
                        throw new Exception("Email is already used");
                    }
                    doctor.Email = request.Email;
                    hasChanges = true;
                }
                List<string> doctorPhoneNumbers = doctor.PhoneNumbers.Select(p => p.PhoneNumber).ToList();
                if (request.PhoneNumbers != null &&
                    request.PhoneNumbers.Any() &&
                    request.PhoneNumbers.Any(p => !string.IsNullOrWhiteSpace(p)) &&
                    !new HashSet<string>(request.PhoneNumbers).SetEquals(doctorPhoneNumbers))
                {
                    await doctorPhoneNumberSL.UpdatePhonesAsync(request.PhoneNumbers, doctorid, transaction);
                    hasChanges = true;
                }
                // Only update timestamp and save if there are actual changes
                if (hasChanges)
                {
                    doctor.UpdatedAt = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Retrieves a paginated list of doctors and their phone numbers, 10 per page.
        /// </summary>
        /// <param name="name">The admin name to search for. Can be partial or full depending on <paramref name="exactmatch"/>.</param>
        /// <param name="pagenumber">The page number to retrieve. Defaults to 1.</param>
        /// <param name="exactmatch">If true, searches for names that exactly match; otherwise, performs a partial match.</param>
        /// <returns>A list of <see cref="DoctorModel"/> objects including associated phone numbers (excluding those with StatusID 3 (deleted) ).</returns>
        public async Task<List<DoctorModel>> GetDoctorsPerPageAsync(string? name,
                                        int pagenumber = 1, bool exactmatch = false)
        {
            List<DoctorModel> doctorsPerPage = new List<DoctorModel>();
            int pageSize = 10;
            if (!String.IsNullOrEmpty(name))
            {
                if (exactmatch)
                {
                    doctorsPerPage = await dbContext.Doctors
                                    .Where(d => (d.FName + " " + d.LName).Contains(name))
                                    .Skip((pagenumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                                    .ToListAsync();
                }
                else
                {
                    doctorsPerPage = await dbContext.Doctors
                                    .Where(d => (d.FName + " " + d.LName).StartsWith(name))
                                    .Skip((pagenumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                                    .ToListAsync();
                }

            }
            else
            {
                doctorsPerPage = await dbContext.Doctors
                .Skip((pagenumber - 1) * pageSize)
                .Take(pageSize)
                .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                .ToListAsync();
            }
            return doctorsPerPage;
        }

        /// <summary>
        /// Retrieves a paginated list of doctor profiles and thier phone number, 10 per page.
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
        /// A list of <see cref="DoctorProfilePageRequest"/> objects including associated phone numbers (excluding those with StatusID 3 (deleted) )
        /// </returns>
        public async Task<List<DoctorProfilePageRequest>> GetDoctorsProfilePerPageAsync(string? name,
                                        int pagenumber = 1, bool exactmatch = false)
        {
            List<DoctorModel> doctorsPerPage = new List<DoctorModel>();
            int pageSize = 10;
            if (!String.IsNullOrEmpty(name))
            {
                if (exactmatch)
                {
                    doctorsPerPage = await dbContext.Doctors
                                    .Where(d => (d.FName + " " + d.LName).Contains(name))
                                    .Skip((pagenumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                                    .ToListAsync();
                }
                else
                {
                    doctorsPerPage = await dbContext.Doctors
                                    .Where(d => (d.FName + " " + d.LName).StartsWith(name))
                                    .Skip((pagenumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                                    .ToListAsync();
                }

            }
            else
            {
                doctorsPerPage = await dbContext.Doctors
                .Skip((pagenumber - 1) * pageSize)
                .Take(pageSize)
                .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                .ToListAsync();
            }
            List<DoctorProfilePageRequest> doctorProfilePages = new List<DoctorProfilePageRequest>();
            foreach (DoctorModel doctor in doctorsPerPage)
            {
                DoctorProfilePageRequest doctorProfilePage =mapper.Map<DoctorProfilePageRequest>(doctor);
                if (!String.IsNullOrEmpty(doctor.PhotoPath))
                {
                    doctorProfilePage.PhotoData = imageService.GetImageBase64(doctor.PhotoPath);
                }
                doctorProfilePages.Add(doctorProfilePage);
            }
            return doctorProfilePages;
        }

        /// <summary>
        /// Checks whether a doctor with the specified ID exists and is not marked as deleted.
        /// </summary>
        /// <param name="doctorID">The unique ID of the requested doctor.</param>
        /// <returns>True if the doctor exists and is active.</returns>
        /// <exception cref="Exception">Thrown if the doctor does not exist or is marked as deleted.</exception>
        public async Task<bool> DoctorExists(int? doctorID)
        {
            bool DoctorExist= await dbContext.Doctors.AnyAsync(d => d.ID == doctorID && d.StatusID != 3);
            if (!DoctorExist)
            {
                throw new Exception("Doctor not found");
            }
            return true;
        }
    }   
}

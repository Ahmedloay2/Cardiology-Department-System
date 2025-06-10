using AutoMapper;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Caridology_Department_System.Requests.Doctor;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Caridology_Department_System.Services
{
    public class DoctorSL
    {
        private readonly DoctorPhoneNumberSL doctorPhoneNumberSL;
        private readonly IMapper mapper;
        private readonly PasswordHasher hasher;
        private readonly EmailValidator emailValidator;
        private readonly IImageService imageService;
        private readonly DBContext dbContext;
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
        public async Task<bool> AddDoctorAsync(DoctorRequest request)
        {
            bool created=false;
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Admin data cannot be empty");
            }
            if (request.PhoneNumbers == null || !request.PhoneNumbers.Any())
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
            DoctorModel doctor = await dbContext.Doctors.SingleOrDefaultAsync(d => d.Email == Request.Email);
            bool passwordValid = doctor != null &&
                hasher.VerifyPassword(Request.Password, doctor.Password) &&
                doctor.StatusID != 3;
            if (!passwordValid)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }
            return doctor;
        }
        public async Task<DoctorModel> GetDoctorByID(int? doctorid)
        {
            DoctorModel doctor = await dbContext.Doctors
                                        .Where(a => a.ID == doctorid && a.StatusID != 3)
                                        .Include(a => a.PhoneNumbers.Where(p => p.StatusID != 3))
                                        .SingleOrDefaultAsync();
            if (doctor == null)
            {
                throw new Exception("account doesnot exist");
            }
            return doctor;
        }
        public async Task<DoctorProfilePageRequest> GetDoctorProfilePage(int? doctorid)
        {
            DoctorModel doctor = await GetDoctorByID(doctorid);
            if (doctor == null)
            {
                throw new Exception("account doesnot exist");
            }
            DoctorProfilePageRequest DoctorProfile = mapper.Map<DoctorProfilePageRequest>(doctor);
            if (!String.IsNullOrEmpty(doctor.PhotoPath))
            {
                DoctorProfile.PhotoData = imageService.GetImageBase64(doctor.PhotoPath);
            }
            return DoctorProfile ;
        }
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
        public async Task<bool> UpdateProfileAsync(int doctorid, DoctorUpdateRequest request)
        {
            DoctorModel doctor = await GetDoctorByID(doctorid);

            if (doctor == null)
            {
                throw new Exception("User not found");
            }

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
                    new HashSet<string>(request.PhoneNumbers).SetEquals(doctorPhoneNumbers))
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
    }
    
}

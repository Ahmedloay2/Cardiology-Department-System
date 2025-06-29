using System.Text;
using AutoMapper;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Caridology_Department_System.Requests.Admin;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Caridology_Department_System.Services
{
    /// <summary>
    /// Service layer for handling admin-related business logic such as profile updates and deletions.
    /// This class delegates responsibilities like phone number operations, email validation,
    /// password hashing, and image handling to specialized services.
    /// </summary>
    public class AdminSL
    {
        private readonly DBContext dbContext;
        private readonly EmailValidator emailValidator;
        private readonly PasswordHasher passwordHasher;
        private readonly AdminPhoneNumberSL adminPhoneNumberSL;
        private readonly IImageService imageService;
        private readonly IMapper automapper;
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminSL"/> class with its required services.
        /// </summary>
        /// <param name="adminPhoneNumberSL">Service for handling admin phone number operations.</param>
        /// <param name="dbContext">The application's database context.</param>
        /// <param name="emailValidator">Service for validating the uniqueness of email addresses.</param>
        /// <param name="passwordHasher">Service for hashing and verifying passwords.</param>
        /// <param name="imageService">Service for saving, retrieving, and deleting images.</param>
        /// <param name="automapper">AutoMapper instance for mapping between models and DTOs.</param>
        public AdminSL(AdminPhoneNumberSL adminPhoneNumberSL, DBContext dbContext,
                        EmailValidator emailValidator, PasswordHasher passwordHasher,
                        IImageService imageService, IMapper automapper)
        {
            this.dbContext = dbContext;
            this.adminPhoneNumberSL = adminPhoneNumberSL;
            this.emailValidator = emailValidator;
            this.passwordHasher = passwordHasher;
            this.imageService = imageService;
            this.automapper = automapper;
        }
        /// <summary>
        /// Retrieves an admin user by their unique ID, including phone numbers and role information.
        /// </summary>
        /// <param name="adminId">
        /// The ID of the admin to retrieve. Must be a valid, non-null, positive integer.
        /// </param>
        /// <returns>
        ///  Returns an <see cref="AdminModel"/> that matches the given ID, or throws an exception if not found.
        /// </returns>
        /// <exception cref="Exception"></exception>
        /// Thrown if no admin is found with the given ID (or if the account is deleted).
        public async Task<AdminModel> GetAdminByID(int? adminId)
        {
            AdminModel admin = await dbContext.Admins
                .Where(a => a.ID == adminId && a.StatusID != 3)
                .Include(a => a.PhoneNumbers.Where(p => p.StatusID!=3))
                .Include(a => a.Role)
                .SingleOrDefaultAsync();
            if (admin == null)
                throw new Exception("account doesnot exist");
            return admin;
        }
        /// <summary>
        /// Retrieves the profile of an admin by their ID, including role, phone numbers, and optional profile photo as Base64.
        /// </summary>
        /// <param name="adminId">
        /// The unique ID of the admin whose profile is requested. Must be a non-null, positive integer.
        /// </param>
        /// <returns>
        /// Returns an <see cref="AdminProfilePageRequest"/> object mapped from the admin entity, 
        /// with embedded Base64-encoded profile photo (if available).
        /// </returns>
        public async Task<AdminProfilePageRequest> GetAdminProfile(int? adminId)
        {
            AdminModel admin = await GetAdminByID(adminId);
            AdminProfilePageRequest request = automapper.Map<AdminProfilePageRequest>(admin);
            if (!string.IsNullOrEmpty(admin.PhotoPath))
            {
                request.PhotoData = imageService.GetImageBase64(admin.PhotoPath);
            }
            return request;
        }
        /// <summary>
        /// Retrieves an admin user by their email and password, including role information.
        /// </summary>
        /// <param name="login">
        /// The login request containing the admin's email and password.
        /// </param>
        /// <returns>
        /// Returns an <see cref="AdminModel"/> if the credentials are valid and the admin account is active.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="login.Email"/> or <paramref name="login.Password"/> is null, empty, or whitespace.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if the email is not found, the password is incorrect, or the account is marked as deleted.
        /// </exception>
        public async Task<AdminModel> GetAdminByEmailAndPassword(LoginRequest login)
        {
            if (string.IsNullOrWhiteSpace(login.Email))
            {
                throw new ArgumentException("Email is required", nameof(login.Email));
            }

            if (string.IsNullOrWhiteSpace(login.Password))
            {
                throw new ArgumentException("Password is required", nameof(login.Password));
            }
            AdminModel admin = await dbContext.Admins
                .Include(a => a.Role)
                .SingleOrDefaultAsync(a => a.Email == login.Email);

            if (admin == null || admin.StatusID == 3 ||
                !passwordHasher.VerifyPassword(login.Password, admin.Password))
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            return admin;
        }
        /// <summary>
        /// Adds a new Admin to the system along with their phone numbers and profile photo.
        /// Performs input validation, password hashing, image saving, and runs within a database transaction.
        /// </summary>
        /// <param name="request">
        /// The Admin creation request containing name, email, password, phone numbers, optional photo, and some other data.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the Admin and phone numbers were added successfully; <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="request"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if:
        /// - <paramref name="request.PhoneNumbers"/> is null or empty,
        /// - <paramref name="request.Email"/> is already used,
        /// - Or mapping the request to <c>AdminModel</c> fails.
        /// </exception>
        /// <remarks>
        /// This method uses a database transaction. If adding phone numbers fails,
        /// the Admin record is rolled back.
        /// </remarks>
        public async Task<bool> AddAdminasync(AdminRequest request)
        {
            bool created = false;
            if (request == null)
            {
                created = false;
                throw new ArgumentNullException(nameof(request), "Admin data cannot be empty");
            }
            if (request.PhoneNumbers == null || !request.PhoneNumbers.Any())
            {
                created = false;
                throw new ArgumentException("At least one phone number is required", nameof(request.PhoneNumbers));
            }
            // Email uniqueness check
            if (!await emailValidator.IsEmailUniqueAsync(request.Email))
            {
                created = false;
                throw new ArgumentException("Email already exists", nameof(request.Email));
            }
            // Create new admin entity from request
            AdminModel newAdmin = automapper.Map<AdminModel>(request);
            newAdmin.StatusID = 1;
            newAdmin.CreatedAt = DateTime.UtcNow;
            newAdmin.RoleID = 1;
            newAdmin.Password = passwordHasher.HashPassword(request.Password);
            if (request.Photo != null)
            {
                newAdmin.PhotoPath = await imageService.SaveImageAsync(request.Photo);
            }
            // Transaction for safe DB insert
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                // 1. Add and save admin FIRST to get the ID
                await dbContext.Admins.AddAsync(newAdmin);
                await dbContext.SaveChangesAsync();
                bool success = await adminPhoneNumberSL.AddPhoneNumbersasync(request.PhoneNumbers,
                                                                             newAdmin.ID, transaction);
                if (!success)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // Handle specific database errors
                if (ex is DbUpdateException dbEx && dbEx.InnerException is PostgresException pgEx)
                {
                    var errorDetails = new StringBuilder()
                        .AppendLine($"Database error ({pgEx.SqlState}): {pgEx.Message}")
                        .AppendLine($"Table: {pgEx.TableName}")
                        .AppendLine($"Constraint: {pgEx.ConstraintName}");

                    // Log the error
                    Console.WriteLine($"Admin creation failed: {errorDetails}");

                    throw new Exception($"Database operation failed: {pgEx.Message}", pgEx);
                }

                // Log general errors
                Console.WriteLine($"Admin creation failed: {ex.Message}\n{ex.StackTrace}");
                throw new Exception("An unexpected error occurred while saving admin", ex);
            }
        }
        /// <summary>
        /// Updates an admin's profile details, including personal info, email, photo, and phone numbers, within a database transaction.
        /// </summary>
        /// <param name="adminId">The unique identifier of the admin to update.</param>
        /// <param name="request">The new data to apply to the admin profile.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        public async Task<bool> UpdateProfileAsync(int adminId, AdminUpdateRequest request)
        {
            AdminModel existingAdmin = await GetAdminByID(adminId);
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                bool hasChanges = false;

                if (request.PhotoData != null && request.PhotoData.Length > 0 && !imageService.SaveImageAsync(request.PhotoData).Equals(existingAdmin.PhotoPath))
                {
                    existingAdmin.PhotoPath = await imageService.SaveImageAsync(request.PhotoData);
                    hasChanges = true;
                }

                if (!String.IsNullOrEmpty(request.Address) && !request.Address.Equals(existingAdmin.Address))
                {
                    existingAdmin.Address = request.Address;
                    hasChanges = true;
                }

                if (!String.IsNullOrEmpty(request.FName)&&!request.FName.Equals(existingAdmin.FName))
                {
                    existingAdmin.FName = request.FName;
                    hasChanges = true;
                }

                if (!String.IsNullOrEmpty(request.LName) && !request.LName.Equals(existingAdmin.LName))
                {
                    existingAdmin.LName = request.LName;
                    hasChanges = true;
                }
                if (!String.IsNullOrEmpty(request.Gender) && !request.Gender.Equals(existingAdmin.Gender))
                {
                    existingAdmin.Gender = request.Gender;
                    hasChanges = true;
                }

                if (request.BirthDate.HasValue && request.BirthDate.Value > DateTime.MinValue && request.BirthDate != null)
                {
                    // Check if they're actually different
                    if (existingAdmin.BirthDate != request.BirthDate.Value)
                    {
                        existingAdmin.BirthDate = request.BirthDate.Value;
                        hasChanges = true;
                    }
                }
                if (!string.IsNullOrWhiteSpace(request.Email)&&!(existingAdmin.Email.Equals(request.Email)))
                {                    
                    if (!await emailValidator.IsEmailUniqueAsync(request.Email))
                    {
                        throw new Exception("Email is already used");
                    }
                    existingAdmin.Email = request.Email;
                    hasChanges = true;
                }
                List<string> existingadminPhoneNumbers = existingAdmin.PhoneNumbers.Select(p => p.PhoneNumber).ToList();
                if (request.PhoneNumbers != null &&
                    request.PhoneNumbers.Any() &&
                    request.PhoneNumbers.Any(p => !string.IsNullOrWhiteSpace(p))&&
                    !new HashSet<string>(request.PhoneNumbers).SetEquals(existingadminPhoneNumbers))
                {
                    await adminPhoneNumberSL.UpdatePhonesAsync(request.PhoneNumbers, adminId, transaction);
                    hasChanges = true;
                }
                // Only update timestamp and save if there are actual changes
                if (hasChanges)
                {
                    existingAdmin.UpdatedAt = DateTime.UtcNow;
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
        /// Deletes an admin and their associated phone numbers within a database transaction.
        /// The admin is soft-deleted by updating their status.
        /// </summary>
        /// <param name="adminId">The unique identifier of the admin to delete.</param>
        /// <returns>True if the deletion was successful and changes were committed; otherwise, false.</returns>
        public async Task<bool> DeleteAdminAsync(int adminId)
        {
            try
            {
               AdminModel admin = await GetAdminByID(adminId);
               using var transaction = await dbContext.Database.BeginTransactionAsync();
                {
                    bool deleted = true;
                    List<string> existingadminPhoneNumbers = admin.PhoneNumbers.Select(p => p.PhoneNumber).ToList();
                    if (existingadminPhoneNumbers.Count>0)
                    {
                         deleted = await adminPhoneNumberSL.DeletePhonesAsync(existingadminPhoneNumbers, adminId, transaction);
                    }
                    admin.StatusID = 3;
                    admin.UpdatedAt = DateTime.UtcNow;
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
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Retrieves a paginated list of admins and their phone numbers, 10 per page.
        /// </summary>
        /// <param name="name">The admin name to search for. Can be partial or full depending on <paramref name="exactmatch"/>.</param>
        /// <param name="pagenumber">The page number to retrieve. Defaults to 1.</param>
        /// <param name="exactmatch">If true, searches for names that exactly match; otherwise, performs a partial match.</param>
        /// <returns>A list of <see cref="AdminModel"/> objects including associated phone numbers (excluding those with StatusID 3 (deleted) ).</returns>
        public async Task<List<AdminModel>> GetAdminsPerPageAsync(string? name,
                                int pagenumber = 1, bool exactmatch = false)
        {
            List<AdminModel> AdminsPerPage = new List<AdminModel>();
            int pageSize = 10;
            if (!String.IsNullOrWhiteSpace(name))
            {
                string loweredName = name.ToLower();
                if (exactmatch)
                {
                    AdminsPerPage = await dbContext.Admins
                                    .Where(a => (a.FName + " " + a.LName).ToLower().Contains(loweredName) && a.StatusID != 3)
                                    .Skip((pagenumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                                    .ToListAsync();
                }
                else
                {
                    AdminsPerPage = await dbContext.Admins
                                    .Where(a => (a.FName + " " + a.LName).ToLower().StartsWith(loweredName) && a.StatusID !=3)
                                    .Skip((pagenumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                                    .ToListAsync();
                }

            }
            else
            {
                AdminsPerPage = await dbContext.Admins
                .Where(a => a.StatusID !=3)
                .Skip((pagenumber - 1) * pageSize)
                .Take(pageSize)
                .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                .ToListAsync();
            }
            return AdminsPerPage;
        }
        /// <summary>
        /// Retrieves a paginated list of admin profiles and thier phone number, 10 per page.
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
        /// A list of <see cref="AdminProfilePageRequest"/> objects including associated phone numbers (excluding those with StatusID 3 (deleted) )
        /// </returns>
        public async Task<List<AdminProfilePageRequest>> GetAdminsProfilePerPageAsync(string? name,
                                        int pagenumber = 1, bool exactmatch = false)
        {
            List<AdminModel> AdminsPerPage = new List<AdminModel>();
            int pageSize = 10;
            if (!String.IsNullOrEmpty(name))
            {
                string loweredName = name.ToLower();
                if (exactmatch)
                {
                    AdminsPerPage = await dbContext.Admins
                                    .Where(a => (a.FName + " " + a.LName).ToLower().Contains(loweredName) && a.StatusID != 3)
                                    .Skip((pagenumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                                    .ToListAsync();
                }
                else
                {
                    AdminsPerPage = await dbContext.Admins
                                    .Where(a => (a.FName + " " + a.LName).ToLower().StartsWith(loweredName) && a.StatusID != 3)
                                    .Skip((pagenumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                                    .ToListAsync();
                }

            }
            else
            {
                AdminsPerPage = await dbContext.Admins
                .Where(a => a.StatusID != 3)
                .Skip((pagenumber - 1) * pageSize)
                .Take(pageSize)
                .Include(d => d.PhoneNumbers.Where(p => p.StatusID != 3))
                .ToListAsync();
            }
            List<AdminProfilePageRequest> AdminProfilePages = new List<AdminProfilePageRequest>();
            foreach (AdminModel Admin in AdminsPerPage)
            {
                AdminProfilePageRequest AdminProfilePage = automapper.Map<AdminProfilePageRequest>(Admin);
                if (!String.IsNullOrEmpty(Admin.PhotoPath))
                {
                    AdminProfilePage.PhotoData = imageService.GetImageBase64(Admin.PhotoPath);
                }
                AdminProfilePages.Add(AdminProfilePage);
            }
            return AdminProfilePages;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AutoMapper;
using Azure.Core;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Caridology_Department_System.Services
{
    /// <summary>
    /// Service Layer for Admin-related operations (CRUD, authentication, patient management).
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
        /// Initializes a new instance of the AdminSL service, creating a new DB context.
        /// </summary>
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
        /// Retrieves an admin by their ID, including phone numbers, only if not deleted.
        /// </summary>
        public async Task<AdminModel> GetAdminByID(int? adminId)
        {
            if (adminId == null)
                throw new ArgumentNullException(nameof(adminId));
            // Find admin with phone numbers, skip if deleted (StatusID == 3)
            AdminModel admin = await dbContext.Admins
                .Where(a => a.ID == adminId && a.StatusID != 3)
                .Include(a => a.PhoneNumbers.Where(p => p.StatusID!=3))
                .SingleOrDefaultAsync();
            if (admin == null)
                throw new Exception("account doesnot exist");
            return admin;
        }
        public async Task<AdminProfilePageRequest> GetAdminProfile(int? adminId)
        {
            if (adminId == null) throw new ArgumentNullException(nameof(adminId));
            AdminModel admin = await GetAdminByID(adminId);
            AdminProfilePageRequest request = automapper.Map<AdminProfilePageRequest>(admin);
            if (!String.IsNullOrEmpty(admin.PhotoPath))
            {
                request.PhotoData = imageService.GetImageBase64(admin.PhotoPath);
            }
            return request;
        }

        /// <summary>
        /// Retrieves an admin by email and password for login.
        /// Throws if credentials are invalid or admin is deleted.
        /// </summary>
        public async Task<AdminModel> GetAdminByEmailAndPassword(LoginRequest login)
        {
            if (string.IsNullOrWhiteSpace(login.Email))
            {
                throw new ArgumentException("Email is required");
            }
            if (string.IsNullOrWhiteSpace(login.Password))
            {
                throw new ArgumentException("Password is required");
            }
            AdminModel admin = await dbContext.Admins.SingleOrDefaultAsync(a => a.Email == login.Email);

            // Check password and ensure admin is not deleted
            bool passwordValid = admin != null &&
                passwordHasher.VerifyPassword(login.Password, admin.Password) &&
                admin.StatusID != 3;

            if (!passwordValid)
                throw new UnauthorizedAccessException("Invalid email or password");

            return admin;
        }
        /// <summary>
        /// Adds a new admin with validated data and phone numbers.
        /// Throws if email is not unique or other validation fails.
        /// </summary>
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
        /// Updates an existing admin's profile with provided fields and phone numbers.
        /// Only updates fields that are supplied (partial update).
        /// </summary>
        public async Task<bool> UpdateProfileAsync(int adminId, AdminUpdateRequest request)
        {
            AdminModel existingAdmin = await GetAdminByID(adminId);

            if (existingAdmin == null)
            {
                throw new Exception("User not found");
            }

            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                bool hasChanges = false;

                if (request.PhotoData != null && request.PhotoData.Length > 0 && !request.FName.Equals(existingAdmin.FName))
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
                    new HashSet<string>(request.PhoneNumbers).SetEquals(existingadminPhoneNumbers))
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
        /*
        /// <summary>
        /// Updates the status of an admin (e.g., activate, deactivate, delete).
        /// </summary>
        public void UpdateAdminStatus(int adminId, int newStatus)
        {
            // Validate allowed status values
            if (newStatus < 1 || newStatus > 3)
                throw new ArgumentException("Invalid status value. Allowed: 1 (Active), 2 (Inactive), 3 (Deleted)");

            var admin = dbContext.Admins.FirstOrDefault(a => a.ID == adminId)
                ?? throw new KeyNotFoundException("Admin not found");

            admin.StatusID = newStatus;
            admin.UpdatedAt = DateTime.UtcNow;
            dbContext.SaveChanges();
        }*/
        /*
        /// <summary>
        /// Soft deletes a patient (sets status to deleted) and removes their phone numbers.
        /// </summary>
        public void DeletePatient(int patientId)
        {
            var patient = dbContext.Patients
                .Include(p => p.PhoneNumbers)
                .FirstOrDefault(p => p.ID == patientId && p.StatusID != 3)
                ?? throw new KeyNotFoundException("Patient not found");

            using var transaction = dbContext.Database.BeginTransaction();
            try
            {
                // Remove all phone numbers
                dbContext.PatientPhoneNumbers.RemoveRange(patient.PhoneNumbers);
                // Soft delete: set status to 3 (deleted)
                patient.StatusID = 3;
                dbContext.SaveChanges();
                transaction.Commit();
            }
            catch (DbUpdateException dbEx)
            {
                transaction.Rollback();
                throw new Exception("Failed to delete patient", dbEx);
            }
        }*/
        /*
        /// <summary>
        /// Updates the status of a patient (e.g., activate, deactivate, delete).
        /// </summary>
        public void UpdatePatientStatus(int patientId, int newStatus)
        {
            // Validate allowed status values
            if (newStatus < 1 || newStatus > 3)
                throw new ArgumentException("Invalid status value. Valid values: 1 (Active), 2 (Inactive), 3 (Deleted)");

            var patient = dbContext.Patients
                .FirstOrDefault(p => p.ID == patientId && p.StatusID != 3)
                ?? throw new KeyNotFoundException("Patient not found");

            patient.StatusID = newStatus;
            patient.UpdatedAt = DateTime.UtcNow;
            dbContext.SaveChanges();
        }*/
        public async Task<bool> DeleteAdminAsync(int adminId)
        {
            try
            {
               AdminModel admin = await GetAdminByID(adminId);
               using var transaction = await dbContext.Database.BeginTransactionAsync();
                {
                    bool deleted = true;
                    List<string> existingadminPhoneNumbers = admin.PhoneNumbers.Select(p => p.PhoneNumber).ToList();
                    if (existingadminPhoneNumbers.Any() && existingadminPhoneNumbers.Count>0)
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
    }
}

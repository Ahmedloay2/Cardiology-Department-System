using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
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

        /// <summary>
        /// Initializes a new instance of the AdminSL service, creating a new DB context.
        /// </summary>
        public AdminSL()
        {
            dbContext = new DBContext();
        }

        /// <summary>
        /// Retrieves an admin by their ID, including phone numbers, only if not deleted.
        /// </summary>
        public AdminModel GetAdminByID(int? adminId)
        {
            if (adminId == null)
                throw new ArgumentNullException(nameof(adminId));

            // Find admin with phone numbers, skip if deleted (StatusID == 3)
            var admin = dbContext.Admins
                .Where(a => a.ID == adminId && a.StatusID != 3)
                .Include(a => a.PhoneNumbers)
                .SingleOrDefault();

            return admin;
        }

        /// <summary>
        /// Retrieves an admin by email and password for login.
        /// Throws if credentials are invalid or admin is deleted.
        /// </summary>
        public AdminModel GetAdminByEmailAndPassword(LoginRequest login)
        {
            if (string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
                throw new ArgumentException("Email and password are required");

            var hasher = new PasswordHasher();
            var admin = dbContext.Admins.SingleOrDefault(a => a.Email == login.Email);

            // Check password and ensure admin is not deleted
            bool passwordValid = admin != null &&
                hasher.VerifyPassword(login.Password, admin.Password) &&
                admin.StatusID != 3;

            if (!passwordValid)
                throw new UnauthorizedAccessException("Invalid email or password");

            return admin;
        }

        /// <summary>
        /// Adds a new admin with validated data and phone numbers.
        /// Throws if email is not unique or other validation fails.
        /// </summary>
        public void AddAdmin(AdminRequest request, List<string> phoneNumbers)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Admin data cannot be empty");

            if (phoneNumbers == null || !phoneNumbers.Any())
                throw new ArgumentException("At least one phone number is required", nameof(phoneNumbers));

            // Email uniqueness check
            if (dbContext.Admins.Any(a => a.Email == request.Email))
                throw new ArgumentException("Email already exists", nameof(request.Email));

            var passwordHasher = new PasswordHasher();

            // Create new admin entity from request
            var newAdmin = new AdminModel
            {
                FName = request.FName,
                LName = request.LName,
                BirthDate = request.BirthDate,
                Email = request.Email,
                Password = passwordHasher.HashPassword(request.Password),
                PhotoPath = request.PhotoPath,
                RoleID = request.RoleID,
                StatusID = request.StatusID,
                CreatedAt = DateTime.UtcNow,
                Address = request.Address,
            };

            // Add phone numbers to the admin
            foreach (var number in phoneNumbers)
            {
                newAdmin.PhoneNumbers.Add(new AdminPhoneNumberModel
                {
                    PhoneNumber = number,
                    StatusID = 1,
                    AdminID = newAdmin.ID
                });
            }

            // Transaction for safe DB insert
            using var transaction = dbContext.Database.BeginTransaction();
            try
            {
                dbContext.Admins.Add(newAdmin);
                dbContext.SaveChanges();
                transaction.Commit();
            }
            catch (DbUpdateException dbEx)
            {
                transaction.Rollback();
                // Log detailed DB errors for troubleshooting
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"Database update failed: {dbEx.Message}");

                if (dbEx.InnerException != null)
                {
                    errorMessage.AppendLine($"Inner exception: {dbEx.InnerException.Message}");
                    if (dbEx.InnerException is PostgresException pgEx)
                    {
                        errorMessage.AppendLine($"Postgres Error Code: {pgEx.SqlState}");
                        errorMessage.AppendLine($"Detail: {pgEx.Detail}");
                        errorMessage.AppendLine($"Table: {pgEx.TableName}");
                        errorMessage.AppendLine($"Column: {pgEx.ColumnName}");
                        errorMessage.AppendLine($"Constraint: {pgEx.ConstraintName}");
                    }
                }
                Console.WriteLine(errorMessage.ToString());
                File.AppendAllText("database_errors.log", $"[{DateTime.UtcNow}] {errorMessage}\n");
                throw new Exception("Failed to save admin data. " + errorMessage, dbEx);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"General error: {ex.Message}\n{ex.StackTrace}");
                throw new Exception("An unexpected error occurred while saving admin", ex);
            }
        }

        /// <summary>
        /// Updates an existing admin's profile with provided fields and phone numbers.
        /// Only updates fields that are supplied (partial update).
        /// </summary>
        public void UpdateProfile(int adminId, AdminUpdateRequest request, List<string>? phoneNumbers)
        {
            var existingAdmin = dbContext.Admins
                .Include(a => a.PhoneNumbers)
                .FirstOrDefault(a => a.ID == adminId)
                ?? throw new KeyNotFoundException("Admin not found");

            // Update only provided fields (partial update)
            if (!string.IsNullOrEmpty(request.FName))
                existingAdmin.FName = request.FName;
            if (!string.IsNullOrEmpty(request.LName))
                existingAdmin.LName = request.LName;
            if (!string.IsNullOrEmpty(request.Email))
                existingAdmin.Email = request.Email;
            if (!string.IsNullOrEmpty(request.PhotoPath))
                existingAdmin.PhotoPath = request.PhotoPath;
            if (!string.IsNullOrEmpty(request.Address))
                existingAdmin.Address = request.Address;

            // Hash new password if changed
            if (!string.IsNullOrEmpty(request.Password) && request.Password != existingAdmin.Password)
                existingAdmin.Password = new PasswordHasher().HashPassword(request.Password);

            if (request.BirthDate.HasValue && request.BirthDate != default)
                existingAdmin.BirthDate = request.BirthDate.Value;

            if (request.RoleID.HasValue)
                existingAdmin.RoleID = request.RoleID.Value;
            if (request.StatusID.HasValue)
                existingAdmin.StatusID = request.StatusID.Value;

            // Replace phone numbers if provided
            if (phoneNumbers != null)
            {
                existingAdmin.PhoneNumbers.Clear();
                foreach (var number in phoneNumbers)
                {
                    existingAdmin.PhoneNumbers.Add(new AdminPhoneNumberModel
                    {
                        PhoneNumber = number,
                        StatusID = 1,
                        AdminID = existingAdmin.ID
                    });
                }
            }

            existingAdmin.UpdatedAt = DateTime.UtcNow;
            dbContext.SaveChanges();
        }

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
        }

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
        }

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
        }
    }
}

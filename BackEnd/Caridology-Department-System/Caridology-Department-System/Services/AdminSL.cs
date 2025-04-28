using System.Text;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Caridology_Department_System.Services
{
    public class AdminSL
    {
        DBContext dbcontext;
        public AdminSL() { 
        dbcontext = new DBContext();
        }
        public AdminModel GetAdminByID(int? AdminID)
        {
            if (AdminID == null)
                throw new ArgumentNullException(nameof(AdminID));
            AdminModel admin = dbcontext.Admins.Where( a=>a.ID==AdminID && a.StatusID != 3)
                .Include(a => a.PhoneNumbers).
                SingleOrDefault() ;
            return admin ;
        }
        public void AddAdmin(AdminRequest admin)
        {

            if (admin == null)
            {
                throw new ArgumentNullException(nameof(admin));
            }
            if (admin.PhoneNumbers == null || !admin.PhoneNumbers.Any())
                throw new ArgumentException("At least one phone number is required", nameof(admin.PhoneNumbers));

            AdminPhoneNumberSL adminPhoneNumberSL = new AdminPhoneNumberSL();
            PasswordHasher Hasher = new PasswordHasher();
            EmailValidator emailValidator = new EmailValidator(dbcontext);
            if (!emailValidator.IsEmailUnique(admin.Email))
            {
                throw new ArgumentException("email already exist", nameof(admin.Email));
            }
            AdminModel createdadmin = new AdminModel
            {
                FName = admin.FName,
                LName = admin.LName,
                BirthDate = admin.BirthDate,
                Email = admin.Email,
                Password = Hasher.HashPassword(admin.Password),
                PhotoPath = admin.PhotoPath,

                StatusID = 1,
                CreatedAt = DateTime.UtcNow,
                RoleID = 1
            };
            var transaction = dbcontext.Database.BeginTransaction();
            try
            {
                dbcontext.Admins.Add(createdadmin);
                dbcontext.SaveChanges();
                adminPhoneNumberSL.AddPhoneNumbers(admin.PhoneNumbers,createdadmin.ID);
                dbcontext.SaveChanges();
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
        public AdminModel GetAdminByEmailAndPassword(LoginRequest Admin)
        {
            if (string.IsNullOrWhiteSpace(Admin.Email) || string.IsNullOrWhiteSpace(Admin.Password))
            {
                throw new ArgumentException("Email and password are required");
            }
            PasswordHasher hasher = new PasswordHasher();
            string dummyHash = hasher.HashPassword("dummy_password");
            AdminModel LAdmin = dbcontext.Admins.SingleOrDefault(a => a.Email == Admin.Email);
            bool passwordValid = LAdmin != null &&
                    hasher.VerifyPassword(Admin.Password, LAdmin.Password) &&
                    LAdmin.StatusID != 3;
            if (!passwordValid)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            return LAdmin;
        }
     }
}


using Caridology_Department_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Caridology_Department_System.Services
{
    /// <summary>
    /// Service class for managing admin phone numbers, including adding, updating, and deleting them with soft-delete logic.
    /// </summary>
    public class AdminPhoneNumberSL
    {
        private readonly  DBContext dbcontext;
        /// <summary>
        /// Initializes a new instance of the <see cref="AdminPhoneNumberSL"/> class with the specified database context.
        /// </summary>
        /// <param name="dbcontext">The database context used for phone number operations.</param>
        public AdminPhoneNumberSL(DBContext dbcontext)
        {
            this.dbcontext = dbcontext;
        }
        /// <summary>
        /// Adds a list of phone numbers for the specified admin within a database transaction.
        /// </summary>
        /// <param name="phoneNumbers">The list of phone numbers to add.</param>
        /// <param name="adminID">The unique ID of the admin to whom the phone numbers belong.</param>
        /// <param name="transaction">The current database transaction.</param>
        /// <returns><c>true</c> if the phone numbers were added successfully; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Thrown if the phone numbers list is null or empty.</exception>
        public async Task<bool> AddPhoneNumbersasync(List<string> PhoneNumbers,int AdminID,
                                                     IDbContextTransaction transaction)
        {
            await dbcontext.Database.UseTransactionAsync(transaction.GetDbTransaction());
            if (PhoneNumbers == null || !PhoneNumbers.Any())
                throw new ArgumentException("Phone numbers list is empty.");
            
            List<AdminPhoneNumberModel> adminPhones = PhoneNumbers.Select(Phone => new AdminPhoneNumberModel
            {
                PhoneNumber = Phone,
                AdminID = AdminID,
                StatusID=1
            }).ToList();
            await dbcontext.AdminPhoneNumbers.AddRangeAsync(adminPhones);
            await dbcontext.SaveChangesAsync();
            return true;
        }
        /// <summary>
        /// Updates an admin's phone numbers by comparing the existing list to the new list.
        /// Removes numbers no longer present and adds new ones, all within a transaction.
        /// </summary>
        /// <param name="newPhoneNumbers">The updated list of phone numbers.</param>
        /// <param name="adminID">The ID of the admin whose phone numbers will be updated.</param>
        /// <param name="transaction">The active database transaction.</param>
        /// <returns><c>true</c> if the update was successful; otherwise, <c>false</c>.</returns>
        public async Task<bool> UpdatePhonesAsync(List<string> newPhoneNumbers, int AdminID, IDbContextTransaction transaction)
        {
            await dbcontext.Database.UseTransactionAsync(transaction.GetDbTransaction());

            if (newPhoneNumbers == null)
                newPhoneNumbers = new List<string>();

            // Get existing phone numbers 
            List<string> existingPhoneNumbers = await dbcontext.AdminPhoneNumbers
                .Where(p => p.AdminID == AdminID && p.StatusID != 3)
                .Select(p => p.PhoneNumber)
                .ToListAsync();

            // Find differences using string comparison
            List<string> phonesToDelete = existingPhoneNumbers.Except(newPhoneNumbers).ToList();
            List<string> phonesToAdd = newPhoneNumbers.Except(existingPhoneNumbers).ToList();

            bool deleteSuccess = true;
            bool addSuccess = true;            
            // Delete phones that are no longer needed
            if (phonesToDelete.Any())
            {
                deleteSuccess = await DeletePhonesAsync(phonesToDelete, AdminID, transaction);
            }

            // Add new phones
            if (phonesToAdd.Any())
            {
                addSuccess = await AddPhoneNumbersasync(phonesToAdd, AdminID, transaction);
            }

            return deleteSuccess && addSuccess;
        }
        /// <summary>
        /// Soft-deletes the specified phone numbers for a given admin by setting their status to deleted (StatusID = 3).
        /// </summary>
        /// <param name="phoneNumbers">The list of phone numbers to delete.</param>
        /// <param name="adminID">The ID of the admin whose phone numbers will be deleted.</param>
        /// <param name="transaction">The current database transaction.</param>
        /// <returns><c>true</c> if the phone numbers were marked as deleted successfully; otherwise, <c>false</c>.</returns>
        public async Task<bool> DeletePhonesAsync(List<String>PhoneNumbers , int AdminID, IDbContextTransaction transaction)
        {
            await dbcontext.Database.UseTransactionAsync(transaction.GetDbTransaction());
            foreach (var phoneNumber in PhoneNumbers)
            {
               await dbcontext.AdminPhoneNumbers.Where(p=> p.PhoneNumber.Equals(phoneNumber)&&
               p.StatusID!=3 && p.AdminID==AdminID)
                    .ExecuteUpdateAsync(s => s.SetProperty(p=> p.StatusID,3));              
            }
            await dbcontext.SaveChangesAsync();
            return true;
        }
    }
}

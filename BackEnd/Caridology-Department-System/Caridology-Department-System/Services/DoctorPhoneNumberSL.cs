using AutoMapper;
using Caridology_Department_System.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace Caridology_Department_System.Services
{
    /// <summary>
    /// Service class for managing doctor phone numbers, including adding, updating, and deleting them with soft-delete logic.
    /// </summary>
    public class DoctorPhoneNumberSL
    {
        private readonly DBContext dbcontext;
        /// <summary>
        /// Initializes a new instance of the <see cref="DoctorPhoneNumberSL"/> class with the specified database context.
        /// </summary>
        /// <param name="dBContext">The database context used for phone number operations.</param>
        public DoctorPhoneNumberSL(DBContext dBContext)
        {
            this.dbcontext = dBContext;
        }
        /// <summary>
        /// Adds a list of phone numbers for the specified doctor within a database transaction.
        /// </summary>
        /// <param name="PhoneNumbers">The list of phone numbers to add.</param>
        /// <param name="DoctorID">The unique ID of the doctor to whom the phone numbers belong.</param>
        /// <param name="transaction">The current database transaction.</param>
        /// <returns><c>true</c> if the phone numbers were added successfully; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Thrown if the phone numbers list is null or empty.</exception>
        public async Task<bool> AddPhoneNumbersasync(List<string> PhoneNumbers, int DoctorID,
                                                     IDbContextTransaction transaction)
        {
            await dbcontext.Database.UseTransactionAsync(transaction.GetDbTransaction());
            if (PhoneNumbers == null || !PhoneNumbers.Any())
                throw new ArgumentException("Phone numbers list is empty.");

            List<DoctorPhoneNumberModel> DoctorPhones = PhoneNumbers.Select(Phone => new DoctorPhoneNumberModel
            {
                PhoneNumber = Phone,
                DoctorID = DoctorID,
                StatusID = 1
            }).ToList();
            await dbcontext.DoctorPhoneNumbers.AddRangeAsync(DoctorPhones);
            await dbcontext.SaveChangesAsync();
            return true;
        }
        /// <summary>
        /// Updates a doctor's phone numbers by comparing the existing list to the new list.
        /// Removes numbers no longer present and adds new ones, all within a transaction.
        /// </summary>
        /// <param name="newPhoneNumbers">The updated list of phone numbers.</param>
        /// <param name="DoctorID">The ID of the doctor whose phone numbers will be updated.</param>
        /// <param name="transaction">The active database transaction.</param>
        /// <returns><c>true</c> if the update was successful; otherwise, <c>false</c>.</returns>
        public async Task<bool> UpdatePhonesAsync(List<string> newPhoneNumbers, int DoctorID,
                                                 IDbContextTransaction transaction)
        {
            await dbcontext.Database.UseTransactionAsync(transaction.GetDbTransaction());

            if (newPhoneNumbers == null)
                newPhoneNumbers = new List<string>();

            // Get existing phone numbers 
            List<string> existingPhoneNumbers = await dbcontext.DoctorPhoneNumbers
                .Where(p => p.DoctorID == DoctorID && p.StatusID != 3)
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
                deleteSuccess = await DeletePhonesAsync(phonesToDelete, DoctorID, transaction);
            }

            // Add new phones
            if (phonesToAdd.Any())
            {
                addSuccess = await AddPhoneNumbersasync(phonesToAdd, DoctorID, transaction);
            }

            return deleteSuccess && addSuccess;
        }
        /// <summary>
        /// Soft-deletes the specified phone numbers for a given doctor by setting their status to deleted (StatusID = 3).
        /// </summary>
        /// <param name="PhoneNumbers">The list of phone numbers to delete.</param>
        /// <param name="DoctorID">The ID of the doctor whose phone numbers will be deleted.</param>
        /// <param name="transaction">The current database transaction.</param>
        /// <returns><c>true</c> if the phone numbers were marked as deleted successfully; otherwise, <c>false</c>.</returns>
        public async Task<bool> DeletePhonesAsync(List<String> PhoneNumbers, int DoctorID,
                                                  IDbContextTransaction transaction)
        {
            await dbcontext.Database.UseTransactionAsync(transaction.GetDbTransaction());
            foreach (var phoneNumber in PhoneNumbers)
            {
                await dbcontext.DoctorPhoneNumbers.Where(p => p.PhoneNumber.Equals(phoneNumber) &&
                p.StatusID != 3 && p.DoctorID == DoctorID)
                     .ExecuteUpdateAsync(s => s.SetProperty(p => p.StatusID, 3));
            }
            await dbcontext.SaveChangesAsync();
            return true;
        }
    }
}

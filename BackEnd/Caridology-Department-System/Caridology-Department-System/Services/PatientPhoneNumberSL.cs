using Caridology_Department_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Caridology_Department_System.Services
{
    public class PatientPhoneNumberSL
    {
        private readonly DBContext dbcontext;
        public PatientPhoneNumberSL(DBContext dBContext)
        {
            this.dbcontext = dBContext;
        }
        public async Task<bool> AddPhoneNumbersasync(List<string> PhoneNumbers, int PatientID,
                                                     IDbContextTransaction transaction)
        {
            await dbcontext.Database.UseTransactionAsync(transaction.GetDbTransaction());
            if (PhoneNumbers == null || !PhoneNumbers.Any())
                throw new ArgumentException("Phone numbers list is empty.");

            List<PatientPhoneNumberModel> PatientPhones = PhoneNumbers.Select(Phone => new PatientPhoneNumberModel
            {
                PhoneNumber = Phone,
                PatientID = PatientID,
                StatusID = 1
            }).ToList();
            await dbcontext.PatientPhoneNumbers.AddRangeAsync(PatientPhones);
            await dbcontext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdatePhonesAsync(List<string> newPhoneNumbers, int PatientID,
                                                 IDbContextTransaction transaction)
        {
            await dbcontext.Database.UseTransactionAsync(transaction.GetDbTransaction());

            if (newPhoneNumbers == null)
                newPhoneNumbers = new List<string>();

            // Get existing phone numbers 
            List<string> existingPhoneNumbers = await dbcontext.PatientPhoneNumbers
                .Where(p => p.PatientID == PatientID && p.StatusID != 3)
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
                deleteSuccess = await DeletePhonesAsync(phonesToDelete, PatientID, transaction);
            }

            // Add new phones
            if (phonesToAdd.Any())
            {
                addSuccess = await AddPhoneNumbersasync(phonesToAdd, PatientID, transaction);
            }

            return deleteSuccess && addSuccess;
        }
        public async Task<bool> DeletePhonesAsync(List<String> PhoneNumbers, int PatientID,
                                                  IDbContextTransaction transaction)
        {
            await dbcontext.Database.UseTransactionAsync(transaction.GetDbTransaction());
            foreach (var phoneNumber in PhoneNumbers)
            {
                await dbcontext.PatientPhoneNumbers.Where(p => p.PhoneNumber.Equals(phoneNumber) &&
                p.StatusID != 3 && p.PatientID == PatientID)
                     .ExecuteUpdateAsync(s => s.SetProperty(p => p.StatusID, 3));
            }
            await dbcontext.SaveChangesAsync();
            return true;
        }
    }
}

using Caridology_Department_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Caridology_Department_System.Services
{
    public class AdminPhoneNumberSL
    {
        private readonly  DBContext dbcontext;
        public AdminPhoneNumberSL(DBContext dbcontext)
        {
            this.dbcontext = dbcontext;
        }
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

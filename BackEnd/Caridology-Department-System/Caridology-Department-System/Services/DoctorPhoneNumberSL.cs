using AutoMapper;
using Caridology_Department_System.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;

namespace Caridology_Department_System.Services
{
    public class DoctorPhoneNumberSL
    {
        private readonly DBContext dbcontext;
        public DoctorPhoneNumberSL(DBContext dBContext)
        {
            this.dbcontext = dBContext;
        }
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

using Caridology_Department_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Caridology_Department_System.Services
{
    public class PatientPhoneNumberSL
    {
        private readonly DBContext dbcontext;

        public PatientPhoneNumberSL(DBContext db) => dbcontext = db;

        public async Task AddPhoneNumbersAsync(List<string> phoneNumbers, int patientId)
        {
            if (phoneNumbers == null || !phoneNumbers.Any())
                throw new ArgumentException("Phone numbers list cannot be empty");

            var existingNumbers = await dbcontext.PatientPhoneNumbers
                .Where(p => p.PatientID == patientId)
                .Select(p => p.PhoneNumber)
                .ToListAsync();

            var newNumbers = phoneNumbers
                .Except(existingNumbers) // Only add new ones
                .ToList();

            if (newNumbers.Any())
            {
                var phonesToAdd = newNumbers.Select(phone => new PatientPhoneNumberModel
                {
                    PhoneNumber = phone,
                    PatientID = patientId,
                    StatusID = 1
                }).ToList();

                await dbcontext.PatientPhoneNumbers.AddRangeAsync(phonesToAdd);
                await dbcontext.SaveChangesAsync();
            }
        }
        public async Task UpdatePhoneNumbersAsync(int patientId, List<string> phoneNumbers)
        {

            // Get current phone numbers
            var existingNumbers = await dbcontext.PatientPhoneNumbers
                .Where(p => p.PatientID == patientId)
                .ToListAsync();

            // Identify numbers to add, keep, and remove
            var currentNumbers = existingNumbers.Select(n => n.PhoneNumber).ToList();
            var numbersToAdd = phoneNumbers.Except(currentNumbers, StringComparer.OrdinalIgnoreCase).ToList();
            var numbersToRemove = currentNumbers.Except(phoneNumbers, StringComparer.OrdinalIgnoreCase).ToList();

            // Only make changes if there are differences
            if (!numbersToAdd.Any() && !numbersToRemove.Any())
                return;

            // Add new numbers
            if (numbersToAdd.Any())
            {
                var phonesToAdd = numbersToAdd.Select(phone => new PatientPhoneNumberModel
                {
                    PhoneNumber = phone,
                    PatientID = patientId,
                    StatusID = 1 // Assuming 1 is active status
                }).ToList();

                await dbcontext.PatientPhoneNumbers.AddRangeAsync(phonesToAdd);
            }

            // Remove numbers not in the provided list
            if (numbersToRemove.Any())
            {
                var phonesToRemove = existingNumbers
                    .Where(p => numbersToRemove.Contains(p.PhoneNumber, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                dbcontext.PatientPhoneNumbers.RemoveRange(phonesToRemove);
            }
        }
        public void DeletePhoneNumbers(int PatientID)
        {
                dbcontext.Database.ExecuteSqlInterpolated(
                $@"UPDATE ""PatientsPhoneNumbers"" 
                SET ""StatusID"" = {3}
                WHERE ""PatientID"" = {PatientID} AND ""StatusID"" <> {3}");
        }
    }
}

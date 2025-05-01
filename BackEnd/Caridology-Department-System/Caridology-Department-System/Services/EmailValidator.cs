using Caridology_Department_System.Models;
using Microsoft.EntityFrameworkCore;
using static Caridology_Department_System.Services.EmailValidator;

namespace Caridology_Department_System.Services
{
    public class EmailValidator
    {
        private readonly DBContext _db;

        public EmailValidator(DBContext db) => _db = db;

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            bool inPatients = await _db.Patients.AnyAsync(p => p.Email == email && p.StatusID !=3);
            bool inDoctors = await _db.Doctors.AnyAsync(d => d.Email == email && d.StatusID != 3 );
            bool inAdmins = await _db.Admins.AnyAsync(a => a.Email == email && a.StatusID != 3);

            return !(inPatients || inDoctors || inAdmins);
        }
    }
}

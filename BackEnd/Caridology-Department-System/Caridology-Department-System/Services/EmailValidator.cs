using Caridology_Department_System.Models;
using Microsoft.EntityFrameworkCore;
using static Caridology_Department_System.Services.EmailValidator;

namespace Caridology_Department_System.Services
{
    public class EmailValidator
    {
        private readonly DBContext _db;
        public EmailValidator(DBContext db) => _db = db;

        public  bool IsEmailUnique(string email)
        {
            bool existsInAdmins =  _db.Admins.Any(a => a.Email == email);
            bool existsInDoctors =  _db.Doctors.Any(d => d.Email == email);
            bool existsInPatients =  _db.Patients.Any(p => p.Email == email);
            return !(existsInAdmins || existsInDoctors || existsInPatients);
        }
    }
}

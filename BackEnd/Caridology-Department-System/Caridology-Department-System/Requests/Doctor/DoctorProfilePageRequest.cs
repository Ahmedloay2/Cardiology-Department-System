using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Caridology_Department_System.Models;

namespace Caridology_Department_System.Requests.Doctor
{
    public class DoctorProfilePageRequest
    {
        public int ID { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string FullName => $"{FName} {LName}";
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Position { get; set; }
        public int YearsOfExperience { get; set; }
        public string Gender { get; set; }
        public string? Address { get; set; }
        public List<string> phoneNumbers { get; set; }
        [NotMapped]
        public int Age => DateTime.UtcNow.Year - BirthDate.Year -
                 (BirthDate > DateTime.UtcNow.AddYears(-(DateTime.UtcNow.Year - BirthDate.Year)) ? 1 : 0);
        [Required]
        [ForeignKey(nameof(Status))]
        public int StatusID { get; set; }
        public StatusModel Status { get; set; }
        [Required]
        [ForeignKey(nameof(Role))]
        public int RoleID { get; set; }
        public RoleModel Role { get; set; }
        public string? PhotoData { get; set; }
    }
}

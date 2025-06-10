using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Caridology_Department_System.Models;

namespace Caridology_Department_System.Requests.Patient
{
    public class PatientProfilePageRequest
    {
        public string FName { get; set; }
        public string LName { get; set; }
        public string FullName => $"{FName} {LName}";
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string? LandLine { get; set; }
        public string? BloodType { get; set; }
        public string? Allergies { get; set; }
        public string? ChronicConditions { get; set; }
        public string? PreviousSurgeries { get; set; }
        public string? CurrentMedications { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string ParentName { get; set; }
        public string? SpouseName { get; set; }
        public string? PolicyNumber { get; set; }
        public string? InsuranceProvider { get; set; }
        public DateTime? PolicyValidDate { get; set; }
        [Required]
        [ForeignKey(nameof(Status))]
        public int StatusID { get; set; }
        public StatusModel Status { get; set; }
        [Required]
        [ForeignKey(nameof(Role))]
        public int RoleID { get; set; }
        public RoleModel Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Link { get; set; }
        public string? PhotoData { get; set; }
        [NotMapped]
        public int Age => DateTime.UtcNow.Year - BirthDate.Year -
         (BirthDate > DateTime.UtcNow.AddYears(-(DateTime.UtcNow.Year - BirthDate.Year)) ? 1 : 0);
        public List<string> phoneNumbers { get; set; }

    }

}

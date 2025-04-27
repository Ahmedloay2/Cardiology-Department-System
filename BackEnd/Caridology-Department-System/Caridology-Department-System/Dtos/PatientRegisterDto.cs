using System.ComponentModel.DataAnnotations;

namespace Caridology_Department_System.Dtos
{
    public class PatientRegisterDto
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string FName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string LName { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; }

        [Required]
        [StringLength(5)]
        public string BloodType { get; set; }

        public List<string> PhoneNumbers { get; set; } = new();
        public string? MedicalHistory { get; set; }
        public string? HealthInsuranceNumber { get; set; }
    }
}

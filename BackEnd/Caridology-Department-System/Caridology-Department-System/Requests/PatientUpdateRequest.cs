namespace Caridology_Department_System.Requests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class PatientUpdateRequest
    {
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string? FName { get; set; }

        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string? LName { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must contain at least one uppercase, one lowercase, one number and one special character")]
        public string? Password { get; set; }

        [StringLength(100)]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Please enter a valid email address"
        )]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? PhotoPath { get; set; }

        [StringLength(5)]
        [RegularExpression(@"^(A|B|AB|O)[+-]$",
             ErrorMessage = "Invalid blood type format (e.g., A+, O-)")]
        public string? BloodType { get; set; }

        //[StringLength(255)]
        //[Column(TypeName = "text")]
        //public string? MedicalHistory { get; set; }

        //[StringLength(255)]
        //[Column(TypeName = "text")]
        //public string? HealthInsuranceNumber { get; set; }

        [StringLength(50, MinimumLength = 2)]
        public string? EmergencyContactName { get; set; }

        [RegularExpression("^(?:\\+20|0)?1[0125]\\d{8}$", ErrorMessage = "Invalid phone number format.")]
        public string? EmergencyContactPhone { get; set; }

        [PhoneListValidation]
        public List<string>? PhoneNumbers { get; set; }

        public string? Gender { get; set; }

        public string? Link { get; set; }

        public string? ParentName { get; set; }

        public string? Address { get; set; }

        // Optional fields for completeness
        public string? SpouseName { get; set; }
        public string? LandLine { get; set; }
        public string? Allergies { get; set; }
        public string? ChronicConditions { get; set; }
        public string? PreviousSurgeries { get; set; }
        public string? CurrentMedications { get; set; }
        public string? PolicyNumber { get; set; }
        public string? insuranceProvider { get; set; }
        public DateTime? PolicyValidDate { get; set; }
    }


}

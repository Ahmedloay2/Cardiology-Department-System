namespace Caridology_Department_System.Requests.Patient
{
    using Swashbuckle.AspNetCore.Annotations;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class PatientUpdateRequest
    {
        [DefaultValue("")]
        [SwaggerSchema(Description = "Patient's first name")]
        [RegularExpression(@"^.{2,50}$", ErrorMessage = "First name must be 2-50 characters")]
        [Display(Name = "First Name")]
        public string? FName { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Patient's last name")]
        [RegularExpression(@"^.{2,50}$", ErrorMessage = "Last name must be 2-50 characters")]
        [Display(Name = "Last Name")]
        public string? LName { get; set; }

        [DefaultValue(null)]
        [SwaggerSchema(Description = "Patient's birth date")]
        public DateTime? BirthDate { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Email address")]
        [RegularExpression(@"^(?=.{1,100}$)[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Email must be valid and 1-100 characters.")]
        public string? Email { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Blood type")]
        [RegularExpression(@"^(A|B|AB|O)[+-]$", ErrorMessage = "Invalid blood type format (e.g., A+, O-)")]
        public string? BloodType { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Emergency contact name")]
        [RegularExpression(@"^.{2,50}$", ErrorMessage = "Emergency contact name must be 2-50 characters.")]
        public string? EmergencyContactName { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Emergency contact phone")]
        [RegularExpression(@"^(?:\+20|0)?1[0125]\d{8}$", ErrorMessage = "Invalid Egyptian phone number (e.g., +20123456789)")]
        public string? EmergencyContactPhone { get; set; }

        [DefaultValue(null)]
        [SwaggerSchema(Description = "Patient's phone numbers")]
        [PhoneListValidation]
        public List<string>? PhoneNumbers { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Gender")]
        [RegularExpression(@"^.{0,20}$", ErrorMessage = "Gender must be up to 20 characters.")]
        public string? Gender { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Link")]
        [RegularExpression(@"^.{0,255}$", ErrorMessage = "Link must be up to 255 characters.")]
        public string? Link { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Parent name")]
        [RegularExpression(@"^.{0,100}$", ErrorMessage = "Parent name must be up to 100 characters.")]
        public string? ParentName { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Address")]
        [RegularExpression(@"^.{0,500}$", ErrorMessage = "Address must be up to 500 characters.")]
        public string? Address { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Spouse name")]
        [RegularExpression(@"^.{0,100}$", ErrorMessage = "Spouse name must be up to 100 characters.")]
        public string? SpouseName { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Land line")]
        [RegularExpression(@"^.{0,20}$", ErrorMessage = "Land line must be up to 20 characters.")]
        public string? LandLine { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Allergies")]
        [RegularExpression(@"^.{0,255}$", ErrorMessage = "Allergies must be up to 255 characters.")]
        public string? Allergies { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Chronic conditions")]
        [RegularExpression(@"^.{0,255}$", ErrorMessage = "Chronic conditions must be up to 255 characters.")]
        public string? ChronicConditions { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Previous surgeries")]
        [RegularExpression(@"^.{0,255}$", ErrorMessage = "Previous surgeries must be up to 255 characters.")]
        public string? PreviousSurgeries { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Current medications")]
        [RegularExpression(@"^.{0,255}$", ErrorMessage = "Current medications must be up to 255 characters.")]
        public string? CurrentMedications { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Policy number")]
        [RegularExpression(@"^.{0,50}$", ErrorMessage = "Policy number must be up to 50 characters.")]
        public string? PolicyNumber { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Insurance provider")]
        [RegularExpression(@"^.{0,100}$", ErrorMessage = "Insurance provider must be up to 100 characters.")]
        public string? InsuranceProvider { get; set; }

        [DefaultValue(null)]
        [SwaggerSchema(Description = "Policy valid date")]
        public DateTime? PolicyValidDate { get; set; }
        [DefaultValue(null)]
        public IFormFile? PhotoData { get; set; }

    }
}

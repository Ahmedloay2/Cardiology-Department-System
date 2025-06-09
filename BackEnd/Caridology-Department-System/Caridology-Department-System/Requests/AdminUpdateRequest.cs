namespace Caridology_Department_System.Requests
{
    using Swashbuckle.AspNetCore.Annotations;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class AdminUpdateRequest
    {
        [DefaultValue("")]
        [SwaggerSchema(Description = "Admin's first name")]
        [RegularExpression(@"^.{2,50}$", ErrorMessage = "First name must be 2-50 characters")]
        [Display(Name = "First Name")]
        public string? FName { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Admin's last name")]
        [RegularExpression(@"^.{2,50}$", ErrorMessage = "Last name must be 2-50 characters")]
        [Display(Name = "Last Name")]
        public string? LName { get; set; }

        [DefaultValue(null)]
        [SwaggerSchema(Description = "Admin's birth date")]
        public DateTime? BirthDate { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Email address")]
        [RegularExpression(
    @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
    ErrorMessage = "Please enter a valid email address"
)]        public string? Email { get; set; }

        [DefaultValue(null)]
        [SwaggerSchema(
        Description = "List of admin phone numbers. Leave empty or null if no phones.",
        Nullable = true
        )]
        [PhoneListValidation]
        public List<string>? PhoneNumbers { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Address")]
        [RegularExpression(@"^.{0,500}$", ErrorMessage = "Address must be up to 500 characters.")]
        public string? Address { get; set; }
        [DefaultValue(null)]
        public IFormFile? PhotoData { get; set; }
        public string? Gender { get; set; }
    }
}

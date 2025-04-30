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
        [SwaggerSchema(Description = "Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,100}$",
            ErrorMessage = "Password must be 8-100 characters with at least one uppercase, one lowercase, one number, and one special character.")]
        public string? Password { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Email address")]
        [RegularExpression(
    @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
    ErrorMessage = "Please enter a valid email address"
)]        public string? Email { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Photo path")]
        [RegularExpression(@"^.{0,255}$", ErrorMessage = "Photo path must be up to 255 characters.")]
        public string? PhotoPath { get; set; }

        [DefaultValue(1)]
        [SwaggerSchema(Description = "Role ID")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid role ID")]
        public int? RoleID { get; set; }

        [DefaultValue(1)]
        [SwaggerSchema(Description = "Status ID")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid status ID")]
        public int? StatusID { get; set; }

        [DefaultValue(null)]
        [SwaggerSchema(Description = "Admin's phone numbers")]
        [PhoneListValidation]
        public List<string>? PhoneNumbers { get; set; }

        [DefaultValue("")]
        [SwaggerSchema(Description = "Address")]
        [RegularExpression(@"^.{0,500}$", ErrorMessage = "Address must be up to 500 characters.")]
        public string? Address { get; set; }
    }
}

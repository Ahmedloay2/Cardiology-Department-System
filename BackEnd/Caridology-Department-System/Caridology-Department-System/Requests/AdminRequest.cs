using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Caridology_Department_System.Requests
{
    public class AdminRequest
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be 2-50 characters")]
        public string FName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be 2-50 characters")]
        public string LName { get; set; }

        [Required(ErrorMessage = "Birth date is required")]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,100}$",
            ErrorMessage = "Password must contain 8-100 characters with at least one uppercase, lowercase, number, and special character")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(
    @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
    ErrorMessage = "Please enter a valid email address"
)]
        public string Email { get; set; }

        [StringLength(255, ErrorMessage = "Photo path cannot exceed 255 characters")]
        public string? PhotoPath { get; set; }

        [Required(ErrorMessage = "Role ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid role ID")]
        public int RoleID { get; set; } = 1; // Default to basic admin role

        [Required(ErrorMessage = "Status ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid status ID")]
        public int StatusID { get; set; } = 1; // Default to active

        [Required(ErrorMessage = "At least one phone number is required")]
        [PhoneListValidation]
        public List<string> PhoneNumbers { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Caridology_Department_System.Requests
{
    public class AdminRequest
    {
        public AdminRequest() { }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LName { get; set; }
        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        [StringLength(100)]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(
        @"^[a-zA-Z0-9._%+-]+@gmail\.com$",
        ErrorMessage = "Only Gmail accounts are allowed (@gmail.com)"
        )]
        public string Email { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must contain at least one uppercase, one lowercase, one number and one special character")]
        public string Password { get; set; }
        [StringLength(255)]
        public string? PhotoPath { get; set; }
        [Required]
        [PhoneListValidation]
        public List<string> PhoneNumbers { get; set; }
    }
}

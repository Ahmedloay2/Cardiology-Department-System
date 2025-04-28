using System.ComponentModel.DataAnnotations;

namespace Caridology_Department_System.Requests
{
    public class LoginRequest
    {
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

    }
}

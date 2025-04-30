using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Caridology_Department_System.Models
{
    [Table("Admins")]
    public class AdminModel : UserModel
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LName { get; set; }

        [NotMapped]
        public string FullName => $"{FName} {LName}";

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        [ForeignKey(nameof(Role))]
        public int RoleID { get; set; }
        public RoleModel Role { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must contain at least one uppercase, one lowercase, one number and one special character")]
        public string Password { get; set; }

        [Required]
        [StringLength(100)]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(
    @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
    ErrorMessage = "Please enter a valid email address"
)]
        public string Email { get; set; }

        [StringLength(255)]
        public string? PhotoPath { get; set; }

        [Required]
        [ForeignKey(nameof(Status))]
        public int StatusID { get; set; }
        public StatusModel Status { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<AdminPhoneNumberModel> PhoneNumbers { get; set; } = new List<AdminPhoneNumberModel>();

        // Optional: Address field if you want to match DTOs
        [StringLength(500)]
        public string? Address { get; set; }
    }
}

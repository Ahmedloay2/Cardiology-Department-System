using Caridology_Department_System.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class UserModel
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
    [Range(0, 120, ErrorMessage = "Age must be between 0 and 120.")]
    public int Age { get; set; }

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
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    [StringLength(255)]
    public string? PhotoPath { get; set; }

    [Required]
    [ForeignKey(nameof(Status))]
    public int StatusID { get; set; }
    public StatusModel Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
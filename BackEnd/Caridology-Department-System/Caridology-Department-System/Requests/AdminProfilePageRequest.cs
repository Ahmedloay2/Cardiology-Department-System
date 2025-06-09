using Caridology_Department_System.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Caridology_Department_System.ValdiationAttributes;

namespace Caridology_Department_System.Requests
{
    public class AdminProfilePageRequest
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
        [StringLength(255)]
        public string? PhotoPath { get; set; }

        [Required]
        [ForeignKey(nameof(Status))]
        public int StatusID { get; set; }
        public StatusModel Status { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<String> phoneNumbers { get; set; }
        [StringLength(500)]
        public string? Address { get; set; }
        public String? PhotoData { get; set; }
        [NotMapped]
        public int Age => DateTime.UtcNow.Year - BirthDate.Year -
                 (BirthDate > DateTime.UtcNow.AddYears(-(DateTime.UtcNow.Year - BirthDate.Year)) ? 1 : 0);
        [Required]
        public string Gender { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
namespace Caridology_Department_System.Models
{
    public class RoleModel
    {
        [Key]
        public int RoleID { get; set; }
        [Required]
        [StringLength(50)]
        public string RoleName { get; set; }

    }
}

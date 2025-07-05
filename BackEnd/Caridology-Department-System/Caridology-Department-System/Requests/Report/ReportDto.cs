using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Caridology_Department_System.Requests.Report
{
    public class ReportDto
    {
        [Required]
        [Range(1, int.MaxValue,ErrorMessage ="Enter a Valid ID")]
        public int appointmentID {  get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "prescription cannot be empty.")]
        [MinLength(10,ErrorMessage = "prescription can not be lower than 10 char")]
        [Column(TypeName = "text")]
        public string prescription { get; set; }

    }
}

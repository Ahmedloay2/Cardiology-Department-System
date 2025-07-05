using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Caridology_Department_System.Models
{
    public class ReportModel
    {
        [Key]
        public int ReportID { get; set; }
        [Required]
        [Column(TypeName = "text")]
        public string Prescription { get; set; } 
        [Required]
        [ForeignKey(nameof(Status))]
        public int StatusID { get; set; }
        public StatusModel Status { get; set; }
        [Required]
        [ForeignKey(nameof(Appointment))]
        public int AppointmentID { get; set; }
        public AppointmentModel Appointment { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } 
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }
        
    }

}


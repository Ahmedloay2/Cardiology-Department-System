using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Caridology_Department_System.Models
{
    public class MessageModel
    {
        [Key]
        public int MessageID { get; set; }
        [Required]
        [StringLength(50)]
        public string Sender { get; set; }
        [Required]
        [StringLength(50)]
        public string Receiver { get; set; }
        [Required]
        [Column(TypeName = "text")]
        public string Content { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DateTime { get; set; }
        [Required]
        [ForeignKey(nameof(Doctor))]
        public int DoctorID { get; set; }
        public DoctorModel Doctor {  get; set; }
        [Required]
        [ForeignKey(nameof(Patient))]
        public int PatientID { get; set; }
        public PatientModel Patient { get; set; }
        [Required]
        [ForeignKey(nameof(Status))]
        public int StatusID { get; set; }
        public StatusModel Status { get; set; }

    }
}

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
        [ForeignKey(nameof(Doctor))]
        public int DoctorID { get; set; }
        public DoctorModel Doctor { get; set; }
        [Required]
        [ForeignKey(nameof(Patient))]
        public int PatientID { get; set; }
        public PatientModel Patient { get; set; }
        [Required]
        [ForeignKey(nameof(Status))]
        public int StatusID { get; set; }
        public StatusModel Status { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        public ICollection<ReportScanModel> Scans { get; set; } = new List<ReportScanModel>();
    }

}


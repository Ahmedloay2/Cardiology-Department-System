using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Caridology_Department_System.Models
{
    public class ReportScanModel
    {
        [Key]
        public int ReportScanID { get; set; }
        [Required]
        [StringLength(255)]
        public string Scan {  get; set; }
        [Required]
        [ForeignKey(nameof(Report))]
        public int ReportID { get; set; }
        public ReportModel Report { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        [StringLength(100)]
        public string? OriginalFileName { get; set; } 

        [StringLength(50)]
        public string? FileType { get; set; }
        [Required]
        [ForeignKey(nameof(Status))]
        public int StatusID { get; set; }
        public StatusModel Status { get; set; }

    }
}

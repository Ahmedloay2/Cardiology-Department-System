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
        public string ScanURL {  get; set; }
        [Required]
        [ForeignKey(nameof(Report))]
        public int ReportID { get; set; }
        public ReportModel Report { get; set; }
        public DateTime ScannedAt{ get; set; } = DateTime.Now;
        [Required]
        [ForeignKey(nameof(Status))]
        public int StatusID { get; set; }
        public StatusModel Status { get; set; }

    }
}

using System.ComponentModel.DataAnnotations;

namespace Caridology_Department_System.Requests
{
    public class PatientStatusUpdateRequest
    {
        [Required]
        [Range(1, 3, ErrorMessage = "Status must be 1 (Active), 2 (Inactive), or 3 (Deleted)")]
        public int NewStatus { get; set; }
    }

}

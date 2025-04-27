using Caridology_Department_System.Models;

namespace Caridology_Department_System.Requests
{
    public class PatientRequest
    {
        public PatientRequest() { }
        public PatientModel Patient { get; set; }
        public List<String> PhoneNumbers { get; set; }
    }
}

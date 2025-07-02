namespace Caridology_Department_System.Requests.Appointment
{
    public class AppointmentDataRequest
    {
        public int AppID { get; set; }
        public DateTime Date { get; set; }
        public int DoctorID { get; set; }
        public string DoctorName { get; set; }  
        public int PatientID { get; set; }
        public string PatientName { get; set; }  
        public string Status { get; set; }
    }
}

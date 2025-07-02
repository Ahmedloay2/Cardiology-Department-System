using AutoMapper;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests.Appointment;
namespace Caridology_Department_System.AutoMappers
{
    public class AppointmentMapper: Profile
    {
        public AppointmentMapper()
        {
            CreateMap<AppointmentModel, AppointmentDataRequest>()
                .ForMember(a => a.PatientName, opt => opt.MapFrom(p => p.Patient.FullName))
                .ForMember(a => a.DoctorName, opt => opt.MapFrom(p => p.Doctor.FullName))
                .ForMember(a => a.Status, opt => opt.MapFrom(p => p.Status.Name));
        }
    }
}

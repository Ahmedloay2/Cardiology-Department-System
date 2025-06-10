using AutoMapper;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests.Doctor;

namespace Caridology_Department_System.AutoMappers
{
    public class DoctorMapper : Profile
    {
        public DoctorMapper()
        {   
            // DoctorRequest → DoctorModel
            CreateMap<DoctorRequest, DoctorModel>()
                .ForMember(dest => dest.ID, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.FullName, opt => opt.Ignore())
                .ForMember(dest => dest.PhotoPath, opt => opt.Ignore())
                .ForMember(dest => dest.StatusID, opt => opt.MapFrom(_ => 1))
                .ForMember(dest => dest.RoleID, opt => opt.MapFrom(_ => 2))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.PhoneNumbers, opt => opt.Ignore())
                .ForMember(dest => dest.Messages, opt => opt.Ignore())
                .ForMember(dest => dest.Appointments, opt => opt.Ignore())
                .ForMember(dest => dest.AuthoredReports, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore());

            // AdminModel → AdminProfilePageRequest
            CreateMap<DoctorModel, DoctorProfilePageRequest>()
                .ForMember(dest => dest.PhotoData, opt => opt.Ignore()) // Handle separately 
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                // Map phone numbers collection to list of strings
                .ForMember(dest => dest.phoneNumbers,
                    opt => opt.MapFrom(src => src.PhoneNumbers.Select(p => p.PhoneNumber).ToList()))
                // Map related entities
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role));



        }
    }
}

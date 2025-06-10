using AutoMapper;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Caridology_Department_System.Requests.Patient;

namespace Caridology_Department_System.AutoMappers
{
    public class PatientMapper: Profile
    {
        public PatientMapper() 
        {
            // PatientRequest → PatientModel
            CreateMap<PatientRequest, PatientModel>()
               .ForMember(dest => dest.ID, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ForMember(dest => dest.FullName, opt => opt.Ignore())
                .ForMember(dest => dest.PhotoPath, opt => opt.Ignore())
                .ForMember(dest => dest.StatusID, opt => opt.MapFrom(_ => 1))
                .ForMember(dest => dest.RoleID, opt => opt.MapFrom(_ => 3))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.PhoneNumbers, opt => opt.Ignore())
                .ForMember(dest => dest.Messages, opt => opt.Ignore())
                .ForMember(dest => dest.Appointments, opt => opt.Ignore())
                .ForMember(dest => dest.Reports, opt => opt.Ignore())
                .ForMember(dest => dest.Password, opt => opt.Ignore());

            // PatientModel → PatientProfilePageRequest
            CreateMap<PatientModel, PatientProfilePageRequest>()
                .ForMember(dest => dest.PhotoData, opt => opt.Ignore())
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.phoneNumbers,
                    opt => opt.MapFrom(src => src.PhoneNumbers.Select(p => p.PhoneNumber).ToList()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.Age, opt => opt.Ignore()); ;

            // PatientUpdateRequest → PatientModel
            CreateMap<PatientUpdateRequest, PatientModel>()
            // Ignore system properties
            .ForMember(dest => dest.ID, opt => opt.Ignore())
            .ForMember(dest => dest.Password, opt => opt.Ignore())
            .ForMember(dest => dest.RoleID, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.StatusID, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Appointments, opt => opt.Ignore())
            .ForMember(dest => dest.Messages, opt => opt.Ignore())
            .ForMember(dest => dest.Reports, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumbers, opt => opt.Ignore())
            .ForMember(dest => dest.FullName, opt => opt.Ignore())
            .ForMember(dest => dest.PhotoPath, opt => opt.Ignore()) // Handle separately
            .ForMember(dest => dest.Email, opt => opt.Ignore()) // Handle separately

            // Only map if source is not null/empty and different from destination
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember, destMember) =>
            {
                if (srcMember == null) return false;

                // Handle string properties
                if (srcMember is string str)
                {
                    return !string.IsNullOrWhiteSpace(str) && !str.Equals(destMember?.ToString());
                }

                // Handle DateTime properties (including nullable)
                if (srcMember.GetType() == typeof(DateTime?) || srcMember.GetType() == typeof(DateTime))
                {
                    if (srcMember is DateTime regularDateTime)
                    {
                        return regularDateTime > DateTime.MinValue && !regularDateTime.Equals(destMember);
                    }

                    // Handle nullable DateTime using GetType check
                    var nullableDateTime = srcMember as DateTime?;
                    if (nullableDateTime.HasValue && nullableDateTime.Value > DateTime.MinValue)
                    {
                        return !nullableDateTime.Equals(destMember);
                    }

                    return false;
                }

                // Handle other types
                return !srcMember.Equals(destMember);
            }));

        }
    }
}

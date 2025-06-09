using AutoMapper;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Caridology_Department_System.Services
{
    public class DoctorSL
    {
        private readonly DoctorPhoneNumberSL doctorPhoneNumberSL;
        private readonly IMapper mapper;
        private readonly PasswordHasher hasher;
        private readonly EmailValidator emailValidator;
        private readonly ImageService imageService;
        private readonly DBContext dbContext;
        public DoctorSL(DBContext dBContext, ImageService imageService,
                                   IMapper mapper, PasswordHasher passwordHasher, EmailValidator emailValidator,
                                   DoctorPhoneNumberSL doctorPhoneNumberSL)
        {
            this.dbContext = dBContext;
            this.mapper = mapper;
            this.hasher = passwordHasher;
            this.emailValidator = emailValidator;
            this.imageService = imageService;
            this.doctorPhoneNumberSL = doctorPhoneNumberSL;
        }
        public async Task<bool> AddAdminAsync(DoctorRequest request)
        {
            bool created=false;
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Admin data cannot be empty");
            }
            if (request.PhoneNumbers == null || !request.PhoneNumbers.Any())
            {
                created = false;
                throw new ArgumentException("At least one phone number is required", nameof(request.PhoneNumbers));
            }
            if (!emailValidator.IsEmailUniqueAsync(request.Email))
            {
                created = false;
                throw new ArgumentException("Email is already used");
            }
            var transaction =dbContext.Database.BeginTransaction();
            {
                DoctorModel doctor = mapper.Map<DoctorModel>(request);
                if (doctor == null)
                {
                    created = false;
                    throw new ArgumentException("error has occured");
                }
                created = doctorPhoneNumberSL.AddPhoneNumbersasync(request.Phone);
            }
            return true;
        }
    }
}

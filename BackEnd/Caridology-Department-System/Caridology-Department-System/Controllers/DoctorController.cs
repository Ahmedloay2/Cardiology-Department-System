using System.Security.Claims;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Caridology_Department_System.Requests.Doctor;
using Caridology_Department_System.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace Caridology_Department_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly DoctorSL doctorSL;
        private readonly JwtTokenService jwtTokenService;       
        public DoctorController(DoctorSL doctorSL, JwtTokenService jwtTokenService)
        {
            this.doctorSL = doctorSL;
            this.jwtTokenService = jwtTokenService;
        }
        [HttpPost("CreateDoctor")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> CreateDoctorAsync([FromForm] DoctorRequest doctor)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                bool Created = false;
                Created = await doctorSL.AddDoctorAsync(doctor);
                if (!Created)
                {
                    throw new Exception("error has occured");
                }
                var response = new ResponseWrapperDto
                {
                    Success = Created,
                    StatusCode = 200,
                    Message = "account created successfully"
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseWrapperDto
                {
                    Errors = ex.Message,
                    Success = false,
                    StatusCode = 400
                };
                return BadRequest(response);
            }
        }
        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync(LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                DoctorModel doctor = await doctorSL.GetDoctorByEmailAndPassword(request);
                var token = jwtTokenService.GenerateToken(doctor);
                return Ok(new
                {
                    Token = token,
                    Doctor = doctor
                });
            }
            catch (Exception ex)
            {
                var response = new ResponseWrapperDto
                {
                    Errors = ex.Message,
                    Success = false,
                    StatusCode = 400
                };
                return BadRequest(response);
            }
        }
        [HttpGet("Profile")]
        public async Task<IActionResult> GetProfilePageAsync(int? ID)
        {
            try
            {
                int doctorid;
                if (!ID.HasValue || ID > 0)
                {
                     doctorid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }
                else
                {
                    doctorid = ID.Value;
                }
                    DoctorProfilePageRequest doctor = await doctorSL.GetDoctorProfilePage(doctorid);
                return Ok(doctor);
            }
            catch (Exception ex)
            {
                var response = new ResponseWrapperDto
                {
                    Errors = ex.Message,
                    Success = false,
                    StatusCode = 400
                };
                return BadRequest(response);
            }            
        }
        [HttpPut("UpdateProfile")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task <IActionResult> UpdateProfileAsync([FromForm] DoctorUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                int doctorid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                bool Updated = await doctorSL.UpdateProfileAsync(doctorid, request);
                if (!Updated)
                {
                    throw new Exception("error has occured");
                }
                var response = new ResponseWrapperDto
                {
                    Message = "Account updated successfully",
                    Success = Updated,
                    StatusCode = 200,
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseWrapperDto
                {
                    Errors = ex.Message,
                    Success = false,
                    StatusCode = 400,
                };
                return BadRequest(response);
            }

        }
        [HttpPost("Logout")]
        public  IActionResult Logout()
        {
            var response = new ResponseWrapperDto
            {
                Success = true,
                StatusCode = 200,
                Message = "Logout Successfully"
            };
            return Ok(response);
        }
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteAccountAsync()
        {
            try
            {
                int adminID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                bool deleted = await doctorSL.DeleteDoctorAsync(adminID);
                if (!deleted)
                {
                    throw new Exception("error has occured");
                }
                var response = new ResponseWrapperDto
                {
                    Message = "account deleted successfuly",
                    Success = true,
                    StatusCode = 200
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseWrapperDto
                {
                    Errors = ex.Message,
                    Success = false,
                    StatusCode = 400
                };
                return BadRequest(response);
            }
        }
        [HttpGet("DoctorsProfileList")]
        public async Task<IActionResult> GetDoctorsProfilePageAsync([FromQuery] string? name
                                ,[FromQuery] int pagenumber=1, [FromQuery] bool exactmatch=false)
        {
            try
            {
                if (pagenumber < 1)
                {
                    var response = new ResponseWrapperDto
                    {
                        StatusCode = 400,
                        Success = false,
                        Message = "pagenumber must be positive integers"
                    };
                    return BadRequest(response);
                }
                List<DoctorProfilePageRequest> doctors = await doctorSL.GetDoctorsProfilePerPage(name,pagenumber,exactmatch);
                if (doctors == null || !doctors.Any())
                {
                    var response = new ResponseWrapperDto
                    {
                        StatusCode = 404,
                        Success = false,
                        Message = "no doctors found"
                    };
                    return NotFound(response);
                }
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                var response = new ResponseWrapperDto
                {
                    StatusCode = 500,
                    Success = false,
                    Message = "An unexpected error occurred",
                    Errors = ex.Message
                };
                return StatusCode(500,response);
            }
        }
        [HttpGet("DoctorsList")]
        public async Task<IActionResult> GetDoctorsPerPageAsync([FromQuery] string? name
                                , [FromQuery] int pagenumber = 1, [FromQuery] bool exactmatch=false)
        {
            try
            {
                if (pagenumber < 1)
                {
                    var response = new ResponseWrapperDto
                    {
                        StatusCode = 400,
                        Success = false,
                        Message = "pagenumber must be positive integers"
                    };
                    return BadRequest(response);
                }
            List <DoctorModel> doctors = await doctorSL.GetDoctorsPerPage(name,pagenumber,exactmatch);
                if (doctors == null || !doctors.Any())
                {
                    var response = new ResponseWrapperDto
                    {
                        StatusCode = 404,
                        Success = false,
                        Message = "no doctors found"
                    };
                    return NotFound(response);
                }
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                var response = new ResponseWrapperDto
                {
                    StatusCode = 500,
                    Success = false,
                    Message = "An unexpected error occurred",
                    Errors = ex.Message
                };
                return StatusCode(500, response);
            }
        }
        
    }
}

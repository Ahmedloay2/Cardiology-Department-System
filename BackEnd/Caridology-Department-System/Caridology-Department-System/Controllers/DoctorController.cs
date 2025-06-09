using System.Security.Claims;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
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
        public async Task<IActionResult> Login(LoginRequest request)
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
        public async Task<IActionResult> GetProfilePageAsync()
        {
            try
            {
                int doctorid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
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
    }
}

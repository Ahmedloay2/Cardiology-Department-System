using System.Security.Claims;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Caridology_Department_System.Requests.Patient;
using Caridology_Department_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Caridology_Department_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly JwtTokenService jwtTokenService;
        private readonly PatientSL PatientSL;

        public PatientController(JwtTokenService tokenService, PatientSL patientService)
        {
            jwtTokenService = tokenService;
            PatientSL = patientService;
        }
        [HttpPost("Register")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> CreatePatientAsync([FromForm] PatientRequest Patient)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                bool Created = false;
                Created = await PatientSL.AddPatientAsync(Patient);
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
                PatientModel Patient = await PatientSL.GetPatientByEmailAndPassword(request);
                var token = jwtTokenService.GenerateToken(Patient);
                return Ok(new
                {
                    Token = token,
                    patient = Patient
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
                int Patientid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                PatientProfilePageRequest Patient = await PatientSL.GetPatientProfilePage(Patientid);
                return Ok(Patient);
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
        public async Task<IActionResult> UpdateProfileAsync([FromForm] PatientUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                int Patientid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                bool Updated = await PatientSL.UpdateProfileAsync(Patientid, request);
                if (!Updated)
                {
                    throw new Exception("there is not data to update");
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
        public IActionResult Logout()
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
                bool deleted = await PatientSL.DeletePatientAsync(adminID);
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
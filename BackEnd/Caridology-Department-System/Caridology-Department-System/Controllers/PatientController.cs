using System.Security.Claims;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Caridology_Department_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Caridology_Department_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly JwtTokenService _tokenService;
        private readonly PatientSL _patientService;

        public PatientController(JwtTokenService tokenService, PatientSL patientService)
        {
            _tokenService = tokenService;
            _patientService = patientService;
        }

        [HttpGet("Profile")]
        [Authorize]
        public async Task<IActionResult> GetPatientProfile()
        {
            try
            {
                var patientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var patient =  _patientService.GetPatientByID(patientId);

                if (patient == null)
                    return NotFound(new { Message = "Patient not found" });

                return Ok(new
                {
                    patient.ID,
                    patient.FName,
                    patient.LName,
                    patient.Email,
                    patient.BirthDate,
                    patient.BloodType,
                    patient.Gender,
                    patient.PhotoPath,
                    patient.Address,
                    PhoneNumbers = patient.PhoneNumbers?.Select(pn => pn.PhoneNumber).ToList() ?? new List<string>(),
                    patient.EmergencyContactName,
                    patient.EmergencyContactPhone,
                    patient.ParentName,
                    patient.Link,
                    patient.SpouseName,
                    patient.LandLine,
                    patient.Allergies,
                    patient.ChronicConditions,
                    patient.PreviousSurgeries,
                    patient.CurrentMedications,
                    patient.PolicyNumber,
                    patient.InsuranceProvider,
                    patient.PolicyValidDate
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving patient profile", Error = ex.Message });
            }
        }

        [HttpPut("Profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] PatientRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var patientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                await _patientService.UpdateProfile(patientId, request, request.PhoneNumbers);

                return Ok(new { Success = true, Message = "Profile updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Email"))
            {
                return Conflict(new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] PatientRequest patient)
        {
            try
            {
                if (patient == null)
                    return BadRequest(new { Message = "Patient data is required" });

                await _patientService.AddPatientAsync(patient, patient.PhoneNumbers ?? new List<string>());
                return Ok(new { Message = "Patient registered successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred during registration", Error = ex.Message });
            }
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors) });

            try
            {
                PatientModel patient =  _patientService.GetPatientByEmailAndPassword(request.Email,request.Password);

                if (patient == null)
                    return Unauthorized(new { Message = "Invalid credentials" });

                var token = _tokenService.GenerateToken(patient);

                return Ok(new
                {
                    Token = token,
                    patient.ID,
                    patient.Email,
                    patient.FName,
                    patient.LName
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred during login", Error = ex.Message });
            }
        }

        [HttpPost("Logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // With JWT, logout is handled client-side by removing the token
            return Ok(new { Message = "Logged out successfully - please remove your token client-side" });
        }

        [HttpDelete("Delete")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                var patientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                 _patientService.DeletePatient(patientId);

                return Ok(new { Message = "Account deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while deleting account", Error = ex.Message });
            }
        }
    }
}
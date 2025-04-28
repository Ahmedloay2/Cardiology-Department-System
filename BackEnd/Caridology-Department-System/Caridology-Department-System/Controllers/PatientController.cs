using Azure.Core;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Caridology_Department_System.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Caridology_Department_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        [HttpGet("Profile")]
        public IActionResult GetPatientProfile()
        {
            PatientSL patientSL = new PatientSL();

            var patientId = HttpContext.Session.GetInt32("PatientId");

            if (patientId == null)
                return Unauthorized("Not logged in");

            PatientModel patient = patientSL.GetPatientByID(patientId);

            if (patient == null)
                return NotFound("Patient not found");

            // Return only what the profile page needs
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
                patient.PhoneNumbers,
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
                //patient.HealthInsuranceNumber,
                patient.PolicyNumber,
                patient.insuranceProvider,
                patient.PolicyValidDate
            });
        }
        [HttpPost("register")]
        public IActionResult Register([FromBody] PatientRequest patient)
        {
            try
            {
                if (patient == null || patient.PhoneNumbers == null)
                    return BadRequest("patient Data is required");
                PatientSL patientSL = new PatientSL();
                patientSL.AddPatient(patient,patient.PhoneNumbers);
                return Ok(new { Message = "Patient registered successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            PatientSL patientSL= new PatientSL();
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                PatientModel patient = patientSL.GetPatientByEmailAndPassword(request.Email, request.Password);

                // Create session cookie with patient ID
                HttpContext.Session.SetInt32("PatientId", patient.ID);

                // Return only essential data for immediate client needs
                var response = new
                {
                    patient.ID,
                    patient.FName,
                    patient.LName,
                    patient.Email
                };

                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }
        }
    }
}

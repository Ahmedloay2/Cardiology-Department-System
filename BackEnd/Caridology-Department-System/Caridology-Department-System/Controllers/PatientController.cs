// Required namespaces for the controller functionality
using Azure.Core;                                   // Azure SDK core functionality
using Caridology_Department_System.Models;          // Data models used by the system
using Caridology_Department_System.Requests;        // Request models for API endpoints
using Caridology_Department_System.Services;        // Service layer classes
using Microsoft.AspNetCore.Http;                    // HTTP context and session functionality
using Microsoft.AspNetCore.Mvc;                     // ASP.NET Core MVC components
using Org.BouncyCastle.Asn1.Ocsp;                   // Cryptography library component (might be unused)

namespace Caridology_Department_System.Controllers
{
    /// <summary>
    /// Controller that handles all patient-related API endpoints including
    /// registration, authentication, profile management, etc.
    /// </summary>
    [Route("api/[controller]")]                     // Routes all requests to /api/Patient/[endpoint]
    [ApiController]                                 // Marks this class as an API controller
    public class PatientController : ControllerBase
    {
        /// <summary>
        /// Gets the profile information for the currently authenticated patient
        /// </summary>
        /// <returns>Patient profile data for the authenticated user</returns>
        [HttpGet("Profile")]
        public IActionResult GetPatientProfile()
        {
            // Initialize the patient service layer
            PatientSL patientSL = new PatientSL();

            // Retrieve the patient ID from the current session
            var patientId = HttpContext.Session.GetInt32("PatientId");

            // Check if the patient is logged in
            if (patientId == null)
                return Unauthorized("Not logged in");

            // Retrieve the patient data using the ID from the session
            PatientModel patient = patientSL.GetPatientByID(patientId);

            // Verify that the patient exists in the database
            if (patient == null)
                return NotFound("Patient not found");

            // Return only the necessary profile information
            // This creates an anonymous object with only the needed properties
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
                //patient.HealthInsuranceNumber,     // Commented out, not included in response
                patient.PolicyNumber,
                patient.insuranceProvider,
                patient.PolicyValidDate
            });
        }

        /// <summary>
        /// Updates the profile information for the currently authenticated patient
        /// </summary>
        /// <param name="request">The profile data to update</param>
        /// <returns>Success message or error details</returns>
        [HttpPut("Profile")]
        public IActionResult UpdateProfile([FromBody] PatientUpdateRequest request)
        {
            try
            {
                // Get the authenticated patient's ID from the session
                var patientId = HttpContext.Session.GetInt32("PatientId");

                // Check if the patient is logged in
                if (patientId == null)
                    return Unauthorized("Not logged in");

                // Initialize the patient service layer
                PatientSL patientSL = new PatientSL();

                // Update the patient profile with the provided information
                patientSL.UpdateProfile(patientId.Value, request, request.PhoneNumbers);

                // Return success response
                return Ok(new { Message = "Profile updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                // Return a 404 Not Found if the patient doesn't exist
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // Return a 400 Bad Request for other errors
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Registers a new patient in the system
        /// </summary>
        /// <param name="patient">Patient registration data</param>
        /// <returns>Success message or error details</returns>
        [HttpPost("register")]
        public IActionResult Register([FromBody] PatientRequest patient)
        {
            try
            {
                // Validate that patient data and phone numbers are provided
                if (patient == null || patient.PhoneNumbers == null)
                    return BadRequest("patient Data is required");

                // Initialize the patient service layer
                PatientSL patientSL = new PatientSL();

                // Add the new patient to the database
                patientSL.AddPatient(patient, patient.PhoneNumbers);

                // Return success response
                return Ok(new { Message = "Patient registered successfully" });
            }
            catch (Exception ex)
            {
                // Return a 400 Bad Request for any errors
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Authenticates a patient and creates a session
        /// </summary>
        /// <param name="request">Login credentials (email and password)</param>
        /// <returns>Basic patient information upon successful login</returns>
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Initialize the patient service layer
            PatientSL patientSL = new PatientSL();

            try
            {
                // Validate the request model
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Authenticate the patient with the provided credentials
                PatientModel patient = patientSL.GetPatientByEmailAndPassword(request.Email, request.Password);

                // Create session with the patient's ID
                HttpContext.Session.SetInt32("PatientId", patient.ID);

                // Return only essential patient data for client needs
                var response = new
                {
                    patient.ID,
                    patient.FName,
                    patient.LName,
                    patient.Email
                };

                // Return success with basic patient info
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                // Return 401 Unauthorized if credentials are invalid
                return Unauthorized(new { message = "Invalid email or password" });
            }
        }

        /// <summary>
        /// Logs out the currently authenticated patient by clearing their session.
        /// </summary>
        /// <returns>HTTP 200 OK with a logout confirmation message.</returns>
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            // Check if the user was logged in by looking for the PatientId in session
            bool wasLoggedIn = HttpContext.Session.TryGetValue("PatientId", out _);

            // Clear all session data
            HttpContext.Session.Clear();

            // If using cookie authentication (this line is commented out as it's not currently configured)
            // await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Return appropriate success message based on whether user was logged in
            return Ok(new
            {
                message = wasLoggedIn
                    ? "Logout successful"
                    : "No active session found"
            });
        }
    }
}
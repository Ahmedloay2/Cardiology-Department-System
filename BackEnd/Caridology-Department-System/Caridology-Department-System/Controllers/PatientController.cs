using System.Security.Claims;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Caridology_Department_System.Requests.Patient;
using Caridology_Department_System.Services;
using Microsoft.AspNetCore.Mvc;

namespace Caridology_Department_System.Controllers
{
    /// <summary>
    /// Controller responsible for handling patinet-related HTTP requests.
    /// It acts as an entry point for the API endpoints related to patient functionalities.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly JwtTokenService jwtTokenService;
        private readonly PatientSL patientSL;

        /// <summary>
        ///  Initializes a new instance of the <see cref="PatientController"/> class with the specified service layer.
        /// </summary>
        /// <param name="patientService">service layer that is responsible to handle doctor logic</param>
        /// <param name="tokenService">web jason token sevice layer that is responsible of generating token</param>
        public PatientController(JwtTokenService tokenService, PatientSL patientService)
        {
            jwtTokenService = tokenService;
            patientSL = patientService;
        }

        /// <summary>
        /// Creates a new patietn account using the provided form data.
        /// </summary>
        /// <param name="Patient">
        /// The <see cref="PatientRequest"/> object containing the patient's details, including personal information,
        /// credentials, and optionally a profile image. Submitted as multipart/form-data.
        /// </param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <c>ResponseWrapperDto</c> with:
        /// - A <c>200 OK</c> status if the account is successfully created.
        /// - A <c>400 Bad Request</c> status if validation fails or an exception occurs.
        /// </returns>
        /// <remarks>
        /// This endpoint accepts multipart/form-data and is limited to 10 MB in request size.
        /// It validates the input model before attempting to create the patient account.
        /// </remarks>
        /// <response code="200">patient account created successfully</response>
        /// <response code="400">Validation failed or a general exception occurred</response>
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
                Created = await patientSL.AddPatientAsync(Patient);
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

        /// <summary>
        /// Authenticates a patient user based on provided credentials and returns a JWT if successful.
        /// </summary>
        /// <param name="request">
        /// The login request containing the patient's email and password. Both fields are required.
        /// </param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <c>ResponseWrapperDto</c> with:
        /// - A <c>200 OK</c> status, including the authenticated patient's data and a JWT token.
        /// - A <c>400 Bad Request</c> status if the credentials are invalid or if a general error occurs.
        /// </returns>
        /// <remarks>
        /// This endpoint is intended for patient users only. 
        /// On successful authentication, a signed JSON Web Token (JWT) is returned for use in subsequent requests.
        /// The credentials are matched against the patient records stored in the database.
        /// </remarks>
        /// <response code="200">Authentication successful; returns patient info and JWT token</response>
        /// <response code="400">Invalid credentials, validation failed, or a general exception occurred</response>
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                PatientModel Patient = await patientSL.GetPatientByEmailAndPassword(request);
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

        /// <summary>
        /// Retrieves a patient's profile information based on the provided ID or the JWT token of the authenticated user.
        /// </summary>
        /// <param name="ID">
        /// The optional ID of the patient whose profile is being requested. 
        /// If not provided and the authenticated user is not an admin, the ID is extracted from the JWT claims.
        /// </param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <c>ResponseWrapperDto</c> with:
        /// - A <c>200 OK</c> status and the patietn's profile data if retrieval is successful.
        /// - A <c>400 Bad Request</c> status if the ID is invalid, negative, or if an error occurs during processing.
        /// </returns>
        /// <remarks>
        /// This endpoint can be accessed by authenticated users with the "Patient" or "Admin" role.
        /// If the user is not an admin and no ID is provided, the patient's ID is automatically extracted from the JWT token.
        /// Admins may supply an explicit patient ID via query to retrieve any patient's profile.
        /// </remarks>
        /// <response code="200">Retrieval successful; returns patient profile data</response>
        /// <response code="400">Invalid ID or a general exception occurred</response>
        [HttpGet("Profile")]
        public async Task<IActionResult> GetProfilePageAsync([FromQuery]int? ID)
        {
            try
            {
                int PatientID;
                string Role = User.FindFirstValue(ClaimTypes.Role);
                if (!ID.HasValue && !Role.Equals("Admin"))
                {
                    PatientID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }
                else
                {
                    if(ID < 1)
                    {
                        var response = new ResponseWrapperDto
                        {
                            Message = "Id must be positive number",
                            Success = false,
                            StatusCode = 400
                        };
                        return BadRequest(response);
                    }
                    PatientID = ID.Value;
                }
                PatientProfilePageRequest Patient = await patientSL.GetPatientProfilePage(PatientID);
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

        /// <summary>
        /// Update a patient account using the provided form data
        /// </summary>
        /// <param name="request">
        /// The <see cref="PatientUpdateRequest"/> object containing the updated patient details
        /// </param>
        /// <returns>        
        /// - A <c>200 OK</c> status if the account is successfully updated.
        /// - A <c>400 Bad Request</c> status if validation fails or an exception occurs.</returns>
        /// <remarks>
        /// This endpoint accepts multipart/form-data and is limited to 10 MB in request size.
        /// It validates the input model before attempting to update the patient account.
        /// </remarks>
        /// <response code="200">patient account updated successfully</response>
        /// <response code="400">Validation failed or a general exception occurred</response>
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
                bool Updated = await patientSL.UpdateProfileAsync(Patientid, request);
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

        /// <summary>
        /// Logs the currently authenticated patient out of their account.
        /// </summary>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <c>ResponseWrapperDto</c> with:
        /// - A <c>200 OK</c> status if the logout process completes successfully.
        /// </returns>
        /// <remarks>
        /// This endpoint simply returns a success response. 
        /// Token invalidation or session cleanup should be handled on the client side (e.g., by removing the JWT).
        /// </remarks>
        /// <response code="200">Logout successful</response>
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

        /// <summary>
        /// Deletes the currently authenticated patient's account or, if the user is an admin, deletes a specified doctor's account.
        /// </summary>
        /// <param name="ID">
        /// The optional ID of the patient whose profile is being requested. 
        /// If not provided and the authenticated user is not an admin, the ID is extracted from the JWT claims.
        /// </param>
        /// <returns>
        /// - A <c>200 OK</c> status if the account is successfully deleted.
        /// - A <c>400 Bad Request</c> status if validation fails or an exception occurs. 
        /// </returns>
        /// <remarks>
        /// This endpoint can be accessed by users with either the "Patient" or "Admin" role.
        /// - Patients can only delete their own account (ID is taken from JWT).
        /// - Admins can delete any patient account by specifying the patient ID in the query.
        /// </remarks>
        /// <response code="200">Admin account deleted successfully</response>
        /// <response code="400">general exception occurred</response>
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteAccountAsync([FromQuery]int? ID)
        {
            try
            {
                int PatientID;
                string Role = User.FindFirstValue(ClaimTypes.Role);
                if (!ID.HasValue && !Role.Equals("Admin"))
                {
                    PatientID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }
                else
                {
                    if (ID < 1)
                    {
                        var response2 = new ResponseWrapperDto
                        {
                            Message = "Id must be positive number",
                            Success = false,
                            StatusCode = 400
                        };
                        return BadRequest(response2);
                    }
                    PatientID = ID.Value;
                }
                bool deleted = await patientSL.DeletePatientAsync(PatientID);
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

        /// <summary>
        /// Retrieves a paginated list of patient profile pages, 10 per page.
        /// </summary>
        /// <param name="name">
        /// Optional name filter to search for patients.
        /// </param>
        /// <param name="pagenumber">
        /// The page number to retrieve (must be a positive integer).
        /// </param>
        /// <param name="exactmatch">
        /// If true, performs an exact match on the patient name; otherwise, performs a partial match.
        /// </param>
        /// <returns>
        /// - A <c>200 OK</c> response with a list of patient profiles if found.
        /// - A <c>404 Not Found</c> if no matching patients are found.
        /// - A <c>400 Bad Request</c> if the page number is invalid.
        /// - A <c>500 Internal Server Error</c> if an unexpected error occurs.
        /// </returns>
        /// <response code="200">A list of patient profiles was found and returned successfully.</response>
        /// <response code="404">No matching patient profiles were found.</response>
        /// <response code="400">The page number provided is invalid.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        [HttpGet("PatientProfilesList")]
        public async Task<IActionResult> GetPatientsProfilePageAsync([FromQuery] string? name
                        , [FromQuery] int pagenumber = 1, [FromQuery] bool exactmatch = false)
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
                List<PatientProfilePageRequest> Patients = await patientSL.GetPatientsProfilePerPageAsync(name, pagenumber, exactmatch);
                if (Patients == null || !Patients.Any())
                {
                    var response = new ResponseWrapperDto
                    {
                        StatusCode = 404,
                        Success = false,
                        Message = "no Patients found"
                    };
                    return NotFound(response);
                }
                return Ok(Patients);
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

        /// <summary>
        /// Retrieves a paginated list of patient accounts, 10 per page.
        /// </summary>
        /// <param name="name">
        /// Optional name filter to search for patients.
        /// </param>
        /// <param name="pagenumber">
        /// The page number to retrieve (must be a positive integer).
        /// </param>
        /// <param name="exactmatch">
        /// If true, performs an exact match on the patient name; otherwise, performs a partial match.
        /// </param>
        /// <returns>
        /// - A <c>200 OK</c> response with a list of patients if found.
        /// - A <c>404 Not Found</c> if no matching patients are found.
        /// - A <c>400 Bad Request</c> if the page number is invalid.
        /// - A <c>500 Internal Server Error</c> if an unexpected error occurs.
        /// </returns>
        /// <response code="200">A list of patients was found and returned successfully.</response>
        /// <response code="404">No matching patients are found.</response>
        /// <response code="200">The page number is invalid.</response>
        /// <response code="500">An unexpected error occurs.</response>
        [HttpGet("PatientsList")]
        public async Task<IActionResult> GetPatientsPerPageAsync([FromQuery] string? name
                                , [FromQuery] int pagenumber = 1, [FromQuery] bool exactmatch = false)
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
                List<PatientModel> Patients = await patientSL.GetPatientsPerPageAsync(name, pagenumber, exactmatch);
                if (Patients == null || !Patients.Any())
                {
                    var response = new ResponseWrapperDto
                    {
                        StatusCode = 404,
                        Success = false,
                        Message = "no Patients found"
                    };
                    return NotFound(response);
                }
                return Ok(Patients);
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
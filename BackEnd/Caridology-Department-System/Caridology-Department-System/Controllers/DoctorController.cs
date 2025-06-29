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
    /// <summary>
     /// Controller responsible for handling doctor-related HTTP requests.
     /// It acts as an entry point for the API endpoints related to doctor functionalities.
     /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly DoctorSL doctorSL;
        private readonly JwtTokenService jwtTokenService;

        /// <summary>
        ///  Initializes a new instance of the <see cref="DoctorController"/> class with the specified service layer.
        /// </summary>
        /// <param name="doctorSL">service layer that is responsible to handle doctor logic</param>
        /// <param name="jwtTokenService">web jason token sevice layer that is responsible of generating token</param>
        public DoctorController(DoctorSL doctorSL, JwtTokenService jwtTokenService)
        {
            this.doctorSL = doctorSL;
            this.jwtTokenService = jwtTokenService;
        }

        /// <summary>
        /// Creates a new doctor account using the provided form data.
        /// </summary>
        /// <param name="doctor">
        /// The <see cref="DoctorRequest"/> object containing the doctor's details, including personal information,
        /// credentials, and optionally a profile image. Submitted as multipart/form-data.
        /// </param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <c>ResponseWrapperDto</c> with:
        /// - A <c>200 OK</c> status if the account is successfully created.
        /// - A <c>400 Bad Request</c> status if validation fails or an exception occurs.
        /// </returns>
        /// <remarks>
        /// This endpoint accepts multipart/form-data and is limited to 10 MB in request size.
        /// It validates the input model before attempting to create the doctor account.
        /// </remarks>
        /// <response code="200">doctor account created successfully</response>
        /// <response code="400">Validation failed or a general exception occurred</response>
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

        /// <summary>
        /// Authenticates a doctor user based on provided credentials and returns a JWT if successful.
        /// </summary>
        /// <param name="request">
        /// The login request containing the doctor's email and password. Both fields are required.
        /// </param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <c>ResponseWrapperDto</c> with:
        /// - A <c>200 OK</c> status, including the authenticated codtor's data and a JWT token.
        /// - A <c>400 Bad Request</c> status if the credentials are invalid or if a general error occurs.
        /// </returns>
        /// <remarks>
        /// This endpoint is intended for doctor users only. 
        /// On successful authentication, a signed JSON Web Token (JWT) is returned for use in subsequent requests.
        /// The credentials are matched against the doctor records stored in the database.
        /// </remarks>
        /// <response code="200">Authentication successful; returns doctor info and JWT token</response>
        /// <response code="400">Invalid credentials, validation failed, or a general exception occurred</response>
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

        /// <summary>
        /// Retrieves a doctor's profile information based on the provided ID or the JWT token of the authenticated user.
        /// </summary>
        /// <param name="ID">
        /// The optional ID of the doctor whose profile is being requested. 
        /// If not provided and the authenticated user is not an admin, the ID is extracted from the JWT claims.
        /// </param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <c>ResponseWrapperDto</c> with:
        /// - A <c>200 OK</c> status and the doctor's profile data if retrieval is successful.
        /// - A <c>400 Bad Request</c> status if the ID is invalid, negative, or if an error occurs during processing.
        /// </returns>
        /// <remarks>
        /// This endpoint can be accessed by authenticated users with the "Doctor" or "Admin" role.
        /// If the user is not an admin and no ID is provided, the doctor's ID is automatically extracted from the JWT token.
        /// Admins may supply an explicit doctor ID via query to retrieve any doctor's profile.
        /// </remarks>
        /// <response code="200">Retrieval successful; returns doctor profile data</response>
        /// <response code="400">Invalid ID or a general exception occurred</response>
        [HttpGet("Profile")]
        public async Task<IActionResult> GetProfilePageAsync([FromQuery]int? ID)
        {
            try
            {
                int DoctorID;
                string Role = User.FindFirstValue(ClaimTypes.Role);
                if (!ID.HasValue && !Role.Equals("Admin"))
                {
                    DoctorID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }
                else
                {
                    if (ID < 1)
                    {
                        var response = new ResponseWrapperDto
                        {
                            Message = "ID must be positive number",
                            Success = false,
                            StatusCode = 400
                        };
                        return BadRequest(response);
                    }
                    DoctorID = ID.Value;
                }
                    DoctorProfilePageRequest doctor = await doctorSL.GetDoctorProfilePage(DoctorID);
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

        /// <summary>
        /// Update a doctor account using the provided form data
        /// </summary>
        /// <param name="request">
        /// The <see cref="DoctorUpdateRequest"/> object containing the updated doctor details
        /// </param>
        /// <returns>        
        /// - A <c>200 OK</c> status if the account is successfully updated.
        /// - A <c>400 Bad Request</c> status if validation fails or an exception occurs.</returns>
        /// <remarks>
        /// This endpoint accepts multipart/form-data and is limited to 10 MB in request size.
        /// It validates the input model before attempting to update the doctor account.
        /// </remarks>
        /// <response code="200">doctor account updated successfully</response>
        /// <response code="400">Validation failed or a general exception occurred</response>
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

        /// <summary>
        /// Logs the currently authenticated doctor out of their account.
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

        /// <summary>
        /// Deletes the currently authenticated doctor's account or, if the user is an admin, deletes a specified doctor's account.
        /// </summary>
        /// <param name="ID">
        /// The optional ID of the doctor whose profile is being requested. 
        /// If not provided and the authenticated user is not an admin, the ID is extracted from the JWT claims.
        /// </param>
        /// <returns>
        /// - A <c>200 OK</c> status if the account is successfully deleted.
        /// - A <c>400 Bad Request</c> status if validation fails or an exception occurs. 
        /// </returns>
        /// <remarks>
        /// This endpoint can be accessed by users with either the "Doctor" or "Admin" role.
        /// - Doctors can only delete their own account (ID is taken from JWT).
        /// - Admins can delete any doctor account by specifying the doctor ID in the query.
        /// </remarks>
        /// <response code="200">Admin account deleted successfully</response>
        /// <response code="400">general exception occurred</response>
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteAccountAsync([FromQuery]int? ID)
        {
            try
            {
                int DoctorID;
                string Role = User.FindFirstValue(ClaimTypes.Role);
                if (!ID.HasValue && !Role.Equals("Admin"))
                {
                    DoctorID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
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
                    DoctorID = ID.Value;
                }
                bool deleted = await doctorSL.DeleteDoctorAsync(DoctorID);
                if (!deleted)
                {
                    throw new Exception("error has occured");
                }
                var response = new ResponseWrapperDto
                {
                    Message = "account deleted successfully",
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
        /// Retrieves a paginated list of doctor profile pages, 10 per page.
        /// </summary>
        /// <param name="name">
        /// Optional name filter to search for doctors.
        /// </param>
        /// <param name="pagenumber">
        /// The page number to retrieve (must be a positive integer).
        /// </param>
        /// <param name="exactmatch">
        /// If true, performs an exact match on the doctor name; otherwise, performs a partial match.
        /// </param>
        /// <returns>
        /// - A <c>200 OK</c> response with a list of doctor profiles if found.
        /// - A <c>404 Not Found</c> if no matching doctors are found.
        /// - A <c>400 Bad Request</c> if the page number is invalid.
        /// - A <c>500 Internal Server Error</c> if an unexpected error occurs.
        /// </returns>
        /// <response code="200">A list of doctor profiles was found and returned successfully.</response>
        /// <response code="404">No matching doctor profiles were found.</response>
        /// <response code="400">The page number provided is invalid.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        [HttpGet("DoctorProfilesList")]
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
                List<DoctorProfilePageRequest> doctors = await doctorSL.GetDoctorsProfilePerPageAsync(name,pagenumber,exactmatch);
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

        /// <summary>
        /// Retrieves a paginated list of doctor accounts, 10 per page.
        /// </summary>
        /// <param name="name">
        /// Optional name filter to search for doctor.
        /// </param>
        /// <param name="pagenumber">
        /// The page number to retrieve (must be a positive integer).
        /// </param>
        /// <param name="exactmatch">
        /// If true, performs an exact match on the doctor name; otherwise, performs a partial match.
        /// </param>
        /// <returns>
        /// - A <c>200 OK</c> response with a list of doctors if found.
        /// - A <c>404 Not Found</c> if no matching doctors are found.
        /// - A <c>400 Bad Request</c> if the page number is invalid.
        /// - A <c>500 Internal Server Error</c> if an unexpected error occurs.
        /// </returns>
        /// <response code="200">A list of doctors was found and returned successfully.</response>
        /// <response code="404">No matching doctors are found.</response>
        /// <response code="200">The page number is invalid.</response>
        /// <response code="500">An unexpected error occurs.</response>
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
            List <DoctorModel> doctors = await doctorSL.GetDoctorsPerPageAsync(name,pagenumber,exactmatch);
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

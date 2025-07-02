using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Caridology_Department_System.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Caridology_Department_System.Requests.Admin;

namespace Caridology_Department_System.Controllers
{
    /// <summary>
    /// Controller responsible for handling admin-related HTTP requests.
    /// It acts as an entry point for the API endpoints related to admin functionalities.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AdminSL adminSL;
        private readonly JwtTokenService jwtTokenService;

        /// <summary>
        ///  Initializes a new instance of the <see cref="AdminController"/> class with the specified service layer.
        /// </summary>
        /// <param name="adminSL">service layer that is responsible to handle admin logic</param>
        /// <param name="jwtTokenService">web jason token sevice layer that is responsible of generating token</param>
        public AdminController(AdminSL adminSL, JwtTokenService jwtTokenService )
        {
            this.adminSL = adminSL;
            this.jwtTokenService = jwtTokenService;
        }

        /// <summary>
        /// Authenticates an admin user based on provided credentials and returns a JWT if successful.
        /// </summary>
        /// <param name="request">
        /// The login request containing the admin's email and password. Both fields are required.
        /// </param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <c>ResponseWrapperDto</c> with:
        /// - A <c>200 OK</c> status, including the authenticated admin's data and a JWT token.
        /// - A <c>400 Bad Request</c> status if the credentials are invalid or if a general error occurs.
        /// </returns>
        /// <remarks>
        /// This endpoint is intended for admin users only. 
        /// On successful authentication, a signed JSON Web Token (JWT) is returned for use in subsequent requests.
        /// The credentials are matched against the admin records stored in the database.
        /// </remarks>
        /// <response code="200">Authentication successful; returns admin info and JWT token</response>
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
                AdminModel admin = await adminSL.GetAdminByEmailAndPassword(request);
                var token = jwtTokenService.GenerateToken(admin);
                return Ok(new
                {
                    Token = token,
                    Admin = admin
                });
            }
            catch (Exception ex)
            {
                var response = new ResponseWrapperDto {
                    Errors = ex.Message,
                    Success= false,
                    StatusCode=400
                };
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Retrieves an admin user's profile information based on the provided ID or JWT token.
        /// </summary>
        /// <param name="ID">
        /// The ID of the admin whose profile is being requested. If not provided, the ID is extracted from the JWT claims.
        /// </param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <c>ResponseWrapperDto</c> with:
        /// - A <c>200 OK</c> status and the admin profile data if retrieval is successful.
        /// - A <c>400 Bad Request</c> status if the ID is invalid, not a positive number, or if an error occurs during processing.
        /// </returns>
        /// <remarks>
        /// This endpoint is accessible to authenticated admin users only.
        /// If no ID is supplied via query, the user's ID is automatically extracted from the JWT token's claims.
        /// </remarks>
        /// <response code="200">Retrieval successful; returns admin profile data</response>
        /// <response code="400">Invalid ID or a general exception occurred</response>
        [HttpGet("Profile")]
        public async Task<IActionResult> GetAdminProfileAsync([FromQuery]int? ID)
        {
            try
            {
                int adminID;
                if (!ID.HasValue)
                {
                    adminID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }
                else
                {
                    if (ID < 1)
                    {
                        var response = new ResponseWrapperDto
                        {
                            Message = "Id must be positive number",
                            Success = false,
                            StatusCode = 400
                        };
                        return BadRequest(response);
                    }
                    adminID = ID.Value;
                }
                AdminProfilePageRequest adminProfilePage = await adminSL.GetAdminProfile(adminID);
                return Ok(adminProfilePage);
            }
            catch(Exception ex) {
                var response = new ResponseWrapperDto {
                    Errors = ex.Message,
                    Success = false,
                    StatusCode =400
                };
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Creates a new admin account using the provided form data.
        /// </summary>
        /// <param name="admin">
        /// The <see cref="AdminRequest"/> object containing the admin's details, including personal information,
        /// credentials, and optionally a profile image. Submitted as multipart/form-data.
        /// </param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <c>ResponseWrapperDto</c> with:
        /// - A <c>200 OK</c> status if the account is successfully created.
        /// - A <c>400 Bad Request</c> status if validation fails or an exception occurs.
        /// </returns>
        /// <remarks>
        /// This endpoint accepts multipart/form-data and is limited to 10 MB in request size.
        /// It validates the input model before attempting to create the admin account.
        /// </remarks>
        /// <response code="200">Admin account created successfully</response>
        /// <response code="400">Validation failed or a general exception occurred</response>
        [HttpPost("CreateAdmin")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> CreateAdminAsync([FromForm] AdminRequest admin)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                bool created = await adminSL.AddAdminasync(admin);
                var response = new ResponseWrapperDto
                {
                    Success = created,
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
        /// Update an admin account using the provided form data
        /// </summary>
        /// <param name="request">
        /// The <see cref="AdminUpdateRequest"/> object containing the updated admin details
        /// </param>
        /// <returns>        
        /// - A <c>200 OK</c> status if the account is successfully updated.
        /// - A <c>400 Bad Request</c> status if validation fails or an exception occurs.</returns>
        /// <remarks>
        /// This endpoint accepts multipart/form-data and is limited to 10 MB in request size.
        /// It validates the input model before attempting to update the admin account.
        /// </remarks>
        /// <response code="200">Admin account updated successfully</response>
        /// <response code="400">Validation failed or a general exception occurred</response>
        [HttpPut("Profile")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> UpdateAdminProfileAsync([FromForm] AdminUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                int adminID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                bool Updated = await adminSL.UpdateProfileAsync(adminID,request);
                if (!Updated)
                {
                    throw new Exception("error has occured");
                }
                var response = new ResponseWrapperDto
                {
                    Message= "Account updated successfully",
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
        /// Logs the currently authenticated admin out of their account.
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
        /// Deletes the currently authenticated admin account.
        /// </summary>
        /// <returns>
        /// - A <c>200 OK</c> status if the account is successfully deleted.
        /// - A <c>400 Bad Request</c> status an exception occurs. 
        /// </returns>
        /// <response code="200">Admin account deleted successfully</response>
        /// <response code="400">general exception occurred</response>
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteAccountAsync()
        {
            try
            {
                int adminID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                bool deleted = await adminSL.DeleteAdminAsync(adminID);
                if (!deleted)
                {
                    throw new Exception("error has occured");
                }
                var response = new ResponseWrapperDto
                {
                    Message= "account deleted successfuly",
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
        /// Retrieves a paginated list of admin accounts, 10 per page.
        /// </summary>
        /// <param name="name">
        /// Optional name filter to search for admins.
        /// </param>
        /// <param name="pagenumber">
        /// The page number to retrieve (must be a positive integer).
        /// </param>
        /// <param name="exactmatch">
        /// If true, performs an exact match on the admin name; otherwise, performs a partial match.
        /// </param>
        /// <returns>
        /// - A <c>200 OK</c> response with a list of admins if found.
        /// - A <c>404 Not Found</c> if no matching admins are found.
        /// - A <c>400 Bad Request</c> if the page number is invalid.
        /// - A <c>500 Internal Server Error</c> if an unexpected error occurs.
        /// </returns>
        /// <response code="200">A list of admins was found and returned successfully.</response>
        /// <response code="404">No matching admins are found.</response>
        /// <response code="200">The page number is invalid.</response>
        /// <response code="500">An unexpected error occurs.</response>
        [HttpGet("AdminsList")]
        public async Task<IActionResult> GetAdminsPerPageAsync([FromQuery] string? name
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
                List<AdminModel> Admins = await adminSL.GetAdminsPerPageAsync(name, pagenumber, exactmatch);
                if (Admins == null || Admins.Count == 0)
                {
                    var response = new ResponseWrapperDto
                    {
                        StatusCode = 404,
                        Success = false,
                        Message = "no Admins found"
                    };
                    return NotFound(response);
                }
                return Ok(Admins);
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
        /// Retrieves a paginated list of admin profile pages, 10 per page.
        /// </summary>
        /// <param name="name">
        /// Optional name filter to search for admins.
        /// </param>
        /// <param name="pagenumber">
        /// The page number to retrieve (must be a positive integer).
        /// </param>
        /// <param name="exactmatch">
        /// If true, performs an exact match on the admin name; otherwise, performs a partial match.
        /// </param>
        /// <returns>
        /// - A <c>200 OK</c> response with a list of admin profiles if found.
        /// - A <c>404 Not Found</c> if no matching admins are found.
        /// - A <c>400 Bad Request</c> if the page number is invalid.
        /// - A <c>500 Internal Server Error</c> if an unexpected error occurs.
        /// </returns>
        /// <response code="200">A list of admin profiles was found and returned successfully.</response>
        /// <response code="404">No matching admin profiles were found.</response>
        /// <response code="400">The page number provided is invalid.</response>
        /// <response code="500">An unexpected server error occurred.</response>
        [HttpGet("AdminProfilesList")]
        public async Task<IActionResult> GetAdminsProfilePageAsync([FromQuery] string? name
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
                List<AdminProfilePageRequest> Admins = await adminSL.GetAdminsProfilePerPageAsync(name, pagenumber, exactmatch);
                if (Admins == null || !Admins.Any())
                {
                    var response = new ResponseWrapperDto
                    {
                        StatusCode = 404,
                        Success = false,
                        Message = "no Admins found"
                    };
                    return NotFound(response);
                }
                return Ok(Admins);
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


using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Caridology_Department_System.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using AutoMapper;
using Caridology_Department_System.Requests.Admin;

namespace Caridology_Department_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AdminSL adminSL;
        private readonly JwtTokenService jwtTokenService;
        public AdminController(AdminSL adminSL, JwtTokenService jwtTokenService )
        {
            this.adminSL = adminSL;
            this.jwtTokenService = jwtTokenService;
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
        [HttpGet("Profile")]
        public async Task<IActionResult> GetAdminProfileAsync(int? ID)
        {
            try
            {
                int adminID;
                if (!ID.HasValue || ID < 1)
                {
                    adminID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }
                else
                {
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
        [HttpPost("CreateAdmim")]
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
        /*
        [HttpPut("UpdatePatient/{id}")]
        public IActionResult UpdatePatientProfile(int id, [FromBody] PatientRequest request)
        {
            AdminSL adminSL = new AdminSL();
            var admin = GetAuthenticatedAdmin(adminSL);

            if (admin == null)
            {
                int? adminID = HttpContext.Session.GetInt32("AdminID");
                if (adminID == null)
                    return Unauthorized("Not logged in");
                else
                    return NotFound("Admin not found");
            }

            try
            {
                //var patientSL = new PatientSL();
               // patientSL.UpdateProfile(id, request, request.PhoneNumbers);

                return Ok(new { Message = "Patient profile updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        */
        /*
        [HttpPost("AddPatient")]
        public IActionResult AddPatient([FromBody] PatientRequest request, [FromQuery] List<string> phoneNumbers)
        {
            AdminSL adminSL = new AdminSL();
            var admin = GetAuthenticatedAdmin(adminSL);

            if (admin == null)
            {
                int? adminID = HttpContext.Session.GetInt32("AdminID");
                if (adminID == null)
                    return Unauthorized("Not logged in");
                else
                    return NotFound("Admin not found");
            }

            try
            {
                if (request == null)
                    return BadRequest("Patient data is required");

                if (phoneNumbers == null || !phoneNumbers.Any())
                    return BadRequest("At least one phone number is required");

              //  PatientSL patientSL = new PatientSL();
                // patientSL.AddPatientAsync(request, phoneNumbers);

                return Ok(new { Message = "Patient added successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, $"Database error: {dbEx.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        */       
        /*
        [HttpDelete("DeletePatient/{id}")]
        public IActionResult DeletePatient(int id)
        {
            AdminSL adminSL = new AdminSL();
            var admin = GetAuthenticatedAdmin(adminSL);

            if (admin == null)
            {
                int? adminID = HttpContext.Session.GetInt32("AdminID");
                if (adminID == null)
                    return Unauthorized("Not logged in");
                else
                    return NotFound("Admin not found");
            }

            try
            {
                adminSL.DeletePatient(id);
                return Ok(new { Message = "Patient deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting patient: {ex.Message}");
            }
        }
        */
        /*
        [HttpPatch("UpdateAdminStatus/{id}")]
        public IActionResult UpdateAdminStatus(int id, [FromBody] int newStatus)
        {
            AdminSL adminSL = new AdminSL();
            var admin = GetAuthenticatedAdmin(adminSL);

            if (admin == null)
            {
                int? adminID = HttpContext.Session.GetInt32("AdminID");
                if (adminID == null)
                    return Unauthorized("Not logged in");
                else
                    return NotFound("Admin not found");
            }

            try
            {
                adminSL.UpdateAdminStatus(id, newStatus);

                return Ok(new { Message = "Admin status updated successfully", AdminId = id, NewStatus = newStatus });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        */
        /*
        public class PatientStatusUpdateRequest
        {
            public int NewStatus { get; set; }
        }

        [HttpPatch("UpdatePatientStatus/{id}")]
        public IActionResult UpdatePatientStatus(int id, [FromBody] PatientStatusUpdateRequest request)
        {
            AdminSL adminSL = new AdminSL();
            var admin = GetAuthenticatedAdmin(adminSL);

            if (admin == null)
            {
                int? adminID = HttpContext.Session.GetInt32("AdminID");
                if (adminID == null)
                    return Unauthorized("Not logged in");
                else
                    return NotFound("Admin not found");
            }

            try
            {
                //var patientSL = new PatientSL();
                //patientSL.UpdatePatientStatus(id, request.NewStatus);

                return Ok(new { Message = "Patient status updated successfully", PatientId = id, NewStatus = request.NewStatus });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        */
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
    }
}


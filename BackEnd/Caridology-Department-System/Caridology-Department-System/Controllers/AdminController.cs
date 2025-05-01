using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Caridology_Department_System.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace Caridology_Department_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        // Helper method for admin authentication and retrieval
        private AdminModel GetAuthenticatedAdmin(AdminSL adminSL)
        {
            int? adminID = HttpContext.Session.GetInt32("AdminID");
            if (adminID == null)
                return null;

            AdminModel admin = adminSL.GetAdminByID(adminID);
            return admin;
        }

        [HttpGet("Profile")]
        public IActionResult GetAdminProfile()
        {
            AdminSL adminSL = new AdminSL();
            var admin = GetAuthenticatedAdmin(adminSL);

            if (admin == null)
            {
                int? id = HttpContext.Session.GetInt32("AdminID");
                if (id == null)
                    return Unauthorized("Not logged in");
                else
                    return NotFound("Admin not found");
            }

            return Ok(new
            {
                admin.ID,
                admin.FName,
                admin.LName,
                admin.FullName,
                admin.Email,
                admin.BirthDate,
                admin.PhotoPath,
                admin.RoleID,
                RoleModel = admin.Role?.RoleName,
                admin.StatusID,
                StatusName = admin.Status?.Name,
                admin.CreatedAt,
                admin.UpdatedAt,
                PhoneNumbers = admin.PhoneNumbers.Select(p => p.PhoneNumber).ToList()
            });
        }

        [HttpPut("Profile")]
        public IActionResult UpdateAdminProfile([FromBody] AdminUpdateRequest request)
        {
            AdminSL adminSL = new AdminSL();
            var admin = GetAuthenticatedAdmin(adminSL);

            if (admin == null)
            {
                int? id = HttpContext.Session.GetInt32("AdminID");
                if (id == null)
                    return Unauthorized("Not logged in");
                else
                    return NotFound("Admin not found");
            }

            try
            {
                adminSL.UpdateProfile(
                    admin.ID,
                    request,
                    request.PhoneNumbers
                );

                return Ok(new { Message = "Admin profile updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, "Database error: " + dbEx.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

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
                var patientSL = new PatientSL();
                patientSL.UpdateProfile(id, request, request.PhoneNumbers);

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

                PatientSL patientSL = new PatientSL();
                 patientSL.AddPatientAsync(request, phoneNumbers);

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

        [HttpPost("Login")]
        public IActionResult Login(LoginRequest request)
        {
            AdminSL adminSL = new AdminSL();
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                AdminModel admin = adminSL.GetAdminByEmailAndPassword(request);
                HttpContext.Session.SetInt32("AdminID", admin.ID);
                return Ok(admin);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }
        }

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
                var patientSL = new PatientSL();
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

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            bool wasLoggedIn = HttpContext.Session.TryGetValue("AdminID", out _);
            HttpContext.Session.Clear();

            return Ok(new
            {
                message = wasLoggedIn
                    ? "Logout successful"
                    : "No active admin session found"
            });
        }
    }
}

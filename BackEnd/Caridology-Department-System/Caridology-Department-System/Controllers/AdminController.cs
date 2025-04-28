using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Caridology_Department_System.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Caridology_Department_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        [HttpGet("Profile")]
        public IActionResult GetAdminProfile()
        {
            AdminSL adminSL = new AdminSL();
            int? AdminID = HttpContext.Session.GetInt32("AdminID");
            if (AdminID == null)
                return Unauthorized("Not logged in");
            AdminModel admin = adminSL.GetAdminByID(AdminID);
            if (admin == null)
            {
                return NotFound("Admin not found");
            }
            return Ok(admin);
        }
        [HttpPost("Register")]
        public IActionResult Register([FromBody] AdminRequest Admin)
        {
            try
            {
                if (Admin == null || Admin.PhoneNumbers == null)
                {
                    return BadRequest("Admin data is required");
                }
                AdminSL adminSL = new AdminSL();
                adminSL.AddAdmin(Admin);
                return Ok("Admin created successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost("Login")]
        public IActionResult Login(LoginRequest request) {
            AdminSL adminSL = new AdminSL();
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                AdminModel Admin = adminSL.GetAdminByEmailAndPassword(request);
                HttpContext.Session.SetInt32("AdminID", Admin.ID);
                return Ok(Admin);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }
        }
    }
}

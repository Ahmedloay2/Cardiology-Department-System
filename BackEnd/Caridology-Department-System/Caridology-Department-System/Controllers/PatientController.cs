using Azure.Core;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests;
using Caridology_Department_System.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Caridology_Department_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        [HttpGet("{id}")]
        public IActionResult GetPatient(int id)
        {
            PatientSL patientSL = new PatientSL();
            PatientModel patient = patientSL.GetPatientByID(id);
            if (patient == null)
                return NotFound($"Patient with ID {id} not found or is deleted.");
            return Ok(patient);
        }
        [HttpPost("register")]
        public IActionResult Register([FromBody] PatientRequest patient)
        {
            try
            {
                if (patient == null || patient.PhoneNumbers == null)
                    return BadRequest("patient Data is required");
                PatientSL patientSL = new PatientSL();
                patientSL.AddPatient(patient);
                return Ok(new { Message = "Patient registered successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

using System.Security.Claims;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests.Report;
using Caridology_Department_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Caridology_Department_System.Controllers
{
    /// <summary>
    /// Controller responsible for handling report-related HTTP requests.
    /// It acts as an entry point for the API endpoints related to report functionalities.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportSL reportSL;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportController"/> class.
        /// </summary>
        /// <param name="reportSL">The service layer instance used to handle report-related operations. This parameter cannot be null.</param>
        public ReportController(IReportSL reportSL)
        {
            this.reportSL = reportSL;
        }
        /// <summary>
        /// Creates a new medical report based on the provided data.
        /// </summary>
        /// <remarks>This method is accessible only to users with the "Doctor" role and requires
        /// authorization. It validates the input model and attempts to create a report using the provided data. If the
        /// report creation is successful, a success response is returned; otherwise, an error response is
        /// generated.</remarks>
        /// <param name="request">The data required to create the report, encapsulated in a <see cref="ReportDto"/> object.</param>
        /// <returns>An <see cref="IActionResult"/> containing the result of the operation.  Returns a success response with
        /// status code 200 if the report is created successfully.  Returns a bad request response with status code 400
        /// if the input is invalid or an error occurs during report creation.</returns>
        [Authorize(Roles = "Doctor")]
        [HttpPost("CreateReport")]
        public async Task<IActionResult> CreateReport(ReportDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                int ID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                bool Created = await reportSL.CreateReportAsync(request, ID);
                if (!Created)
                {
                    throw new Exception("Error has occured");
                }
                ResponseWrapperDto response = new ResponseWrapperDto
                {
                    StatusCode = 200,
                    Message = "Report created successfully",
                    Success = true
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                ResponseWrapperDto response = new ResponseWrapperDto
                {
                    StatusCode = 400,
                    Message = "Error has occured",
                    Errors = ex.Message,
                    Success = false
                };
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Updates an existing medical report with the provided data.
        /// </summary>
        /// <remarks>This endpoint is restricted to users with the "Doctor" role and requires
        /// authorization. Ensure that the <paramref name="request"/> object is valid before calling this
        /// method.</remarks>
        /// <param name="request">An object containing the updated report details. The request must conform to the expected model structure.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the outcome of the operation.  Returns a success response if the
        /// report is updated successfully, or a message indicating no changes were made. Returns a bad request response
        /// if the input model is invalid or an error occurs during processing.</returns>
        [Authorize(Roles = "Doctor")]
        [HttpPost("UpdateReport")]
        public async Task<IActionResult> UpdateReport(ReportDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                int ID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                bool Updated= await reportSL.UpdateReportAsync(request, ID);
                if (!Updated)
                {
                    ResponseWrapperDto response1 = new ResponseWrapperDto
                    {
                        StatusCode = 200,
                        Message = "There is no thing to update",
                        Success = true
                    };
                    return Ok(response1);
                }
                ResponseWrapperDto response2 = new ResponseWrapperDto
                {
                    StatusCode = 200,
                    Message = "Report Updated successfully",
                    Success = true
                };
                return Ok(response2);
            }
            catch (Exception ex)
            {
                ResponseWrapperDto response = new ResponseWrapperDto
                {
                    StatusCode = 400,
                    Message = "Error has occured",
                    Errors = ex.Message,
                    Success = false
                };
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Retrieves a report associated with the specified appointment ID.
        /// </summary>
        /// <remarks>This method requires authorization and is accessible via an HTTP GET request to the
        /// "GetReport" endpoint. Ensure that the <paramref name="appointmentID"/> is a positive integer.</remarks>
        /// <param name="appointmentID">The unique identifier of the appointment for which the report is requested. Must be greater than 0.</param>
        /// <returns>An <see cref="IActionResult"/> containing the report data if the operation is successful. Returns a <see
        /// cref="BadRequestObjectResult"/> if the appointment ID is invalid or if an error occurs.</returns>
        [Authorize]
        [HttpGet("GetReport")]
        public async Task<IActionResult> GetReport([FromQuery] int appointmentID)
        {
            try
            {
                if (appointmentID < 1)
                {
                    return BadRequest("enter a vlaid ID");
                }
                ReportModel report = await reportSL.GetReportByAppointmentIdAsync(appointmentID);
                return Ok(report);
            }
            catch (Exception ex)
            {
                ResponseWrapperDto response = new ResponseWrapperDto
                {
                    StatusCode = 400,
                    Message = "Error has occured",
                    Errors = ex.Message,
                    Success = false
                };
                return BadRequest(response);
            }
        }
    }
}

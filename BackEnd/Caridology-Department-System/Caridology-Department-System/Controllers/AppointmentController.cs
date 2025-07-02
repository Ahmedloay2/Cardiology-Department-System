
using System.Security.Claims;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests.Appointment;
using Caridology_Department_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Caridology_Department_System.Controllers
{
    /// <summary>
    /// API controller responsible for handling appointment-related HTTP requests.
    /// Provides endpoints for creating, retrieving, updating, and managing appointments
    /// between patients and doctors in the cardiology department system.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly AppointmentSL appointmentSL;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppointmentController"/> class.
        /// </summary>
        /// <param name="appointmentSL">The appointment service layer responsible for handling appointment business logic.</param>
        public AppointmentController(AppointmentSL appointmentSL)
        {
            this.appointmentSL = appointmentSL;
        }

        /// <summary>
        /// Creates a new appointment between the authenticated patient and the specified doctor.
        /// </summary>
        /// <param name="appointmentRequest">The appointment creation request containing the appointment date and doctor ID.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> representing the result of the appointment creation:
        /// <list type="bullet">
        /// <item><description>200 OK - Appointment created successfully</description></item>
        /// <item><description>400 Bad Request - Invalid input data or business logic error</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// <para>This endpoint requires authentication and is restricted to users with the "Patient" role.</para>
        /// <para>The patient's ID is automatically extracted from the JWT token claims.</para>
        /// <para>The appointment date must be in the future and the requested time slot must be available.</para>
        /// </remarks>
        /// <response code="200">Appointment created successfully with confirmation details.</response>
        /// <response code="400">Invalid request model, appointment date, or time slot unavailable.</response>
        [Authorize(Roles = "Patient")]
        [HttpPost("BookAppointment")]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentRequest appointmentRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                int patientid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                bool Created = await appointmentSL.CreateAppointmentAsync(patientid,appointmentRequest);
                if (!Created)
                {
                    throw new Exception("Failed to create an appointment");
                }
                var response = new ResponseWrapperDto
                {
                    StatusCode = 200,
                    Success = true,
                    Message = $"Appointment Created successfully at {appointmentRequest.AppDate}"
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseWrapperDto
                {
                    StatusCode = 400,
                    Success = false,
                    Message = "Error has occured",
                    Errors = ex.Message
                };
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Retrieves all confirmed appointments for a specific doctor or patient on a given day.
        /// </summary>
        /// <param name="RequestedDate">The date for which to retrieve appointments (required).</param>
        /// <param name="ID">
        /// The unique identifier of the doctor or patient. 
        /// If the user is authenticated as a doctor or patient, this parameter is ignored and the ID is extracted from the token.
        /// </param>
        /// <param name="IsPatient">
        /// Indicates whether to retrieve appointments for a patient (true) or doctor (false).
        /// Required only for non-authenticated requests or admin users.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// <list type="bullet">
        /// <item><description>200 OK - List of appointments for the specified date</description></item>
        /// <item><description>400 Bad Request - Invalid parameters or error occurred</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// <para>For authenticated doctors and patients, the ID is automatically extracted from the JWT token.</para>
        /// <para>For admin users or public access, both ID and isPatient parameters must be provided.</para>
        /// <para>Returns an empty list if no appointments are found for the specified date.</para>
        /// </remarks>
        /// <response code="200">List of appointments retrieved successfully.</response>
        /// <response code="400">Invalid parameters, missing role information, or user not found.</response>
        [HttpGet("GetAppointments")]
        public async Task<IActionResult> GetAppointmentsByDay([FromQuery]DateTime RequestedDate
                                                              ,[FromQuery]int?ID , 
                                                            [FromQuery] bool? IsPatient)
        {
            try
            {
                string Role = User.FindFirstValue(ClaimTypes.Role);
                if (Role.Equals("Doctor") || Role.Equals("Patient"))
                {
                    ID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }
                else
                {
                    if (IsPatient.HasValue)
                    {
                        if (IsPatient.Value)
                        {
                            Role = "Patient";
                        }
                        else
                        {
                            Role = "Doctor";
                        }
                    }
                    else
                    {
                        throw new Exception("You must choose if it patient or doctor");
                    }
                }
                List<AppointmentDataRequest> appointments = await appointmentSL.
                                                               GetAppointmentsDataByDayAsync(RequestedDate,
                                                               ID, Role);
                    return Ok(appointments);
            }
            catch (Exception ex)
            {
                var response = new ResponseWrapperDto
                {
                    StatusCode = 400,
                    Success = false,
                    Message = "Error has occured",
                    Errors = ex.Message
                };
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Retrieves a specific confirmed appointment for a doctor or patient at an exact date and time.
        /// </summary>
        /// <param name="requestedDate">The exact date and time of the appointment to retrieve (required).</param>
        /// <param name="id">
        /// The unique identifier of the doctor or patient.
        /// If the user is authenticated as a doctor or patient, this parameter is ignored and the ID is extracted from the token.
        /// </param>
        /// <param name="isPatient">
        /// Indicates whether to retrieve an appointment for a patient (true) or doctor (false).
        /// Required only for non-authenticated requests or admin users.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// <list type="bullet">
        /// <item><description>200 OK - The appointment details at the specified time</description></item>
        /// <item><description>400 Bad Request - Invalid parameters, no appointment found, or error occurred</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// <para>For authenticated doctors and patients, the ID is automatically extracted from the JWT token.</para>
        /// <para>For admin users or public access, both ID and isPatient parameters must be provided.</para>
        /// <para>The date and time must match exactly with an existing confirmed appointment.</para>
        /// </remarks>
        /// <response code="200">Appointment details retrieved successfully.</response>
        /// <response code="400">No appointment found at the specified time, invalid parameters, or user not found.</response>

        [HttpGet("GetAppointment")]
        public async Task<IActionResult> GetAppointmentByDayAndTime([FromQuery] DateTime RequestedDate
                                                              , [FromQuery] int? ID , 
                                                            [FromQuery] bool? IsPatient)
        {
            try
            {
                string Role = User.FindFirstValue(ClaimTypes.Role);
                if (Role.Equals("Doctor") || Role.Equals("Patient"))
                {
                    ID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                }
                else
                {
                    if (IsPatient.HasValue)
                    {
                        if (IsPatient.Value)
                        {
                            Role = "Patient";
                        }
                        else
                        {
                            Role = "Doctor";
                        }
                    }
                    else
                    {
                        throw new Exception("You must choose if it patient or doctor");
                    }
                }
                AppointmentDataRequest appointment = await appointmentSL.
                                                               GetAppointmentDataByDayAndTimeAsync(
                                                                RequestedDate, ID, Role);
                return Ok(appointment);
            }
            catch (Exception ex)
            {
                var response = new ResponseWrapperDto
                {
                    StatusCode = 400,
                    Success = false,
                    Message = "Error has occured",
                    Errors = ex.Message
                };
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Reschedules an existing appointment to a new date and time for the authenticated patient.
        /// </summary>
        /// <param name="request">
        /// The reschedule request containing:
        /// <list type="bullet">
        /// <item><description>AppDate - The original appointment date and time</description></item>
        /// <item><description>NewDate - The new desired appointment date and time</description></item>
        /// <item><description>DoctorID - The unique identifier of the doctor</description></item>
        /// </list>
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> representing the result of the reschedule operation:
        /// <list type="bullet">
        /// <item><description>200 OK - Appointment rescheduled successfully</description></item>
        /// <item><description>400 Bad Request - Invalid input, unauthorized access, or conflict</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// <para>This endpoint requires authentication and is restricted to users with the "Patient" role.</para>
        /// <para>The patient's ID is automatically extracted from the JWT token.</para>
        /// <para>Only the patient who originally booked the appointment can reschedule it.</para>
        /// <para>The new appointment date must be in the future and the time slot must be available.</para>
        /// <para>The original appointment will be marked as "Postponed" and a new appointment will be created.</para>
        /// </remarks>
        /// <response code="200">Appointment rescheduled successfully with new date confirmation.</response>
        /// <response code="400">Invalid request data, original appointment not found, new time slot unavailable, or unauthorized access.</response>

        [Authorize(Roles ="Patient")]
        [HttpPost("RescheduleAppointment")]
        public async Task<IActionResult> RescheduleAppointment([FromBody] RescheduleAppointmentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                int PatientID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                bool Rescheduled = await appointmentSL.RescheduleAppointmentAsync(request,PatientID);
                if (!Rescheduled)
                {
                    throw new Exception("Failed to create an appointment");
                }
                ResponseWrapperDto response = new ResponseWrapperDto
                {
                    Success = true,
                    Message = $"Appointment rescheduled successfully to {request.NewDate:yyyy-MM-dd HH:mm}",
                    StatusCode = 200
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                ResponseWrapperDto response = new ResponseWrapperDto
                {
                    Success = false,
                    Message = "Error has occured",
                    StatusCode = 400,
                    Errors = ex.Message
                };
                return BadRequest(response);
            }
        }

        /// <summary>
        /// Cancels an existing appointment for the authenticated patient.
        /// </summary>
        /// <param name="request">
        /// The cancellation request containing:
        /// <list type="bullet">
        /// <item><description>AppDate - The appointment date and time to cancel</description></item>
        /// <item><description>DoctorID - The unique identifier of the doctor</description></item>
        /// </list>
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> representing the result of the cancellation:
        /// <list type="bullet">
        /// <item><description>200 OK - Appointment cancelled successfully</description></item>
        /// <item><description>400 Bad Request - Invalid input, appointment not found, or unauthorized access</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// <para>This endpoint requires authentication and is restricted to users with the "Patient" role.</para>
        /// <para>The patient's ID is automatically extracted from the JWT token.</para>
        /// <para>Only the patient who originally booked the appointment can cancel it.</para>
        /// <para>The appointment status will be changed to "Cancelled".</para>
        /// </remarks>
        /// <response code="200">Appointment cancelled successfully with confirmation details.</response>
        /// <response code="400">Invalid request data, appointment not found, or unauthorized access.</response>

        [Authorize(Roles = "Patient")]
        [HttpPost("CancelAppointment")]
        public async Task<IActionResult> CancelAppointment([FromBody] AppointmentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                int PatientID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                bool Cancelled = await appointmentSL.CancelAppointmentAsync(request, PatientID);
                if (!Cancelled)
                {
                    throw new Exception("Failed to create an appointment");
                }
                ResponseWrapperDto response = new ResponseWrapperDto
                {
                    Success = true,
                    Message = $"Appointment with date:{request.AppDate:yyyy-MM-dd HH:mm} successfully cancelled ",
                    StatusCode = 200
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                ResponseWrapperDto response = new ResponseWrapperDto
                {
                    Success = false,
                    Message = "Error has occured",
                    StatusCode = 400,
                    Errors = ex.Message
                };
                return BadRequest(response);
            }
        }


        /// <summary>
        /// Marks an appointment as completed or missed by the attending doctor.
        /// </summary>
        /// <param name="AppointmentId">The unique identifier of the appointment to mark (required).</param>
        /// <param name="IsCompleted">
        /// Indicates the appointment outcome:
        /// <list type="bullet">
        /// <item><description>true - The appointment was completed successfully</description></item>
        /// <item><description>false - The appointment was missed by the patient</description></item>
        /// </list>
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> representing the result of the marking operation:
        /// <list type="bullet">
        /// <item><description>200 OK - Appointment marked successfully</description></item>
        /// <item><description>400 Bad Request - Invalid input, appointment not found, unauthorized access, or timing error</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// <para>This endpoint requires authentication and is restricted to users with the "Doctor" role.</para>
        /// <para>The doctor's ID is automatically extracted from the JWT token.</para>
        /// <para>Only the doctor assigned to the appointment can mark it.</para>
        /// <para>Appointments can only be marked after their scheduled time has passed.</para>
        /// <para>The appointment status will be changed to either "Completed" or "Missed" based on the isCompleted parameter.</para>
        /// </remarks>
        /// <response code="200">Appointment marked successfully with status confirmation.</response>
        /// <response code="400">Invalid appointment ID, appointment not found, unauthorized access, or attempt to mark future appointment.</response>
        [Authorize(Roles ="Doctor")]
        [HttpPost("MarkAppointment")]
        public async Task<IActionResult> MarkAppointment([FromQuery] int AppointmentId,
                                                         [FromQuery] bool IsCompleted)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                int DoctorID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                bool Marked= await appointmentSL.MarkAppointmentAsync(AppointmentId, DoctorID, IsCompleted);
                if (!Marked)
                {
                    throw new Exception("Error has occured while marking");
                }
                ResponseWrapperDto response = new ResponseWrapperDto
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Appointment has been marked successfully"
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                ResponseWrapperDto response = new ResponseWrapperDto
                {
                    Success = false,
                    StatusCode = 400,
                    Message = "Error has occured",
                    Errors = ex.Message
                };
                return BadRequest(response);
            }
        }

    }
}

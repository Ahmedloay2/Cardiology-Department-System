using System.Security.Claims;
using Caridology_Department_System.Models;
using Caridology_Department_System.Services;
using Microsoft.AspNetCore.Mvc;

namespace Caridology_Department_System.Controllers
{
    /// <summary>
    /// Controller responsible for handling message-related HTTP requests.
    /// It acts as an entry point for the API endpoints related to messaging functionalities.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly MessageSL messageSL;
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageController"/> class with the specified service layer.
        /// </summary>
        /// <param name="messageSL">The service layer that contains business logic for message operations.</param>
        public MessageController(MessageSL messageSL)
        {
            this.messageSL = messageSL;
        }
        /// <summary>
        /// Retrieves messages between the logged-in user and the opposite role (patient/doctor).
        /// </summary>
        /// <param name="patientid">Optional query parameter. Will be overridden if the logged-in user is a patient.</param>
        /// <param name="doctorid">Optional query parameter. Will be overridden if the logged-in user is a doctor.</param>
        /// <returns>
        /// Returns a list of messages in an <see cref="IActionResult"/>.
        /// If successful, returns HTTP 200 with the message list.
        /// If failed, returns HTTP 400 with an error message.
        /// </returns>
        /// <remarks>
        /// The method uses the logged-in user's identity and role (from JWT claims) to determine the sender.
        /// Only users with roles "Patient" or "Doctor" can use this endpoint.
        /// </remarks>
        /// <response code="200">Returns list of messages between the doctor and patient</response>
        /// <response code="400">If the user is not logged in or an error occurs</response>
        [HttpGet("GetMessages")]
        public async Task<IActionResult> GetMessagesAsync([FromQuery] int? patientid, [FromQuery] int? doctorid)
        {
            try
            {
                int ID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                string Role = User.FindFirstValue(ClaimTypes.Role);

                if (string.IsNullOrWhiteSpace(Role))
                {
                    var response = new ResponseWrapperDto
                    {
                        Message = "you must be logged in",
                        StatusCode = 400,
                        Success = false,
                    };
                    return BadRequest(response);
                }

                if (Role.Equals("Patient"))
                {
                    patientid = ID;
                }
                else if (Role.Equals("Doctor"))
                {
                    doctorid = ID;
                }

                List<MessageModel> messages = await messageSL.GetMessagesAsync(patientid, doctorid);
                return Ok(messages);
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
        /// Sends a message from the currently authenticated user (Doctor or Patient) to a recipient.
        /// </summary>
        /// <param name="Content">
        /// The content of the message to be sent. Must not be null or empty.
        /// </param>
        /// <param name="reciverID">
        /// The ID of the recipient user, passed as a query parameter. Required if sender is a Doctor.
        /// </param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a <c>ResponseWrapperDto</c>:
        /// - <c>200 OK</c> if the message is successfully sent.
        /// - <c>400 Bad Request</c> if input is invalid or a general error occurs.
        /// - <c>403 Forbidden</c> if the sender is unauthorized to perform this action.
        /// </returns>
        /// <remarks>
        /// The sender's ID and role are automatically extracted from the user's JWT claims.
        /// Only users with roles "Patient" or "Doctor" are authorized to send messages.
        /// Admins are explicitly forbidden from using this endpoint.
        /// </remarks>
        /// <response code="200">Message sent successfully</response>
        /// <response code="400">Validation failed or a general exception occurred</response>
        /// <response code="403">User is not authorized to send messages (e.g., Admin role)</response>
        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessageAsync([FromBody] string? Content,
                                                          [FromQuery] int? reciverID)
        {
            try
            {
                var response = new ResponseWrapperDto();
                int ID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                string Role = User.FindFirstValue(ClaimTypes.Role).ToString();

                bool created = await messageSL.CreateMessageAsync(ID, Role, reciverID, Content);

                if (!created)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "An error has occurred.";
                    return BadRequest(response);
                }

                response.Success = true;
                response.Message = "Message sent successfully.";
                response.StatusCode = 200;
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                var response = new ResponseWrapperDto
                {
                    Success = false,
                    StatusCode = 403,
                    Message = ex.Message
                };
                return StatusCode(403, response);
            }
            catch (Exception ex)
            {
                var response = new ResponseWrapperDto
                {
                    Success = false,
                    StatusCode = 400,
                    Errors = ex,
                };
                return BadRequest(response);
            }
        }
        /// <summary>
        /// Deletes a message by marking it as deleted (soft delete).
        /// Only the original sender or an admin can delete a message.
        /// </summary>
        /// <param name="messageId">
        /// The ID of the message to delete, passed as a query parameter. Must be a positive integer.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a <c>ResponseWrapperDto</c> with status and message.
        /// - Returns <c>200 OK</c> if the message was successfully deleted.
        /// - Returns <c>400 Bad Request</c> if the message ID is invalid or an argument exception occurs.
        /// - Returns <c>403 Forbidden</c> if the user is not authorized to delete the message.
        /// - Returns <c>404 Not Found</c> if the message does not exist or is already deleted.
        /// - Returns <c>500 Internal Server Error</c> if an unexpected error occurs.
        /// </returns>
        /// <remarks>
        /// The role of the current user is extracted from their authentication token (JWT claims).
        /// Admins and the original sender are authorized to delete messages.
        /// The deletion is implemented as a soft delete (changing the message's status).
        /// </remarks>
        /// <response code="200">Message deleted successfully</response>
        /// <response code="400">Invalid message ID or bad input</response>
        /// <response code="403">User is not authorized to delete the message</response>
        /// <response code="404">Message not found or already deleted</response>
        /// <response code="500">Unexpected server error occurred</response>
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteMessageAsync([FromQuery] int messageId)
        {
            try
            {
                if (messageId < 1)
                {
                    var errorResponse = new ResponseWrapperDto
                    {
                        Success = false,
                        StatusCode = 400,
                        Message = "Invalid message ID"
                    };
                    return BadRequest(errorResponse);
                }

                string Role = User.FindFirstValue(ClaimTypes.Role).ToString();
                var response = new ResponseWrapperDto();

                bool Deleted = await messageSL.DeleteMessageAsync(messageId, Role);
                if (!Deleted)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Error has occurred";
                    return BadRequest(response);
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Message deleted successfully";
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                var response = new ResponseWrapperDto
                {
                    Success = false,
                    StatusCode = 400,
                    Message = ex.Message,
                };
                return BadRequest(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                var response = new ResponseWrapperDto
                {
                    Success = false,
                    StatusCode = 403,
                    Message = ex.Message
                };
                return StatusCode(403, response);
            }
            catch (InvalidOperationException ex)
            {
                var response = new ResponseWrapperDto
                {
                    Success = false,
                    StatusCode = 404,
                    Message = ex.Message
                };
                return NotFound(response);
            }
            catch (Exception ex)
            {
                var response = new ResponseWrapperDto
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "An unexpected error occurred: " + ex.Message
                };
                return StatusCode(500, response);
            }
        }
    }
}


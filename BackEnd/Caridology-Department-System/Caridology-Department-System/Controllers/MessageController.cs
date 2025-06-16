using System.Linq.Expressions;
using System.Security.Claims;
using Caridology_Department_System.Models;
using Caridology_Department_System.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Caridology_Department_System.Controllers
{ 
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly MessageSL messageSL;
        public MessageController(MessageSL messageSL)
        {
            this.messageSL = messageSL;
        }
        [HttpGet("GetMessages")]
        public async Task<IActionResult> GetMessagesAsync([FromQuery] int? patientid,[FromQuery] int? doctorid)
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
                    return  BadRequest(response);
                }
                if (Role.Equals("Patient"))
                {
                    patientid = ID;
                }
                else {
                    if (Role.Equals("Doctor"))
                    {
                        doctorid = ID;
                    }
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
        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessageAsync([FromBody] string? Content,
                                                          [FromQuery] int? reciverID)
        {
            try
            {
                var response = new ResponseWrapperDto();
                int ID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                string Role = User.FindFirstValue(ClaimTypes.Role).ToString();
                bool created = await messageSL.CreateMessageAsync(ID, Role, reciverID,Content);
                if (!created)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "error has occured";
                    return BadRequest(response);
                }
                response.Success = true;
                response.Message = " message sent successfuly";
                response.StatusCode = 200;
                return Ok(response);
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
    }
}


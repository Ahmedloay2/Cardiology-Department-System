using System.Reflection.Metadata.Ecma335;
using Caridology_Department_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Caridology_Department_System.Services
{
    public class MessageSL
    {
        private readonly DoctorSL doctorSL;
        private readonly DBContext dbContext;
        private readonly PatientSL patientSL;
        public MessageSL(DoctorSL doctorSL, DBContext dbContext, PatientSL patientSL)
        {
            this.doctorSL = doctorSL;
            this.dbContext = dbContext;
            this.patientSL = patientSL;
        }
        public async Task<List<MessageModel>> GetMessagesAsync(int? patientID, int? doctorID)
        {
            if (patientID == null || !patientID.HasValue)
            {
                throw new Exception("You must be logged in or choose patient");
            }
            if (doctorID == null || !doctorID.HasValue)
            {
                throw new Exception("You must be logged in or choose doctor");
            }
            List<MessageModel> messages = await dbContext.Messages.Where(m => m.StatusID != 3
                            && m.PatientID == patientID && m.DoctorID == doctorID)
                            .OrderBy(m => m.DateTime).ToListAsync();
            /*if (messages.Count == 0 || messages == null)
            {
                messages = new List<MessageModel>();
            }*/
            return messages;
        }
        public async Task<bool> CreateMessageAsync(int senderid, string senderRole,
                                                   int? reciverid, string? content)
        {
            if (string.IsNullOrEmpty(senderRole))
            {
                throw new Exception("You must login first.");
            }
            if (senderRole.Equals("Admin"))
            {
                throw new Exception("Only patients and doctors can send messages.");
            }
            if (reciverid == null || !reciverid.HasValue) { 
                throw new Exception("You must choose someone to send to.");
            }
            if (string.IsNullOrEmpty(content))
            {
                throw new Exception("You cannot send empty messages.");
            }
            MessageModel message = new MessageModel
            {
                Content = content,
                DateTime = DateTime.UtcNow,
                StatusID = 1
            };
            if (senderRole.Equals("Patient"))
            {
                message.Sender = "Patient";
                message.Receiver = "Doctor";
                message.PatientID = senderid;
                message.DoctorID = reciverid.Value;
            }
            else
            {
                message.Sender = "Doctor";
                message.Receiver = "Patient";
                message.PatientID = reciverid.Value;
                message.DoctorID = senderid;
            }

            await dbContext.Messages.AddAsync(message);
            await dbContext.SaveChangesAsync();
            return true;
        }
    }
}

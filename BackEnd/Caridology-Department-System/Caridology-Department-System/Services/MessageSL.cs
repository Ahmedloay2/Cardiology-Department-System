using System.Reflection.Metadata.Ecma335;
using Caridology_Department_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Caridology_Department_System.Services
{
    /// <summary>
    /// Service layer for handling message-related business logic, 
    /// including retrieving, sending, and soft-deleting messages exchanged between patients and doctors.
    /// </summary>
    public class MessageSL
    {
        private readonly DBContext dbContext;
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageSL"/> class  with the application's database context.
        /// <param name="dbContext">The application's database context</param>
        public MessageSL(DBContext dbContext)
        {
            this.dbContext = dbContext;
        }
        /// <summary>
        /// Retrieves all non-deleted messages exchanged between a specific doctor and patient, sorted by time.
        /// </summary>
        /// <param name="patientID">ID of the patient.</param>
        /// <param name="doctorID">ID of the doctor.</param>
        /// <returns>
        /// A list of messages exchanged between the specified patient and doctor, ordered by timestamp.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown if either <paramref name="patientID"/> or <paramref name="doctorID"/> is null or not a positive integer,
        /// indicating invalid or missing user context.
        /// </exception>
        public async Task<List<MessageModel>> GetMessagesAsync(int? patientID, int? doctorID)
        {
            if (patientID == null || !patientID.HasValue || patientID.Value <= 0)
            {
                throw new Exception("You must be logged in or choose patient");
            }
            if (doctorID == null || !doctorID.HasValue || doctorID.Value <= 0)
            {
                throw new Exception("You must be logged in or choose doctor");
            }

            List<MessageModel> messages = await dbContext.Messages
                .Where(m => m.StatusID != 3 && m.PatientID == patientID && m.DoctorID == doctorID)
                .OrderBy(m => m.DateTime)
                .ToListAsync();

            return messages;
        }

        /// <summary>
        /// Creates and stores a new message between a patient and a doctor.
        /// The sender and receiver roles are automatically assigned based on the sender's role.
        /// </summary>
        /// <param name="senderid">
        /// The ID of the user sending the message. Must be a patient or a doctor.
        /// </param>
        /// <param name="senderRole">
        /// The role of the sender. Must be either "Patient" or "Doctor". "Admin" is not allowed.
        /// </param>
        /// <param name="reciverid">
        /// The ID of the recipient. Must be a valid, non-null, positive integer.
        /// </param>
        /// <param name="content">
        /// The textual content of the message. Cannot be null or empty.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the message is successfully created and saved to the database.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when:
        /// - <paramref name="senderRole"/> is null or empty.
        /// - <paramref name="senderRole"/> is "Admin", who is not allowed to send messages.
        /// - <paramref name="reciverid"/> is null or less than 1.
        /// - <paramref name="content"/> is null or empty.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown when the sender has the "Admin" role, which is not allowed to send messages.
        /// </exception>
        public async Task<bool> CreateMessageAsync(int senderid, string senderRole,
                                                   int? reciverid, string? content)
        {
            if (string.IsNullOrEmpty(senderRole))
            {
                throw new Exception("You must login first.");
            }

            if (senderRole.Equals("Admin"))
            {
                throw new UnauthorizedAccessException("Only patients and doctors can send messages.");
            }

            if (reciverid == null || !reciverid.HasValue || reciverid.Value < 1)
            {
                throw new Exception("You must choose someone to send to or enter a valid ID.");
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
        /// <summary>
        /// Soft-deletes a message by setting its <c>StatusID</c> to 3.
        /// Only the original sender or an admin is authorized to perform this action.
        /// </summary>
        /// <param name="MessageID">The unique identifier of the message to delete.</param>
        /// <param name="senderRole">
        /// The role of the user attempting to delete the message.
        /// Must match the original sender's role or be "Admin".
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if the message exists and was successfully marked as deleted.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the <paramref name="MessageID"/> is null, invalid, or the message is not found or already deleted.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown when the user is not authorized to delete the message (not the sender and not an admin).
        /// </exception>
        public async Task<bool> DeleteMessageAsync(int? MessageID, string senderRole)
        {
            if (!MessageID.HasValue || MessageID < 1 || MessageID == null)
            {
                throw new InvalidOperationException("You must enter a valid id.");
            }

            MessageModel message = await dbContext.Messages
                .Where(m => m.MessageID == MessageID && m.StatusID != 3)
                .FirstOrDefaultAsync();

            if (message == null)
            {
                throw new InvalidOperationException("Message not found or it is already deleted.");
            }

            if (!message.Sender.Equals(senderRole, StringComparison.OrdinalIgnoreCase) &&
                !senderRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("You can only delete your own messages.");
            }

            message.StatusID = 3;
            await dbContext.SaveChangesAsync();
            return true;
        }
    }
}

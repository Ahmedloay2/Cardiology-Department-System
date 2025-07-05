using AutoMapper;
using Caridology_Department_System.Models;
using Caridology_Department_System.Requests.Appointment;
using Microsoft.EntityFrameworkCore;

namespace Caridology_Department_System.Services
{

    /// <summary>
    /// Service layer for handling appointment-related business logic, 
    /// including creating, retrieving, updating, and managing appointments between patients and doctors.
    /// </summary>
    public class AppointmentSL
    {
        private readonly DBContext dbContext;
        private readonly DoctorSL doctorSL;
        private readonly PatientSL patientSL;
        private readonly StatusSL statusSL;
        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppointmentSL"/> class.
        /// </summary>
        /// <param name="dbContext">The database context for data operations.</param>
        /// <param name="doctorSL">Service for doctor-related operations.</param>
        /// <param name="patientSL">Service for patient-related operations.</param>
        /// <param name="statusSL">Service for status-related operations.</param>
        /// <param name="mapper">AutoMapper instance for object mapping.</param>
        public AppointmentSL(DBContext dbContext,DoctorSL doctorSL,
                            StatusSL statusSL, IMapper mapper, PatientSL patientSL)
        {
            this.dbContext = dbContext;
            this.doctorSL = doctorSL;
            this.statusSL = statusSL;
            this.mapper = mapper;
            this.patientSL = patientSL;
        }

        /// <summary>
        /// Creates a new appointment between a patient and a doctor.
        /// </summary>
        /// <param name="PatientID">The unique identifier of the patient booking the appointment.</param>
        /// <param name="request">The appointment request containing the appointment date and doctor ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the appointment was created successfully.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the requested time slot is already taken or the doctor is unavailable.</exception>
        /// <exception cref="Exception">Thrown when the doctor doesn't exist, is inactive, or required status is missing from the database.</exception>
        public async Task<bool> CreateAppointmentAsync(int PatientID,AppointmentRequest request)
        {
            await doctorSL.DoctorExists(request.DoctorID);
            AppointmentModel existingAppointment = await GetAppointmentByDayAndTimeAsync(request.AppDate
                                                                                          , request.DoctorID
                                                                                          ,"Doctor");
            if (existingAppointment != null)
            {
                throw new InvalidOperationException("This time slot is already taken.");
            }
            StatusModel ConfirmedStatus = await statusSL.GetStatusByNameAsync("Confirmed");
            AppointmentModel appointment = new AppointmentModel
            {
                Date = request.AppDate,
                PatientID = PatientID,
                DoctorID = request.DoctorID,
                StatusID = ConfirmedStatus.StatusID,
            };
            await dbContext.AddAsync(appointment);
            await dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves all confirmed appointments for a specific doctor or patient on a given day.
        /// </summary>
        /// <param name="RequestedDay">The date for which to retrieve appointments.</param>
        /// <param name="ID">The unique identifier of the doctor or patient.</param>
        /// <param name="Role">The role of the user ("Doctor" or "Patient").</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of appointment data for the specified day.</returns>
        /// <exception cref="ArgumentException">Thrown when the ID is invalid or null.</exception>
        public async Task<List<AppointmentDataRequest>> GetAppointmentsDataByDayAsync(DateTime RequestedDay,
                                                                                  int? ID,string Role)
        {
            if ( !ID.HasValue || ID.Value < 1)
            {
                throw new ArgumentException("A valid ID must be provided.");
            }
            StatusModel ConfirmedStatus = await statusSL.GetStatusByNameAsync("Confirmed");
            List<AppointmentModel> appointments=new List<AppointmentModel>();
            if (Role.Equals("Doctor"))
            {
                await doctorSL.DoctorExists(ID);
                appointments = await dbContext.Appointments
                                .Where(a => a.StatusID == ConfirmedStatus.StatusID
                                && a.DoctorID == ID && a.Date.Date == RequestedDay.Date)
                                .Include(a => a.Patient)
                                .Include(a => a.Doctor)
                                .Include(a => a.Status)
                                .ToListAsync();
            }
            if (Role.Equals("Patient"))
            {
                await patientSL.PatientExists(ID);
                appointments = await dbContext.Appointments
                                                .Where(a => a.StatusID == ConfirmedStatus.StatusID
                                                && a.PatientID == ID && a.Date.Date == RequestedDay.Date)
                                                .Include(a => a.Patient)
                                                .Include(a => a.Doctor)
                                                .Include(a => a.Status)
                                                .ToListAsync();
            }
            List<AppointmentDataRequest> ViewAppointments= new List<AppointmentDataRequest>();
            if (appointments.Count==0)
            {
                return ViewAppointments;
            }
            foreach (AppointmentModel appointment in appointments)
            {
                AppointmentDataRequest ViewAppointment = mapper.Map<AppointmentDataRequest>(appointment);
                ViewAppointments.Add(ViewAppointment);
            }
            return ViewAppointments;
        }

        /// <summary>
        /// Retrieves a specific confirmed appointment for a doctor or patient at an exact date and time.
        /// </summary>
        /// <param name="RequestedDate">The exact date and time of the requested appointment.</param>
        /// <param name="ID">The unique identifier of the doctor or patient.</param>
        /// <param name="Role">The role of the user ("Doctor" or "Patient").</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the appointment data at the specified time.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no appointment exists at the specified date and time.</exception>
        public async Task<AppointmentDataRequest> GetAppointmentDataByDayAndTimeAsync(DateTime RequestedDate,
                                                                                  int? ID,string Role)
        {
            AppointmentModel appointment = await GetAppointmentByDayAndTimeAsync(RequestedDate,ID, Role);
            if (appointment == null)
            {
                throw new InvalidOperationException("No appointment found at the specified date and time.");
            }
            AppointmentDataRequest ViewAppointment = mapper.Map < AppointmentDataRequest > (appointment);
            return ViewAppointment;
        }

        /// <summary>
        /// Reschedules an existing appointment to a new date and time.
        /// </summary>
        /// <param name="request">The reschedule request containing the original appointment date, new date, and doctor ID.</param>
        /// <param name="PatientID">The unique identifier of the patient requesting the reschedule.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the reschedule was successful.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the original appointment doesn't exist or the new time slot is taken.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when a patient tries to reschedule another patient's appointment.</exception>
        public async Task<bool> RescheduleAppointmentAsync(RescheduleAppointmentRequest request,int PatientID)
        {
            AppointmentModel appointment = await GetAppointmentByDayAndTimeAsync(request.AppDate,request.DoctorID,"Doctor");
            if (appointment == null)
            {
                throw new InvalidOperationException("Original appointment not found or already cancelled.");
            }
            if (PatientID != appointment.PatientID)
            {
                throw new UnauthorizedAccessException("Only the patient who booked the appointment can reschedule it.");
            }
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                StatusModel ConfirmedStatus = await statusSL.GetStatusByNameAsync("Confirmed");
                StatusModel PostponedStatus = await statusSL.GetStatusByNameAsync("Postponed");
                appointment.StatusID = PostponedStatus.StatusID;
                await dbContext.SaveChangesAsync();
                bool Created = await CreateAppointmentAsync(appointment.PatientID, new AppointmentRequest { AppDate=request.NewDate,DoctorID=request.DoctorID});
                if (!Created)
                {
                    transaction.Rollback();
                    return false;
                }
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Cancels an existing appointment.
        /// </summary>
        /// <param name="request">The appointment request containing the appointment date and doctor ID.</param>
        /// <param name="PatientID">The unique identifier of the patient requesting the cancellation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the cancellation was successful.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the appointment doesn't exist or is already cancelled.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when a patient tries to cancel another patient's appointment.</exception>
        public async Task<bool> CancelAppointmentAsync(AppointmentRequest request,int PatientID)
        {
            AppointmentModel appointment = await GetAppointmentByDayAndTimeAsync(request.AppDate, request.DoctorID,"Doctor");
            if (appointment == null)
            {
                throw new InvalidOperationException("Appointment not found or already cancelled.");
            }
            if (PatientID != appointment.PatientID)
            {
                throw new UnauthorizedAccessException("Only the patient who booked the appointment can cancel it.");
            }
                StatusModel CancelledStatus = await statusSL.GetStatusByNameAsync("Cancelled");
                appointment.StatusID = CancelledStatus.StatusID;
                await dbContext.SaveChangesAsync();
                return true;            
        }

        /// <summary>
        /// Marks an appointment as completed or missed by the attending doctor.
        /// </summary>
        /// <param name="AppointmentID">The unique identifier of the appointment to mark.</param>
        /// <param name="DoctorID">The unique identifier of the doctor marking the appointment.</param>
        /// <param name="IsCompleted">True if the appointment was completed; false if it was missed.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the appointment was marked successfully.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the appointment is not found or when trying to mark a future appointment.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the doctor is not authorized to mark this appointment.</exception>
        public async Task<bool> MarkAppointmentAsync(int AppointmentID, int DoctorID, bool IsCompleted)
        {
            AppointmentModel appointment = await dbContext.Appointments.SingleOrDefaultAsync(a=> a.APPID== AppointmentID);
            if (appointment == null)
            {
                throw new InvalidOperationException("Appointment not found.");
            }
            if (appointment.DoctorID != DoctorID)
            {
                throw new UnauthorizedAccessException("You are not authorized to mark this appointment.");
            }
            if (appointment.Date > DateTime.Now)
            {
                throw new InvalidOperationException("Cannot mark future appointments.");
            }
            StatusModel CompletedStatus = await statusSL.GetStatusByNameAsync("Completed");
            StatusModel MissedStatus = await statusSL.GetStatusByNameAsync("Missed");
            if (IsCompleted)
            {
                appointment.StatusID = CompletedStatus.StatusID;
            }
            else
            {
                appointment.StatusID = MissedStatus.StatusID;
            }
            await dbContext.SaveChangesAsync();
            return true;
        }
        
        /// <summary>
        /// Retrieves an appointment by its unique identifier.
        /// </summary>
        /// <param name="appointmentID">The unique identifier of the appointment to retrieve. Must be a positive integer.</param>
        /// <returns>An <see cref="AppointmentModel"/> representing the appointment with the specified identifier.</returns>
        /// <exception cref="Exception">Thrown if no appointment with the specified identifier is found.</exception>
        private async Task<AppointmentModel> GetAppointmentByIDAsync(int appointmentID)
        {
            AppointmentModel? appointment = await dbContext.Appointments.
                                        Where(a=> a.APPID==appointmentID).
                                        FirstOrDefaultAsync();
            if (appointment == null)
            {
                throw new Exception("Appointment not found");
            }
            return appointment;
        }

        /// <summary>
        /// Determines whether the specified appointment is marked as completed.
        /// </summary>
        /// <remarks>This method checks the status of the appointment by comparing its status ID to the ID
        /// of the "Completed" status.</remarks>
        /// <param name="appointmentID">The unique identifier of the appointment to check.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains  true if the appointment is
        /// completed; otherwise, false.</returns>
        public async Task<bool> IsCompletedAsync(int appointmentID)
        {
            StatusModel CompletedStatus = await statusSL.GetStatusByNameAsync("Completed");
            AppointmentModel appointment = await GetAppointmentByIDAsync(appointmentID);
            if (appointment.StatusID != CompletedStatus.StatusID)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the specified appointment is associated with the given doctor.
        /// </summary>
        /// <remarks>This method retrieves the appointment details asynchronously and compares the
        /// associated doctor ID  with the provided <paramref name="doctorID"/>. Ensure that the <paramref
        /// name="appointmentID"/>  corresponds to a valid appointment.</remarks>
        /// <param name="appointmentID">The unique identifier of the appointment to check.</param>
        /// <param name="doctorID">The unique identifier of the doctor to compare against.</param>
        /// <returns><see langword="true"/> if the appointment is associated with the specified doctor;  otherwise, <see
        /// langword="false"/>.</returns>
        public async Task<bool> isSameDoctor(int appointmentID,int doctorID)
        {
            AppointmentModel appointment = await GetAppointmentByIDAsync(appointmentID);
            if (appointment.DoctorID != doctorID)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the specified appointment is associated with the given patient.
        /// </summary>
        /// <remarks>This method retrieves the appointment details asynchronously and compares the
        /// associated patient ID  with the provided <paramref name="patientID"/>. Ensure that the appointment ID
        /// corresponds to a valid  appointment in the system.</remarks>
        /// <param name="appointmentID">The unique identifier of the appointment to check.</param>
        /// <param name="patientID">The unique identifier of the patient to compare against the appointment.</param>
        /// <returns><see langword="true"/> if the appointment is associated with the specified patient;  otherwise, <see
        /// langword="false"/>.</returns>
        public async Task<bool> isSamePatient(int appointmentID, int patientID)
        {
            AppointmentModel appointment = await GetAppointmentByIDAsync(appointmentID);
            if (appointment.PatientID != patientID)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Retrieves a confirmed appointment for a doctor or patient at a specific date and time.
        /// </summary>
        /// <param name="RequestedDate">The exact date and time of the appointment.</param>
        /// <param name="ID">The unique identifier of the doctor or patient.</param>
        /// <param name="Role">The role ("Doctor" or "Patient").</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the appointment model or null if not found.</returns>
        private async Task<AppointmentModel> GetAppointmentByDayAndTimeAsync(DateTime RequestedDate, int? ID, string Role)
        {
            StatusModel ConfirmedStatus = await statusSL.GetStatusByNameAsync("Confirmed");
            AppointmentModel appointment = new AppointmentModel();
            if (Role.Equals("Doctor"))
            {
                await doctorSL.DoctorExists(ID);
                appointment = await dbContext.Appointments
                                                .Where(a => a.StatusID == ConfirmedStatus.StatusID
                                                && a.DoctorID == ID && a.Date == RequestedDate)
                                                .Include(a => a.Patient)
                                                .Include(a => a.Status)
                                                .Include(a => a.Doctor)
                                                .SingleOrDefaultAsync();
            }
            if (Role.Equals("Patient"))
            {
                await patientSL.PatientExists(ID);
                appointment = await dbContext.Appointments
                                                .Where(a => a.StatusID == ConfirmedStatus.StatusID
                                                && a.PatientID == ID && a.Date == RequestedDate)
                                                .Include(a => a.Patient)
                                                .Include(a => a.Status)
                                                .Include(a => a.Doctor)
                                                .SingleOrDefaultAsync();
            }
            return appointment;
        }

    }
}

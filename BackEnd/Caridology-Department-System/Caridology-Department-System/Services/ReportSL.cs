
using Caridology_Department_System.Models;
using Caridology_Department_System.Repository;
using Caridology_Department_System.Requests.Report;

namespace Caridology_Department_System.Services
{
    /// <summary>
    /// Provides functionality for managing reports associated with appointments, including creation,  updating, and
    /// retrieval operations.
    /// </summary>
    /// <remarks>The <see cref="ReportSL"/> class acts as a service layer for handling report-related
    /// operations.  It validates business rules and interacts with the underlying repository to persist report data. 
    /// This class ensures that reports are created, updated, and retrieved in accordance with appointment  and
    /// doctor-specific constraints.</remarks>
    public class ReportSL : IReportSL
    {
        private readonly IReportRepository reportRepository;
        private readonly StatusSL statusSL;
        private readonly AppointmentSL appointmentSL;
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportSL"/> class.
        /// </summary>
        /// <param name="reportRepository">The repository used for accessing and managing report data.</param>
        /// <param name="statusSL">The service layer responsible for handling status-related operations.</param>
        /// <param name="appointmentSL">The service layer responsible for managing appointment-related operations.</param>
        public ReportSL(IReportRepository reportRepository, StatusSL statusSL, AppointmentSL appointmentSL)
        {
            this.reportRepository = reportRepository;
            this.statusSL = statusSL;
            this.appointmentSL = appointmentSL;
        }
        /// <summary>
        /// Creates a new report for a completed appointment.
        /// </summary>
        /// <remarks>This method validates several conditions before creating the report: <list
        /// type="bullet"> <item>If a report already exists for the specified appointment, an exception is
        /// thrown.</item> <item>If the doctor attempting to create the report is not the doctor associated with the
        /// appointment, an exception is thrown.</item> <item>If the appointment is not marked as completed, an
        /// exception is thrown.</item> </list> The report is created with an "Active" status and saved to the
        /// repository.</remarks>
        /// <param name="request">The data transfer object containing the details of the report to be created, including the appointment ID
        /// and prescription.</param>
        /// <param name="doctorID">The ID of the doctor attempting to create the report. Must match the doctor associated with the appointment.</param>
        /// <returns><see langword="true"/> if the report is successfully created; otherwise, an exception is thrown.</returns>
        /// <exception cref="Exception">Thrown if a report already exists for the specified appointment. Thrown if the doctor attempting to create
        /// the report is not associated with the appointment. Thrown if the appointment is not marked as completed.</exception>
        public async Task<bool> CreateReportAsync(ReportDto request,int doctorID)
        {
            ReportModel? report= await reportRepository.GetByAppointmentIDAsync(request.appointmentID);
            if (report is not null)
            {
                throw new Exception(
                    $"there is already created report for this appointment with ID number {report.ReportID}");
            }
            bool isSameDoctor = await appointmentSL.isSameDoctor(request.appointmentID, doctorID);
            if (!isSameDoctor)
            {
                throw new Exception("you can not generate a report for appointment is not yours");
            }
            bool appointmentCompleted = await appointmentSL.IsCompletedAsync(request.appointmentID);
            if (!appointmentCompleted)
            {
                throw new Exception("you can not generate a report for uncompleted appointmet");
            }
            StatusModel ActiveStatus =await statusSL.GetStatusByNameAsync("Active");
            report = new ReportModel
            {
                AppointmentID = request.appointmentID,
                CreatedAt = DateTime.UtcNow,
                Prescription = request.prescription,
                StatusID = ActiveStatus.StatusID,                
            };       
            await reportRepository.AddReportAsync(report);
            await reportRepository.SaveAsync();
            return true;
        }

        /// <summary>
        /// Updates an existing report associated with a specific appointment.
        /// </summary>
        /// <remarks>This method ensures that the report belongs to the specified doctor before allowing
        /// updates.  If the report does not exist or the doctor does not have permission to update it, an exception is
        /// thrown.</remarks>
        /// <param name="request">The data transfer object containing the updated report details, including the appointment ID and
        /// prescription.</param>
        /// <param name="doctorID">The unique identifier of the doctor attempting to update the report.</param>
        /// <returns><see langword="true"/> if the report was successfully updated; otherwise, <see langword="false"/> if no
        /// changes were made.</returns>
        /// <exception cref="Exception">Thrown if no report exists for the specified appointment ID. Thrown if the doctor does not have permission
        /// to update the report.</exception>
        public async Task<bool> UpdateReportAsync(ReportDto request , int doctorID)
        {
            ReportModel? report = await reportRepository.GetByAppointmentIDAsync(request.appointmentID);
            if (report is null)
            {
                throw new Exception(
                    $"there is no report created for appointment number {request.appointmentID}");
            }
            bool isSameDoctor = await appointmentSL.isSameDoctor(request.appointmentID, doctorID);
            if (!isSameDoctor)
            {
                throw new Exception("you can not update a report is not yours");
            }
            bool changed = false;
            if (!string.Equals(request.prescription?.Trim(), report.Prescription?.Trim(), StringComparison.Ordinal))
            {
                report.Prescription = request.prescription;
                changed = true;
            }
            if (changed)
            {
                report.UpdatedAt = DateTime.UtcNow;
            }
            await reportRepository.SaveAsync();
            return changed;
        }
        /// <summary>
        /// Retrieves the report associated with the specified appointment ID.
        /// </summary>
        /// <param name="appointmentID">The unique identifier of the appointment for which the report is to be retrieved. Must be a positive
        /// integer.</param>
        /// <returns>A <see cref="ReportModel"/> object representing the report for the specified appointment.</returns>
        /// <exception cref="Exception">Thrown if no report has been generated for the specified appointment ID.</exception>
        public async Task<ReportModel> GetReportByAppointmentIdAsync(int appointmentID)
        {
            ReportModel? report = await reportRepository.GetByAppointmentIDAsync(appointmentID);
            if (report is null)
            {
                throw new Exception("Report is not generated yet");
            }
            return report;
        }
    }
}

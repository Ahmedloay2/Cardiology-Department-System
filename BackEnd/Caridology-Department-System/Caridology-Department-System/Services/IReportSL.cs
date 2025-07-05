using Caridology_Department_System.Models;
using Caridology_Department_System.Requests.Report;

namespace Caridology_Department_System.Services
{
    /// <summary>
    /// Defines the contract for report-related operations, including creating, updating, and retrieving reports.
    /// </summary>
    /// <remarks>This interface provides methods for managing reports in an asynchronous manner.
    /// Implementations of this interface should ensure proper validation of input parameters and handle any necessary
    /// business logic for report creation, updates, and retrieval.</remarks>
    public interface IReportSL
    {
        /// <summary>
        /// Asynchronously creates a report based on the provided request data and associates it with the specified doctor.
        /// </summary>
        /// <remarks>Ensure that the <paramref name="request"/> contains all required fields before calling this method.
        /// The operation may fail if the provided data is invalid or if the doctor ID does not exist.</remarks>
        /// <param name="request">The data required to create the report, including patient information and report details. Cannot be null.</param>
        /// <param name="doctorID">The unique identifier of the doctor to associate with the report. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the report was
        /// successfully created;  otherwise, <see langword="false"/>.</returns>
        public Task<bool> CreateReportAsync(ReportDto request, int doctorID);
        /// <summary>
        /// Updates an existing report with the provided data.
        /// </summary>
        /// <remarks>Ensure that the <paramref name="request"/> contains valid data and that the <paramref
        /// name="doctorID"/> corresponds to an existing doctor.</remarks>
        /// <param name="request">The report data to update, including any changes to its fields.</param>
        /// <param name="doctorID">The unique identifier of the doctor associated with the report.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the update
        /// was successful; otherwise, <see langword="false"/>.</returns>
        public Task<bool> UpdateReportAsync(ReportDto request, int doctorID);
        /// <summary>
        /// Retrieves the report associated with the specified appointment ID.
        /// </summary>
        /// <remarks>This method performs an asynchronous operation to fetch the report data. Ensure that
        /// the appointment ID provided is valid and corresponds to an existing appointment.</remarks>
        /// <param name="appointmentID">The unique identifier of the appointment for which the report is to be retrieved. Must be a positive
        /// integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the  <see cref="ReportModel"/>
        /// associated with the specified appointment ID. If no report is found, the result will be <see
        /// langword="null"/>.</returns>
        public  Task<ReportModel> GetReportByAppointmentIdAsync(int appointmentID);

    }
}

using Caridology_Department_System.Models;
using Caridology_Department_System.Services;

namespace Caridology_Department_System.Repository
{
    /// <summary>
    /// Defines a contract for managing reports in the system.
    /// </summary>
    /// <remarks>This interface provides methods for saving, adding, and retrieving reports asynchronously.
    /// Implementations of this interface are responsible for interacting with the underlying data store to perform
    /// these operations. Ensure that all preconditions for the methods are met before invoking them.</remarks>
    public interface IReportRepository
    {
        /// <summary>
        /// Saves the current state asynchronously.
        /// </summary>
        /// <remarks>This method performs an asynchronous save operation to persist the current state.
        /// Ensure that any required data is properly initialized before calling this method.</remarks>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public  Task SaveAsync();
        /// <summary>
        /// Asynchronously adds a new report to the system.
        /// </summary>
        /// <remarks>This method adds the provided report to the system's data store. Ensure that the 
        /// <paramref name="report"/> object is properly initialized before calling this method.</remarks>
        /// <param name="report">The report to be added. Must not be <see langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task AddReportAsync(ReportModel report);
        /// <summary>
        /// Retrieves a report associated with the specified appointment ID.
        /// </summary>
        /// <remarks>Use this method to retrieve detailed information about a specific appointment's
        /// report. If no report exists for the given appointment ID, the method returns <see
        /// langword="null"/>.</remarks>
        /// <param name="appointmentID">The unique identifier of the appointment for which the report is to be retrieved. Must be a positive
        /// integer.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the  <see cref="ReportModel"/>
        /// associated with the specified appointment ID, or <see langword="null"/>  if no report is found.</returns>
        public Task<ReportModel?> GetByAppointmentIDAsync(int appointmentID);

    }
}

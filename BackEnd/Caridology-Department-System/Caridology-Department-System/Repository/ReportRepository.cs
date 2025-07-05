
using Caridology_Department_System.Models;
using Caridology_Department_System.Services;
using Microsoft.EntityFrameworkCore;

namespace Caridology_Department_System.Repository
{
    /// <summary>
    /// Provides methods for managing reports in the database, including adding, retrieving, and saving reports.
    /// </summary>
    /// <remarks>The <see cref="ReportRepository"/> class serves as a repository for handling report-related
    /// operations. It interacts with the underlying database context and a status service layer to perform tasks such
    /// as adding new reports, retrieving reports by appointment ID, and saving changes to the database.</remarks>
    public class ReportRepository : IReportRepository
    {
        private readonly DBContext dbContext;
        private readonly StatusSL statusSL;
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context used to interact with the underlying data store. This parameter cannot be null.</param>
        /// <param name="statusSL">The service layer responsible for handling status-related operations. This parameter cannot be null.</param>
        public ReportRepository(DBContext dbContext,StatusSL statusSL)
        {
            this.dbContext = dbContext;
            this.statusSL = statusSL;
        }
        /// <summary>
        /// Asynchronously adds a new report to the database.
        /// </summary>
        /// <remarks>This method adds the specified <see cref="ReportModel"/> instance to the database
        /// context. Changes are not persisted to the database until <c>SaveChangesAsync</c> is called on the
        /// context.</remarks>
        /// <param name="report">The report to add. This parameter cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AddReportAsync(ReportModel report)
        {
            await dbContext.Reports.AddAsync(report);
        }
        /// <summary>
        /// Retrieves a report associated with the specified appointment ID, excluding reports marked with a "Deleted"
        /// status.
        /// </summary>
        /// <remarks>This method filters out reports with a "Deleted" status before attempting to retrieve
        /// a single report. Ensure that the appointment ID provided corresponds to a valid appointment in the
        /// system.</remarks>
        /// <param name="appointmentID">The unique identifier of the appointment for which the report is to be retrieved.</param>
        /// <returns>A <see cref="ReportModel"/> object representing the report associated with the specified appointment ID,  or
        /// <see langword="null"/> if no matching report is found.</returns>
        public async Task<ReportModel?> GetByAppointmentIDAsync(int appointmentID)
        {
            StatusModel DeletedStatus = await statusSL.GetStatusByNameAsync("Deleted");
            return await dbContext.Reports.Where(r => r.AppointmentID == appointmentID && r.StatusID != DeletedStatus.StatusID).SingleOrDefaultAsync();
        }
        /// <summary>
        /// Saves all pending changes to the database asynchronously.
        /// </summary>
        /// <remarks>This method commits all changes tracked by the underlying <see cref="DbContext"/> to
        /// the database. It should be called after making modifications to entities to persist those changes.</remarks>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public async Task SaveAsync()
        {
            await dbContext.SaveChangesAsync();
        }

    }
}

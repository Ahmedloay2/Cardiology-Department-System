namespace Caridology_Department_System.Services
{
    public interface IReportSL
    {
        public Task<bool> CreateReportAsync();
    }
    public class ReportSL:IReportSL 
    {
        public async Task<bool> CreateReportAsync()
        {
            throw new NotImplementedException();
        }
    }
}

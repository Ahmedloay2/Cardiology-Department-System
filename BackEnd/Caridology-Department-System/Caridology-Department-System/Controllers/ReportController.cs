using Caridology_Department_System.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Caridology_Department_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportSL reportSL;
        public ReportController(IReportSL reportSL)
        {
            this.reportSL = reportSL;
        }
    }
}

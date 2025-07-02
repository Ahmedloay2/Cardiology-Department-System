using Caridology_Department_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Caridology_Department_System.Services
{
    public class StatusSL
    {
        private readonly DBContext dbContext;
        public StatusSL(DBContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<StatusModel> GetStatusByNameAsync(string name)
        {
            StatusModel status = await dbContext.Statuses.FirstOrDefaultAsync(s => s.Name == name);
            if (status == null)
            {
                throw new Exception($"{name} status is not configured in the database.");
            }
            return status;
        }
    }
}

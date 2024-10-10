using Microsoft.EntityFrameworkCore;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Data;
using static System.Collections.Specialized.BitVector32;

namespace StudentProjectsCenterSystem.Infrastructure.Repositories
{
    public class ProjectDetailsRepository : GenericRepository<ProjectDetailEntity>, IProjectDetailsRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ProjectDetailsRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }

    }
}

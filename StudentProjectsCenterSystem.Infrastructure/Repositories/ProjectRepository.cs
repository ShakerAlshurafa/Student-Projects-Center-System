using Microsoft.EntityFrameworkCore;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Data;

namespace StudentProjectsCenterSystem.Infrastructure.Repositories
{
    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ProjectRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Project> GetByIdWithDetails(int id)
        {
            var model = await dbContext.Projects.Include(p => p.ProjectDetails).FirstOrDefaultAsync(p => p.Id == id);
            return model;
        }
    }
}

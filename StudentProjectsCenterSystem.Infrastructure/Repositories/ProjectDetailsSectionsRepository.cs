using Microsoft.EntityFrameworkCore;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Data;

namespace StudentProjectsCenterSystem.Infrastructure.Repositories
{
    public class ProjectDetailsSectionsRepository : GenericRepository<ProjectDetailsSection>, IProjectDetailsSectionsRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ProjectDetailsSectionsRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<int> GetSectionId(string name)
        {
            // Retrieves the Id of the section by name, returns 0 if no section is found.
            var section = await dbContext.ProjectDetailsSections
                            .FirstOrDefaultAsync(x => x.Name == name);

            return section?.Id ?? 0;
        }

    }
}

using Microsoft.EntityFrameworkCore;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentProjectsCenterSystem.Infrastructure.Repositories
{
    public class ProjectDetailsSectionsRepository : GenericRepository<ProjectDetailsSection>, IProjectDetailsSectionsRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ProjectDetailsSectionsRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<ProjectDetailsSection>> GetAllByProjecId(int projectId)
        {
            var sections = await dbContext.ProjectDetailsSections
                            .Include(p => p.Project)
                            .Where(p => p.ProjectId == projectId)
                            .ToListAsync();
            return sections;
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

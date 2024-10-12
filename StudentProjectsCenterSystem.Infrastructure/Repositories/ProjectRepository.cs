using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentProjectsCenterSystem.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Data;
using System.Linq.Expressions;

namespace StudentProjectsCenterSystem.Infrastructure.Repositories
{
    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ProjectRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            this.dbContext = dbContext;
        }


        public async Task<List<Project>> GetAllWithUser(Expression<Func<Project, bool>>? filter = null, int PageSize = 6, int PageNumber = 1)
        {
            var model = await dbContext.Projects
                        .Include(p => p.Workgroup)
                        .Include(p => p.UserProjects)
                        .ThenInclude(up => up.User) 
                        .ToListAsync();

            return model;
        }

        public async Task<ProjectDetailsDTO> GetByIdWithDetails(int id)
        {
            var model = await dbContext.Projects.FirstOrDefaultAsync(p => p.Id == id);
            var details = await dbContext.ProjectDetails
               .Where(d => d.ProjectDetailsSection != null && d.ProjectDetailsSection.ProjectId == id)
               .ToListAsync();

            var viewModel = new ProjectDetailsDTO
            {
                Id = id,
                Name = model?.Name ?? "",
                Overview = model?.Overview ?? "",
                Status = model?.Status ?? "",
                ProjectDetails = details,
                StartDate = model?.StartDate ?? new DateTime(),
                EndDate = model?.EndDate ?? new DateTime()
            };

            return viewModel;
        }

    }
}

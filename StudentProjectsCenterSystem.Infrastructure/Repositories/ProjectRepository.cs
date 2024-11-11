using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentProjectsCenterSystem.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.Entities.DTO.ProjectDetails;
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
            var project = await dbContext.Projects
                .Include(p => p.UserProjects)
                .ThenInclude(up => up.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            var details = await dbContext.ProjectDetails
                .Include(p => p.ProjectDetailsSection)
                .Where(d => d.ProjectDetailsSection != null && d.ProjectDetailsSection.ProjectId == id)
                .ToListAsync();

            var viewModel = new ProjectDetailsDTO
            {
                Id = project.Id,
                Name = project?.Name ?? "",
                Overview = project?.Overview ?? "",
                Status = project?.Status ?? "",
                ProjectDetails = details.Select(d => new ProjectDetailEntityDTO
                {
                    detail = d,
                    SectionName = d?.ProjectDetailsSection?.Name ?? ""
                }),

                SupervisorName = project?.UserProjects?.FirstOrDefault(up => up.Role == "Supervisor")?.User?.FirstName
                                + " " + project?.UserProjects?.FirstOrDefault(up => up.Role == "Supervisor")?.User?.LastName
                                ?? "No Supervisor Assigned",
                CoSupervisorName = project?.UserProjects?.FirstOrDefault(up => up.Role == "Co-Supervisor")?.User?.FirstName
                                + " " + project?.UserProjects?.FirstOrDefault(up => up.Role == "Co-Supervisor")?.User?.LastName
                                ?? "No Co-Supervisor Assigned",
                CustomerName = project?.UserProjects?.FirstOrDefault(up => up.Role == "Customer")?.User?.FirstName
                                + " " + project?.UserProjects?.FirstOrDefault(up => up.Role == "Customer")?.User?.LastName
                                ?? "No Customer Assigned",
                Company = project?.UserProjects?.FirstOrDefault(up => up.Role == "Customer")?.User?.CompanyName
                                ?? "No Customer Assigned",
                Team = project?.UserProjects?.Where(up => up.Role == "Student")?
                                .Select(up => up?.User?.FirstName + " " + up?.User?.LastName)
                                .ToList()
                                ?? new List<string>(),
                Favorite = project?.Favorite ?? false,

                StartDate = project?.StartDate ?? new DateTime(),
                EndDate = project?.EndDate ?? new DateTime()
            };

            return viewModel;
        }


    }
}

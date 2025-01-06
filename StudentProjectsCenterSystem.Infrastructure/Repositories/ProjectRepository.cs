using Microsoft.EntityFrameworkCore;
using StudentProjectsCenter.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.Entities.DTO.ProjectDetails;
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

            var supervisor = project?.UserProjects?
                    .FirstOrDefault(up => up.Role == "supervisor" && !up.IsDeleted);
            var co_supervisor = project?.UserProjects?
                .FirstOrDefault(up => up.Role == "co-supervisor" && !up.IsDeleted);
            var customer = project?.UserProjects?
                .FirstOrDefault(up => up.Role == "customer" && !up.IsDeleted);

            var viewModel = new ProjectDetailsDTO
            {
                Id = project.Id,
                Name = project?.Name ?? "",
                Overview = project?.Overview ?? "",
                Status = project?.Status ?? "",
                ProjectDetails = details
                    .GroupBy(d => d?.ProjectDetailsSection?.Name ?? string.Empty)
                    .Select(group => new ProjectDetailEntityDTO
                    {
                        SectionName = group.Key,
                        details = group.ToList()
                    }).ToList(),

                SupervisorJoinAt = supervisor?.JoinAt,
                SupervisorId = supervisor?.UserId ?? "",
                SupervisorName = supervisor?.User?.FirstName + " " + supervisor?.User?.LastName
                                    ?? "No Supervisor Assigned",
                CoSupervisorJoinAt = co_supervisor?.JoinAt,
                CoSupervisorId = co_supervisor?.UserId ?? "",
                CoSupervisorName = co_supervisor?.User?.FirstName + " " + co_supervisor?.User?.LastName
                                    ?? "No Co-Supervisor Assigned",
                CustomerId = customer?.UserId ?? "",
                CustomerName = customer?.User?.FirstName + " " + customer?.User?.LastName
                                    ?? "No Customer Assigned",
                Company = customer?.User?.CompanyName
                                    ?? "No Customer Assigned",
                Team = project?.UserProjects?.Where(up => up.Role == "student" && !up.IsDeleted)?
                                    .Select(up => new TeamDTO
                                    {
                                        Id = up.User?.Id ?? string.Empty,
                                        Name = $"{up.User?.FirstName} {up.User?.LastName}".Trim()
                                    })
                                    .ToList()
                                    ?? new List<TeamDTO>(),

                Favorite = project?.Favorite ?? false,

                StartDate = project?.StartDate ?? new DateTime(),
                EndDate = project?.EndDate,
                ChangeStatusNotes = project?.ChangeStatusNotes,
                ChangeStatusAt = project?.ChangeStatusAt
            };

            return viewModel;
        }

    }
}

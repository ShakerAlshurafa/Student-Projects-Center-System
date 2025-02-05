using Microsoft.EntityFrameworkCore;
using StudentProjectsCenter.Core.Entities.DTO.Project;
using StudentProjectsCenter.Core.Entities.DTO.ProjectDetails;
using StudentProjectsCenter.Core.Entities.DTO.Users;
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

            var sections = await dbContext.ProjectDetailsSections
                .Include(p => p.ProjectDetails)
                .Where(d => d.ProjectId == id)
                .ToListAsync();
            var details = await dbContext.ProjectDetails
                .Include(p => p.ProjectDetailsSection)
                .Where(d => d.ProjectDetailsSection != null && d.ProjectDetailsSection.ProjectId == id)
                .ToListAsync();

            var supervisor = project?.UserProjects?
                    .FirstOrDefault(up => up.Role == "supervisor" && !up.IsDeleted);
            var co_supervisor = project?.UserProjects?
                .Where(up => up.Role == "co-supervisor" && !up.IsDeleted)
                .Select(u => new CoSupervisorDTO() 
                { 
                    CoSupervisorId = u.UserId,
                    CoSupervisorName = string.Join(" ", [u?.User?.FirstName, u?.User?.MiddleName, u?.User?.LastName]),
                    CoSupervisorJoinAt = u?.JoinAt
                }).ToList();
            var customer = project?.UserProjects?
                .FirstOrDefault(up => up.Role == "customer" && !up.IsDeleted);

            var viewModel = new ProjectDetailsDTO
            {
                Id = project.Id,
                Name = project?.Name ?? "",
                Overview = project?.Overview ?? "",
                Status = project?.Status ?? "",
                ProjectDetails = sections
                    .GroupBy(section => new
                    {
                        SectionID = section?.Id,
                        SectionName = section?.Name ?? string.Empty
                    })
                    .Select(group => new ProjectDetailEntityDTO
                    {
                        SectionId = group.Key.SectionID,
                        SectionName = group.Key.SectionName,
                        details = group.SelectMany(section => section.ProjectDetails.Select(detail => new ProjectDetailDTO
                        {
                            Id = detail.Id,
                            Title = detail.Title,
                            Description = detail.Description,
                            ImagePath = detail.ImagePath
                        })).ToList()
                    })
                    .ToList(),

                SupervisorJoinAt = supervisor?.JoinAt,
                SupervisorId = supervisor?.UserId ?? "",
                SupervisorName = supervisor?.User?.FirstName + " " + supervisor?.User?.LastName
                                    ?? "No Supervisor Assigned",
                coSupervisors = co_supervisor,
                CustomerId = customer?.UserId ?? "",
                CustomerName = customer?.User?.FirstName + " " + customer?.User?.LastName
                                    ?? "No Customer Assigned",
                Company = customer?.User?.CompanyName
                                    ?? "No Customer Assigned",
                Team = project?.UserProjects?.Where(up => up.Role == "student" && !up.IsDeleted)?
                                    .Select(up => new TeamDTO
                                    {
                                        Id = up.User?.Id ?? string.Empty,
                                        Name = $"{up.User?.FirstName} {up.User?.LastName}".Trim(),
                                        JoinAt = up.JoinAt
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

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentProjectsCenter.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Security.Claims;

namespace StudentProjectsCenter.Controllers.project
{
    [Route("api/user/projects")]
    [ApiController]
    public class ProjectsUserController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly UserManager<LocalUser> userManager;

        public ProjectsUserController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<LocalUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
        }


        [Authorize]
        [HttpGet("get-all-for-user/{PageSize}/{PageNumber}")]
        //[ResponseCache(CacheProfileName = ("defaultCache"))]
        public async Task<ActionResult<ApiResponse>> GetAllForUser(
            int PageSize = 6,
            int PageNumber = 1)
        {
            // Retrieve the logged-in user's ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse(401, "User not Find."));
            }

            // Filter workgroups where the logged-in user is associated
            Expression<Func<StudentProjectsCenterSystem.Core.Entities.project.Project, bool>> filter = x =>
                x.UserProjects.Any(up => up.UserId == userId && !up.IsDeleted);

            var projects = await unitOfWork.projectRepository.GetAll(filter, PageSize, PageNumber, "Workgroup,UserProjects.User");
            if (!projects.Any())
            {
                return new ApiResponse(404, "No Projects Found");
            }

            var projectDTOs = mapper.Map<List<MyProjectDTO>>(projects);

            int projects_count = await unitOfWork.projectRepository.Count(filter);

            return new ApiResponse(200, "Projects retrieved successfully", new
            {
                Total = projects_count,
                Projects = projectDTOs
            });
        }


        [Authorize(Roles = "supervisor,admin")]
        [HttpPost("students")]
        public async Task<ActionResult<ApiResponse>> AddStudent(
            int projectId,
            CreateStudentDTO students)
        {
            if (projectId <= 0)
            {
                return BadRequest(new ApiResponse(400, "Invalid Project ID."));
            }

            if (students == null || students.usersIds == null || !students.usersIds.Any())
            {
                return BadRequest(new ApiResponse(400, "Invalid student data or empty user IDs."));
            }

            var existingProject = await unitOfWork.projectRepository.GetById(projectId, "UserProjects");
            if (existingProject == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            var existingUsers = await userManager.Users
                .Where(u => students.usersIds.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync();

            var invalidUserIds = students.usersIds.Except(existingUsers).ToList();
            if (invalidUserIds.Any())
            {
                return BadRequest(new ApiValidationResponse(
                    invalidUserIds.Select(id => $"User with ID {id} not found.").ToList()
                ));
            }

            foreach (var userId in students.usersIds)
            {
                var existingUserProject = existingProject.UserProjects
                    .FirstOrDefault(u => u.UserId == userId);

                if (existingUserProject != null)
                {
                    if (existingUserProject.IsDeleted)
                    {
                        existingProject.UserProjects.Remove(existingUserProject);

                        existingUserProject.JoinAt = DateTime.UtcNow;
                        existingUserProject.IsDeleted = false;
                        existingUserProject.Role = "student";

                        existingProject.UserProjects.Add(new UserProject { UserId = userId, Role = "student" });
                    }
                    else if (existingUserProject.Role == "student")
                    {
                        return BadRequest(new ApiResponse(400, $"A student {existingUserProject.User.UserName} already exists and is active."));
                    }
                    else
                    {
                        return BadRequest(new ApiResponse(400, "User already assigned to the project with a different role."));
                    }
                }
                else
                {
                    existingProject.UserProjects.Add(new UserProject { UserId = userId, Role = "student" });
                }
            }

            existingProject.Status = "active";

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to add students."));
            }

            return Ok(new ApiResponse(200, "Students added successfully."));
        }



        [Authorize(Roles = "supervisor,admin")]
        [HttpDelete("{projectId}/students/{studentId}")]
        public async Task<ActionResult<ApiResponse>> DeleteStudent(
            int projectId,
            string studentId,
            [FromBody] NotesDTO notes)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return BadRequest(new ApiValidationResponse(new List<string> { "Student ID is required." }));
            }

            // Fetch the project and include UserProjects to check if the student exists
            var existingProject = await unitOfWork.projectRepository.GetById(projectId, "UserProjects");
            if (existingProject == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            // Find the student entry in the UserProjects collection
            var studentEntry = existingProject.UserProjects
                                .FirstOrDefault(up => up.UserId == studentId && up.Role == "student");

            if (studentEntry == null)
            {
                return NotFound(new ApiResponse(404, "Student not found in this project."));
            }

            // Remove the student entry from the UserProjects collection
            existingProject.UserProjects.Remove(studentEntry);

            studentEntry.IsDeleted = true;
            studentEntry.DeletedNotes = notes.Notes;
            studentEntry.DeletededAt = DateTime.UtcNow;

            // ADd the student entry after update to the UserProjects collection
            existingProject.UserProjects.Add(studentEntry);

            // Save the changes
            unitOfWork.projectRepository.Update(existingProject);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to remove the student from the project."));
            }

            return Ok(new ApiResponse(200, "Student removed successfully."));
        }


        [Authorize(Roles = "supervisor,admin")]
        [HttpPost("co-supervisor")]
        public async Task<ActionResult<ApiResponse>> AddCoSupervisor(
            [Required] int projectId,
            [Required] CreateCoSupervisorDTO coSupervisor)
        {

            var existingProject = await unitOfWork.projectRepository.GetById(projectId, "UserProjects");
            if (existingProject == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            var existingUserProject = existingProject.UserProjects
                .FirstOrDefault(u => u.UserId == coSupervisor.userId && u.ProjectId == projectId);

            if (existingUserProject != null)
            {
                if (existingUserProject.IsDeleted)
                {
                    existingProject.UserProjects.Remove(existingUserProject);
                    
                    existingUserProject.IsDeleted = false;
                    existingUserProject.Role = "co-supervisor";
                    existingUserProject.DeletedNotes = null;

                    existingProject.UserProjects.Add(new UserProject { UserId = coSupervisor.userId, Role = "co-supervisor" });
                }
                else if (existingUserProject.Role == "co-supervisor")
                {
                    return BadRequest(new ApiResponse(400, "A co-supervisor already exists and is active."));
                }
                else
                {
                    return BadRequest(new ApiResponse(400, "User already assigned to the project with a different role."));
                }
            }
            else
            {
                var user = await userManager.FindByIdAsync(coSupervisor.userId);
                if (user == null)
                {
                    return BadRequest(new ApiValidationResponse(new List<string> { $"User with ID {coSupervisor.userId} not found." }));
                }

                existingProject.UserProjects.Add(new UserProject { UserId = coSupervisor.userId, Role = "co-supervisor" });
            }


            // Save the changes
            unitOfWork.projectRepository.Update(existingProject);

            // Save changes to the database
            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to add Co-Supervisor."));
            }

            return Ok(new ApiResponse(200, "Co-Supervisor added successfully."));
        }

        [Authorize(Roles = "supervisor,admin")]
        [HttpDelete("{projectId}/co-supervisor")]
        public async Task<ActionResult<ApiResponse>> DeleteCoSupervisor(
            int projectId,
            [FromQuery] string co_supervisorId,
            [FromBody] NotesDTO notes)
        {

            // Fetch the project and include UserProjects to check if the student exists
            var existingProject = await unitOfWork.projectRepository.GetById(projectId, "UserProjects");
            if (existingProject == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            // Find the student entry in the UserProjects collection
            var co_supervisorEntry = existingProject.UserProjects
                                .FirstOrDefault(up => up.UserId == co_supervisorId && up.Role == "co-supervisor");

            if (co_supervisorEntry == null)
            {
                return NotFound(new ApiResponse(404, "Co-Supervisor not found in this project."));
            }

            // Remove the co_supervisorEntry entry from the UserProjects collection
            existingProject.UserProjects.Remove(co_supervisorEntry);

            co_supervisorEntry.IsDeleted = true;
            co_supervisorEntry.DeletedNotes = notes.Notes;
            co_supervisorEntry.DeletededAt = DateTime.UtcNow;

            // Add the co_supervisorEntry after update to delete
            existingProject.UserProjects.Add(co_supervisorEntry);

            // Save the changes
            unitOfWork.projectRepository.Update(existingProject);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to remove the Co-Supervisor from the project."));
            }

            return Ok(new ApiResponse(200, "Co-Supervisor removed successfully."));
        }
    }
}

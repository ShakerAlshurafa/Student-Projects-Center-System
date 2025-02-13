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
        private readonly IEmailService emailService;

        public ProjectsUserController(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            UserManager<LocalUser> userManager, 
            IEmailService emailService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
            this.emailService = emailService;
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

        [HttpPut("{projectId}/overview")]
        public async Task<ActionResult<ApiResponse>> UpdateOverview(int projectId, [FromQuery] string overview)
        {
            var project = await unitOfWork.projectRepository.GetById(projectId);
            if (project == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            project.Overview = overview;

            unitOfWork.projectRepository.Update(project);
            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Update Failed"));
            }

            return Ok(new ApiResponse(200, "Project updated successfully"));
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

            unitOfWork.projectRepository.Update(existingProject);
            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to add students."));
            }

            var existingUsersEmail = await userManager.Users
                .Where(u => students.usersIds.Contains(u.Id))
                .ToListAsync();

            // Send email notifications to added students
            foreach (var user in existingUsersEmail)
            {
                string emailContent = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; 
                                border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9;'>
                        <h2 style='color: #0275d8; text-align: center;'>You Have Been Added to a Project</h2>
                        <p style='font-size: 16px; color: #555;'>Hello <strong>{user.FirstName}</strong>,</p>
                        <p style='font-size: 16px; color: #555;'>You have been added as a <strong>Student</strong> in the project <strong>'{existingProject.Name}'</strong>.</p>
                        <p style='text-align: center; margin-top: 20px;'>
                            <a href='http://localhost:5173/workgroups/{existingProject.WorkgroupId}' 
                                style='display: inline-block; padding: 12px 20px; background-color: #007bff; 
                                color: #fff; text-decoration: none; font-size: 16px; border-radius: 5px;'>
                                View Workgroup
                            </a>
                        </p>
                        <p style='font-size: 14px; color: #777; text-align: center; margin-top: 20px;'>
                            If you have any questions, please contact support.
                        </p>
                    </div>
                        ";

                await emailService.SendEmailAsync(user.Email ?? "", "Added to Project", emailContent, true);
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
            var existingProject = await unitOfWork.projectRepository.GetById(projectId, "UserProjects.User");
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

            string emailContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; 
                            border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9;'>
                    <h2 style='color: #d9534f; text-align: center;'>You Have Been Removed from a Project</h2>
                    <p style='font-size: 16px; color: #555;'>Hello <strong>{studentEntry.User.FirstName}</strong>,</p>
        
                    <p style='font-size: 16px; color: #555;'>
                        We want to inform you that you have been removed from the project 
                        <strong>'{existingProject.Name}'</strong> by <strong>{User?.Identity?.Name ?? "an administrator"}</strong>.
                    </p>

                    <p style='font-size: 16px; color: #d9534f; font-weight: bold;'>
                        <em>Reason for Removal:</em> <span style='color: #555;'>{studentEntry.DeletedNotes ?? "No specific reason provided."}</span>
                    </p>

                    <p style='font-size: 14px; color: #777;'>
                        If you believe this removal was an error or need further clarification, please contact your supervisor or the project administrator.
                    </p>

                    <hr style='border: none; border-top: 1px solid #ddd;'>

                    <p style='font-size: 14px; color: #777; text-align: center;'>
                        If you have any concerns, please reach out to the support team.
                    </p>
                </div>";


            await emailService.SendEmailAsync(studentEntry.User.Email ?? "", "Student Removed from Project", emailContent, true);

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

            var user = await userManager.FindByIdAsync(coSupervisor.userId);
            if (user == null)
            {
                return BadRequest(new ApiValidationResponse(new List<string> { $"User with ID {coSupervisor.userId} not found." }));
            }

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

            string emailContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; 
                            border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9;'>
                    <h2 style='color: #28a745; text-align: center;'>You Have Been Assigned as a Co-Supervisor</h2>
                    <p style='font-size: 16px; color: #555;'>Hello <strong>{user.FirstName}</strong>,</p>
        
                    <p style='font-size: 16px; color: #555;'>
                        Congratulations! You have been assigned as a co-supervisor for the project 
                        <strong>'{existingProject.Name}'</strong> by <strong>{User.Identity?.Name ?? "an administrator"}</strong>.
                    </p>

                    <p style='font-size: 16px; color: #555;'>
                        You can now collaborate with the team and oversee project progress.
                    </p>

                    <p style='font-size: 16px;'>
                        <a href='http://localhost:5173/workgroups/{existingProject.WorkgroupId}' 
                            style='display: inline-block; padding: 10px 20px; background-color: #007bff; color: #fff; 
                                   text-decoration: none; border-radius: 5px;'>
                            View Workgroup
                        </a>
                    </p>

                    <p style='font-size: 14px; color: #777;'>
                        If you have any questions, please contact the project administrator.
                    </p>

                    <hr style='border: none; border-top: 1px solid #ddd;'>

                    <p style='font-size: 14px; color: #777; text-align: center;'>
                        Welcome to the team, and we look forward to your contributions!
                    </p>
                </div>";

            await emailService.SendEmailAsync(user.Email ?? "", "Added to Project", emailContent, true);

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
            var existingProject = await unitOfWork.projectRepository.GetById(projectId, "UserProjects.User");
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

            string emailContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; 
                            border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9;'>
                    <h2 style='color: #d9534f; text-align: center;'>You Have Been Removed as a Co-Supervisor</h2>
                    <p style='font-size: 16px; color: #555;'>Hello <strong>{co_supervisorEntry.User.FirstName}</strong>,</p>
        
                    <p style='font-size: 16px; color: #555;'>
                        We want to inform you that you have been removed as a co-supervisor from the project 
                        <strong>'{existingProject.Name}'</strong> by <strong>{User?.Identity?.Name ?? "an administrator"}</strong>.
                    </p>

                    <p style='font-size: 16px; color: #d9534f; font-weight: bold;'>
                        <em>Reason for Removal:</em> <span style='color: #555;'>{co_supervisorEntry.DeletedNotes ?? "No specific reason provided."}</span>
                    </p>

                    <p style='font-size: 14px; color: #777;'>
                        If you believe this removal was an error or need further clarification, please contact the project administrator.
                    </p>

                    <hr style='border: none; border-top: 1px solid #ddd;'>

                    <p style='font-size: 14px; color: #777; text-align: center;'>
                        If you have any concerns, please reach out to the support team.
                    </p>
                </div>";


            await emailService.SendEmailAsync(co_supervisorEntry.User.Email ?? "", "Co-Supervisor Removed from Project", emailContent, true);


            return Ok(new ApiResponse(200, "Co-Supervisor removed successfully."));
        }
    }
}

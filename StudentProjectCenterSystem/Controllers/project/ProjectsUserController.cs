using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Security.Claims;
using StudentProjectsCenter.Core.Entities.DTO.Project;

namespace StudentProjectsCenterSystem.Controllers
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


        [HttpGet("get-all-for-user")]
        [Authorize]
        //[ResponseCache(CacheProfileName = ("defaultCache"))]
        public async Task<ActionResult<ApiResponse>> GetAllForUser([FromQuery] int PageSize = 6, [FromQuery] int PageNumber = 1)
        {
            // Retrieve the logged-in user's ID from the claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse(401, "User not Find."));
            }

            // Filter workgroups where the logged-in user is associated
            Expression<Func<StudentProjectsCenterSystem.Core.Entities.project.Project, bool>> filter = x =>
                x.UserProjects.Any(up => up.UserId == userId);

            var projects = await unitOfWork.projectRepository.GetAll(filter, PageSize, PageNumber, "Workgroup,UserProjects.User");
            if (!projects.Any())
            {
                return new ApiResponse(404, "No Projects Found");
            }

            var projectDTOs = mapper.Map<List<MyProjectDTO>>(projects);

            return new ApiResponse(200, "Projects retrieved successfully", projectDTOs);
        }



        [HttpPost("students")]
        public async Task<ActionResult<ApiResponse>> AddStudent([Required] int projectId, [Required] CreateStudentDTO students)
        {
            var existingProject = await unitOfWork.projectRepository.GetById(projectId, "UserProjects");
            if (existingProject == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            // Validate that all students exist in the system
            foreach (var userId in students.usersId)
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return BadRequest(new ApiValidationResponse(new List<string> { $"User with ID {userId} not found." }));
                }
            }

            foreach (var userId in students.usersId)
            {
                var existingUserProject = existingProject.UserProjects
                .FirstOrDefault(u => u.UserId == userId);


                if (existingUserProject != null)
                {
                    if (existingUserProject.IsDeleted)
                    {
                        // Change IsDeleted to false
                        existingUserProject.JoinAt = DateTime.UtcNow;
                        existingUserProject.IsDeleted = false;
                        existingUserProject.Role = "student";
                        existingUserProject.DeletedNotes = null;
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
                    existingProject.UserProjects.Add(new UserProject { UserId = userId, Role = "Student" });
                }
            }

            existingProject.Status = "active";

            // Save the changes
            unitOfWork.projectRepository.Update(existingProject);

            // Save changes to the database
            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to add students."));
            }

            return Ok(new ApiResponse(200, "Students added successfully."));
        }


        [Authorize(Roles = "supervisor,admin")]
        [HttpDelete("{projectId}/students")]
        public async Task<ActionResult<ApiResponse>> DeleteStudent(
            int projectId,
            [FromQuery, Required] string studentId,
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

            studentEntry.IsDeleted = true;
            studentEntry.DeletedNotes = notes.Notes;
            studentEntry.DeletededAt = DateTime.UtcNow;

            //// Remove the student entry from the UserProjects collection
            //existingProject.UserProjects.Remove(studentEntry);

            // Save the changes
            unitOfWork.projectRepository.Update(existingProject);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to remove the student from the project."));
            }

            return Ok(new ApiResponse(200, "Student removed successfully."));
        }

    }
}

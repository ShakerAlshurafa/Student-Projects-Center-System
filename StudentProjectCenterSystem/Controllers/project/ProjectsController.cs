using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenter.Core.Entities.DTO.Project;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace StudentProjectsCenter.Controllers.project
{
    [Route("api/projects")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<LocalUser> userManager;

        public ProjectsController(IUnitOfWork unitOfWork, UserManager<LocalUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
        }

        [Authorize]
        [HttpGet("details/{id}")]
        public async Task<ActionResult<ApiResponse>> GetById(int id)
        {
            var model = await unitOfWork.projectRepository.GetByIdWithDetails(id);
            if (model == null)
            {
                return NotFound(new ApiResponse(404, "No Projects Found!"));
            }

            return Ok(new ApiResponse(200, result: model));
        }

        [Authorize]
        [HttpGet("project-statuses")]
        public ActionResult<ApiResponse> GetAllProjectStatuses()
        {
            return Ok(new ApiResponse(200, result: new
            {
                active = "The project is currently ongoing, with tasks and activities actively being worked on.",
                completed = "The project has been successfully finished, with all objectives and deliverables met.",
                pending = "The project is in a state of preparation, awaiting the necessary decisions or requirements to begin or reach completion.",
                canceled = "The project has been terminated before completion due to specific circumstances or changes in requirements."
            }));
        }

        [HttpGet("favorites")]
        public async Task<ActionResult<ApiResponse>> GetFavoriteProjects()
        {
            Expression<Func<Project, bool>> filter = x => x.Favorite;
            var favoriteProjects = await unitOfWork.projectRepository.GetAll(filter);

            if(favoriteProjects == null)
            {
                return NotFound(new ApiResponse(404, "You don't have any favorite projects yet."));
            }

            return Ok(new ApiResponse(200, result: favoriteProjects));
        }

        [Authorize(Roles ="supervisor")]
        [HttpPut("{projectId}/status")]
        public async Task<ActionResult<ApiResponse>> UpdateProjectStatus(
            int projectId,
            [FromBody, Required] ChangeProjectStatusDTO changeProjectStatusDTO)
        {
            // Check if project exists
            var project = await unitOfWork.projectRepository.GetById(projectId);
            if (project == null)
            {
                return NotFound(new ApiResponse(404, "Project not found."));
            }

            // Validate the new status
            var validStatuses = new List<string> { "active", "completed", "pending", "canceled" };
            if (string.IsNullOrWhiteSpace(changeProjectStatusDTO.status) || !validStatuses.Contains(changeProjectStatusDTO.status.ToLower()))
            {
                return BadRequest(new ApiResponse(400, $"Invalid status. Allowed values: {string.Join(", ", validStatuses)}"));
            }

            // Update the project status
            project.Status = changeProjectStatusDTO.status;
            project.ChangeStatusNotes = changeProjectStatusDTO.notes;
            project.ChangeStatusAt = DateTime.UtcNow;

            if (project.Status == "completed")
            {
                project.EndDate = DateTime.UtcNow;
            }

            // Save changes to database
            unitOfWork.projectRepository.Update(project);
            var result = await unitOfWork.save();
            if (result <= 0)
            {
                return StatusCode(500, new ApiResponse(500, "Failed to update project status."));
            }

            return Ok(new ApiResponse(200, $"Project status updated to '{changeProjectStatusDTO.status}'."));
        }

    }

}
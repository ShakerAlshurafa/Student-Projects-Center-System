using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Core.IRepositories;

namespace StudentProjectsCenterSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IUnitOfWork<Project> unitOfWork;
        private readonly IMapper mapper;

        public ProjectController(IUnitOfWork<Project> unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetAll([FromQuery] int PageSize = 6, [FromQuery] int PageNumber = 1)
        {
            var model = await unitOfWork.projectRepository.GetAll(PageSize, PageNumber);

            if (!model.Any())
            {
                return new ApiResponse(404, "No Projects Found");
            }

            var viewModel = mapper.Map<List<ProjectDTO>>(model);
            return new ApiResponse(200, "Projects retrieved successfully", viewModel);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse>> GetById(int id)
        {
            var model = await unitOfWork.projectRepository.GetByIdWithDetails(id);
            if (model == null)
            {
                return NotFound(new ApiResponse(404, "No Projects Found!"));
            }

            var viewModel = mapper.Map<ProjectDetailsDTO>(model);
            return Ok(new ApiResponse(200, result: viewModel));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> Create(ProjectCreateDTO project)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new ApiValidationResponse(errors));
            }

            var model = mapper.Map<Project>(project);
            await unitOfWork.projectRepository.Create(model);
            int success = await unitOfWork.save();

            if (success == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Create Failed"));
            }

            return CreatedAtAction(nameof(Create), new { id = success }, new ApiResponse(201, result: project));
        }

        [HttpPut]
        public async Task<ActionResult<ApiResponse>> Update(ProjectUpdateDTO project)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new ApiValidationResponse(errors));
            }

            var existingProject = await unitOfWork.projectRepository.GetById(project.Id);
            if (existingProject == null)
            {
                return NotFound(new ApiResponse(404, "Project not found"));
            }

            mapper.Map(project, existingProject);
            unitOfWork.projectRepository.Update(existingProject);
            int success = await unitOfWork.save();

            if (success == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Update Failed"));
            }

            return Ok(new ApiResponse(200, result: existingProject));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            int successDelete = unitOfWork.projectRepository.Delete(id);
            if(successDelete == 0)
            {
                return NotFound(new ApiResponse(404));
            }

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Deleted failed!"));
            }

            return Ok(new ApiResponse(200, "Deleted Successfully"));
        }

    }
}
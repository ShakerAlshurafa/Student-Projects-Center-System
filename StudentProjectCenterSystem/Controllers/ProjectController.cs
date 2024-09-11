using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Data;

namespace StudentProjectsCenterSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IUnitOfWork<Project> unitOfWork;
        private readonly IMapper mapper;
        private ApiResponse apiResponse;

        public ProjectController(IUnitOfWork<Project> unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            apiResponse = new ApiResponse();
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> getAll()
        {
            var model = await unitOfWork.projectRepository.GetAll();
            apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
            if (!model.Any())
            {
                apiResponse.IsSuccess = false;
                apiResponse.ErrorMessages = "No Projects Found!";
            }
            else
            {
                apiResponse.IsSuccess = true;
                var viewModel = mapper.Map<List<ProjectDTO>>(model);
                apiResponse.Result = viewModel;
            }
            return apiResponse;
        }

        [HttpGet("id")]
        public async Task<ActionResult<ApiResponse>> getById(int id)
        {
            var model = await unitOfWork.projectRepository.GetByIdWithDetails(id);
            if(model == null)
            {
                apiResponse.IsSuccess = false;
                apiResponse.StatusCode= System.Net.HttpStatusCode.NotFound;
                apiResponse.ErrorMessages = "No Projects Found!";
            }
            else
            {
                apiResponse.IsSuccess = true;
                apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
                var viewModel = mapper.Map<ProjectDetailsDTO>(model);
                apiResponse.Result = viewModel;
            }
            return apiResponse;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> Create(ProjectCreateDTO project)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                var errorResponse = new ApiResponse
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = string.Join(", ", errors)
                };
                return BadRequest(errorResponse);
            }

            var model = mapper.Map<Project>(project);

            await unitOfWork.projectRepository.Create(model);
            int success = await unitOfWork.save();

            if (success == 0)
            {
                var failureResponse = new ApiResponse
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    ErrorMessages = "Create Failed"
                };
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError, failureResponse);
            }

            var successResponse = new ApiResponse
            {
                StatusCode = System.Net.HttpStatusCode.Created,
                IsSuccess = true,
                Result = project
            };
            return CreatedAtAction(nameof(Create), new { id = success }, successResponse);
        }


        [HttpPut]
        public async Task<ActionResult<ApiResponse>> Update(ProjectUpdateDTO project)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                var errorResponse = new ApiResponse
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    ErrorMessages = string.Join(", ", errors)
                };
                return BadRequest(errorResponse);
            }

            var existingProject = await unitOfWork.projectRepository.GetById(project.Id);
            if (existingProject == null)
            {
                var notFoundResponse = new ApiResponse
                {
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    IsSuccess = false,
                    ErrorMessages = "Project not found"
                };
                return NotFound(notFoundResponse);
            }

            mapper.Map(project, existingProject);

            unitOfWork.projectRepository.Update(existingProject);
            int success = await unitOfWork.save();

            if (success == 0)
            {
                var failureResponse = new ApiResponse
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    ErrorMessages = "Update Failed"
                };
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError, failureResponse);
            }

            var successResponse = new ApiResponse
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccess = true,
                Result = existingProject
            };
            return Ok(successResponse);
        }



        [HttpDelete]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            unitOfWork.projectRepository.Delete(id);
            int success = await unitOfWork.save();
            if (success == 0)
            {
                var failureResponse = new ApiResponse
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    ErrorMessages = "Deletion failed!"
                };
                return StatusCode((int)System.Net.HttpStatusCode.InternalServerError, failureResponse);
            }

            var successResponse = new ApiResponse
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                IsSuccess = true,
            };
            return successResponse;
        }
    }
}
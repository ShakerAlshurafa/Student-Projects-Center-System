using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenterSystem.Core.Entities;
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
        private ApiResponse apiResponse;

        public ProjectController(IUnitOfWork<Project> unitOfWork)
        {
            this.unitOfWork = unitOfWork;
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
                apiResponse.ErrorMessages = "No Products Found!";
            }
            else
            {
                apiResponse.IsSuccess = true;
                apiResponse.Result = model;
            }
            return apiResponse;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> Create(Project project)
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

            await unitOfWork.projectRepository.Create(project);
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

    }
}
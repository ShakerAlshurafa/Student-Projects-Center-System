using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.DTO.MyProject;
using StudentProjectsCenterSystem.Core.Entities.DTO.ProjectDetails;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;

namespace StudentProjectsCenterSystem.Controllers
{
    [Route("api/project-details")]
    [ApiController]
    public class ProjectDetailsController : ControllerBase
    {
        private readonly IUnitOfWork<ProjectDetailEntity> unitOfWork;
        private readonly IMapper mapper;

        public ProjectDetailsController(IUnitOfWork<ProjectDetailEntity> unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }


        [HttpPost]
        public async Task<ActionResult<ApiResponse>> Create([FromBody] ProjectDetailsCreateDTO details)
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new ApiValidationResponse(errors));
            }

            // Check if the sction exists
            if (!await unitOfWork.detailsSectionsRepository.IsExist(details.SectionId))
            {
                return NotFound(new ApiResponse(404, "Section not found."));
            }

            // Check if Title is null or empty
            if (string.IsNullOrEmpty(details.Title))
            {
                return BadRequest(new ApiResponse(400, "Title is required."));
            }

            // Check if Description is null or empty
            if (string.IsNullOrEmpty(details.Description))
            {
                return BadRequest(new ApiResponse(400, "Description is required."));
            }

            // Create new ProjectDetails entity
            var model = new ProjectDetailEntity
            {
                Title = details.Title,
                Description = details.Description,
                IconData = details.IconData ?? Array.Empty<byte>(),
                //ProjectDetailsSectionId = details.SectionId,
                ProjectDetailsSection = await unitOfWork.detailsSectionsRepository.GetById(details.SectionId)
            };

            // Add and save the new entity
            await unitOfWork.projectDetailsRepository.Create(model);
            if (await unitOfWork.save() == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Create operation failed."));
            }

            // Return success response with 201 Created status
            return CreatedAtAction(nameof(Create), new { id = model.Id }, new ApiResponse(201, result: model));
        }



        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] ProjectDetailsEditDTO section)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new ApiValidationResponse(errors));
            }

            var details = await unitOfWork.projectDetailsRepository.GetById(id);
            if (details == null)
            {
                return NotFound(new ApiResponse(404, "Section not found."));
            }

            // Update Section fields
            details.Title = section.Title;
            details.Description = section.Description;
            details.IconData = section.IconData;

            var model = mapper.Map<ProjectDetailEntity>(details);

            // Save the changes
            unitOfWork.projectDetailsRepository.Update(model);
            int successSave = await unitOfWork.save();

            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Update Failed"));
            }

            return Ok(new ApiResponse(200, "Information updated successfully"));
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            int successDelete = unitOfWork.projectDetailsRepository.Delete(id);
            if (successDelete == 0)
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
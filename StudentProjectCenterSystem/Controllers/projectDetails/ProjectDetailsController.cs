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

namespace StudentProjectsCenter.Controllers.ProjectDetails
{
    [Route("api/project-details")]
    [ApiController]
    public class ProjectDetailsController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public ProjectDetailsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }


        [HttpPost("{sectionId}")]
        public async Task<ActionResult<ApiResponse>> Create(
            int sectionId, 
            [FromBody, Required] ProjectDetailsCreateDTO[] projectDetailsDto)
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new ApiValidationResponse(errors));
            }
            
            if (projectDetailsDto == null || !projectDetailsDto.Any())
            {
                return BadRequest(new ApiResponse(400, "No details provided"));
            }

            var section = await unitOfWork.detailsSectionsRepository.GetById(sectionId);
            if(section == null)
            {
                return NotFound(new ApiResponse(404, "Section not found"));
            }
            
            var models = projectDetailsDto.Select(detail => new ProjectDetailEntity
            {
                Title = detail.Title,
                Description = detail.Description,
                IconData = detail.IconData ?? Array.Empty<byte>(),
                ProjectDetailsSection = section
            }).ToList();

            await unitOfWork.projectDetailsRepository.CreateRange(models);

            // Save all changes
            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Create operation failed."));
            }

            // Return success response with 201 Created status
            return CreatedAtAction(nameof(Create), new ApiResponse(201, "Details created successfully"));
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

            unitOfWork.projectDetailsRepository.Update(details);

            // Save the changes
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
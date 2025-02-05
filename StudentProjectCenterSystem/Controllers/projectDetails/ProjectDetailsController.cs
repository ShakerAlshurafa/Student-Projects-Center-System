using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.DTO.MyProject;
using StudentProjectsCenterSystem.Core.Entities.DTO.ProjectDetails;
using StudentProjectsCenterSystem.Core.Entities.DTO.Workgroup;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Core.IRepositories;

namespace StudentProjectsCenter.Controllers.ProjectDetails
{
    [Authorize]
    [Route("api/project-details")]
    [ApiController]
    public class ProjectDetailsController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly AzureFileUploader _uploadHandler;

        public ProjectDetailsController(IUnitOfWork unitOfWork, IMapper mapper, AzureFileUploader uploadHandler)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            _uploadHandler = uploadHandler;
        }

        
        [HttpPost("{sectionId}")]
        public async Task<ActionResult<ApiResponse>> Create(
            int sectionId,
            [FromForm] ProjectDetailsCreateDTO projectDetailsDto)
        {
            // Validate the model state
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new ApiValidationResponse(errors));
            }

            if (projectDetailsDto == null)
            {
                return BadRequest(new ApiResponse(400, "No details provided"));
            }

            var section = await unitOfWork.detailsSectionsRepository.GetById(sectionId);
            if (section == null)
            {
                return NotFound(new ApiResponse(404, "Section not found"));
            }

            string imagePath = "";
            // Upload the image if present
            if (projectDetailsDto.image != null && projectDetailsDto.image.Length > 0)
            {
                var image = await _uploadHandler.UploadAsync(projectDetailsDto.image, "uploads");
                imagePath = image?.Path ?? "";
            }

            // Process the list and upload images asynchronously
            var model =  new ProjectDetailEntity
            {
                Title = projectDetailsDto.Title,
                Description = projectDetailsDto.Description,
                ImagePath = imagePath, // Save the uploaded image URL
                ProjectDetailsSection = section 
            };

            await unitOfWork.projectDetailsRepository.Create(model);

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
        public async Task<ActionResult<ApiResponse>> Update(
            int id,
            [FromForm] ProjectDetailsEditDTO section)
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
            details.Title = section.Title ?? details.Title;
            details.Description = section.Description ?? details.Description;

            if (section.image is { Length: > 0 })  
            {
                var uploadedImage = await _uploadHandler.UploadAsync(section.image, "uploads").ConfigureAwait(false);
                details.ImagePath = uploadedImage?.Path ?? details.ImagePath;
            }


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
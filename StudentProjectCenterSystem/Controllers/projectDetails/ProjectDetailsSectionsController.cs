using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.DTO.ProjectDetailsSection;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace StudentProjectsCenter.Controllers.ProjectDetails
{
    [Route("api/project-sections")]
    [ApiController]
    public class ProjectDetailsSectionsController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public ProjectDetailsSectionsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }


        [HttpGet]
        [ResponseCache(CacheProfileName = "defaultCache")]
        public async Task<ActionResult<ApiResponse>> GetAllSection(
            [FromQuery, Required] int projectId)
        {
            Expression<Func<ProjectDetailsSection, bool>> filter = s => s.ProjectId == projectId;
            var model = await unitOfWork.detailsSectionsRepository.GetAll(filter, "Project");

            if (model == null)
            {
                return NotFound(new ApiResponse(404, "No Section Found"));
            }

            var sectionDtos = mapper.Map<List<ProjectDetailsSectionDTO>>(model);
            if(sectionDtos == null)
            {
                return Ok(new ApiResponse(200, "No Section found"));
            }

            return Ok(new ApiResponse(200, "Sections retrieved successfully", sectionDtos));
        }


        [HttpPost]
        public async Task<ActionResult<ApiResponse>> Create(
            [FromQuery, Required] int projectId, 
            [FromBody, Required] ProjectDetailsSectionCreateDTO section)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new ApiValidationResponse(errors));
            }

            // add check if section exist
            var model = mapper.Map<ProjectDetailsSection>(section);
            model.ProjectId = projectId;
            await unitOfWork.detailsSectionsRepository.Create(model);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Create Failed"));
            }

            return CreatedAtAction(nameof(Create), new { id = model.Id }, new ApiResponse(201, result: model.Name));
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> Update(int id, 
            [FromBody, Required] ProjectDetailsSectionCreateDTO section)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new ApiValidationResponse(errors));
            }

            var existingSection = await unitOfWork.detailsSectionsRepository.GetById(id);
            if (existingSection == null)
            {
                return NotFound(new ApiResponse(404, "Section not found."));
            }

            // Update Section fields
            existingSection.Name = section.Name;

            unitOfWork.detailsSectionsRepository.Update(existingSection);
            
            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Update Failed"));
            }

            return Ok(new ApiResponse(200, "Section Name updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            int successDelete = unitOfWork.detailsSectionsRepository.Delete(id);
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

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.DTO.ProjectDetailsSection;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace StudentProjectsCenterSystem.Controllers
{
    [Route("api/project-sections")]
    [ApiController]
    public class ProjectDetailsSectionsController : ControllerBase
    {
        private readonly IUnitOfWork<ProjectDetailsSection> unitOfWork;
        private readonly IMapper mapper;

        public ProjectDetailsSectionsController(IUnitOfWork<ProjectDetailsSection> unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }


        [HttpGet]
        [ResponseCache(CacheProfileName = ("defaultCache"))]
        public async Task<ActionResult<ApiResponse>> GetAllSection(int projectId)
        {
            //var checkProjectId = await unitOfWork.projectRepository.IsExistAsync(projectId);

            var model = await unitOfWork.detailsSectionsRepository.GetAllByProjecId(projectId);

            if (!model.Any())
            {
                return new ApiResponse(404, "No Section Found");
            }

            var sectionDtos = mapper.Map<List<ProjectDetailsSectionDTO>>(model);

            return new ApiResponse(200, "Sections retrieved successfully", sectionDtos);
        }


        [HttpPost]
        public async Task<ActionResult<ApiResponse>> Create([Required][FromQuery] int projectId, [FromBody] ProjectDetailsSectionCreateDTO section)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new ApiValidationResponse(errors));
            }

            var model = mapper.Map<ProjectDetailsSection>(section);
            model.ProjectId = projectId;
            await unitOfWork.detailsSectionsRepository.Create(model);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Create Failed"));
            }

            return CreatedAtAction(nameof(Create), new { id = model.Id }, new ApiResponse(201, result: model));
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody] ProjectDetailsSectionCreateDTO section)
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

            var model = mapper.Map<ProjectDetailsSection>(existingSection);

            // Save the changes
            unitOfWork.detailsSectionsRepository.Update(model);
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

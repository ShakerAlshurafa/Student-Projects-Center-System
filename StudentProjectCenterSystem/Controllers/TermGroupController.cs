using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenter.Core.Entities.Domain.Terms;
using StudentProjectsCenter.Core.Entities.DTO.Terms;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace StudentProjectsCenter.Controllers
{
    [Route("api/terms&conditions")]
    [ApiController]
    public class TermGroupController : ControllerBase
    {
        private readonly IUnitOfWork<TermGroup> unitOfWork;
        private readonly IMapper mapper;

        public TermGroupController(IUnitOfWork<TermGroup> unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetAll([FromQuery] int PageSize = 6, [FromQuery] int PageNumber = 1)
        {
            Expression<Func<TermGroup, bool>> filter = x => true;
            var termGroups = await unitOfWork.termGroupRepository.GetAll(filter, PageSize, PageNumber, "Terms");

            if (!termGroups.Any())
            {
                return new ApiResponse(200, "No Term Groups Found");
            }

            var termGroupDto = mapper.Map<List<TermGroupDTO>>(termGroups);
            return Ok(new ApiResponse(200, "Terms retrieved successfully", termGroupDto));
        }


        [HttpGet("active/{id}")]
        public async Task<ActionResult<ApiResponse>> GetActiveTerms([Required] int id)
        {
            var termGroup = await unitOfWork.termGroupRepository.GetById(id, "Terms");
            if(termGroup == null)
            {
                return new ApiResponse(200, "No Term Group Found");
            }

            // Filter out terms where IsDeleted is true
            var activeTerms = termGroup.Terms?.Where(t => !t.IsDeleted).ToList();
            if(activeTerms == null || !activeTerms.Any())
            {
                return new ApiResponse(200, "No active terms found");
            }

            var result = new
            {
                TermGroupId = termGroup.Id,
                Title = termGroup.Title,
                ActiveTerms = activeTerms.Select(x => x.Description).ToList()
            };

            return Ok(new ApiResponse(200, "Terms retrieved successfully", result));
        }

        [HttpGet("archived/{id}")]
        public async Task<ActionResult<ApiResponse>> GetArchivedTerms([Required] int id)
        {
            var termGroup = await unitOfWork.termGroupRepository.GetById(id, "Terms");
            if (termGroup == null)
            {
                return new ApiResponse(200, "No Term Group Found");
            }

            // Filter out terms where IsDeleted is true (archived/deleted terms)
            var archivedTerms = termGroup.Terms?.Where(t => t.IsDeleted).ToList();
            if (archivedTerms == null || !archivedTerms.Any())
            {
                return new ApiResponse(200, "No archived terms found");
            }

            var result = new
            {
                ArchivedTerms = archivedTerms.Select(x => x.Description)
            };

            return Ok(new ApiResponse(200, "Archived terms retrieved successfully", result));
        }

        [HttpGet("inclusive/{id}")]
        public async Task<ActionResult<ApiResponse>> GetTermsIncludingDeleted([Required] int id)
        {
            var termGroup = await unitOfWork.termGroupRepository.GetById(id, "Terms");
            if (termGroup == null)
            {
                return new ApiResponse(200, "No Term Group Found");
            }

            var result = new
            {
                Id = termGroup.Id,
                Title = termGroup.Title,
                ActiveTerms = termGroup?.Terms?.Select(x => x.Description).ToList()
            };


            return Ok(new ApiResponse(200, "All terms retrieved successfully", result));
        }


        [HttpPost]
        public async Task<ActionResult<ApiResponse>> Create([FromBody] TermGroupCreateDTO termGroupDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new ApiValidationResponse(errors));
            }

            if (termGroupDto == null)
            {
                return BadRequest(new ApiResponse(400, "The provided TermGroup data is null or invalid."));
            }

            var termGroup = mapper.Map<TermGroup>(termGroupDto);

            await unitOfWork.termGroupRepository.Create(termGroup);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Create Failed"));
            }

            return CreatedAtAction(nameof(Create), new { id = termGroup.Id }, new ApiResponse(201, result: termGroup));
        }

        [HttpPut]
        public async Task<ActionResult<ApiResponse>> Update([Required, FromQuery] int id, [FromBody] TermGroupCreateDTO termGroupDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new ApiValidationResponse(errors));
            }

            var term = await unitOfWork.termGroupRepository.GetById(id);
            if (term == null)
            {
                return BadRequest(new ApiValidationResponse(new List<string> { "TermGroup not found." }));
            }

            term.Title = termGroupDto.Title;

            unitOfWork.termGroupRepository.Update(term);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Update Failed"));
            }

            return Ok(new ApiResponse(200, "TermGroup updated successfully"));

        }


        [HttpDelete]
        public async Task<ActionResult<ApiResponse>> Delete([Required, FromQuery] int id)
        {
            var successDelete = unitOfWork.termGroupRepository.Delete(id);
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

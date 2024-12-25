using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using StudentProjectsCenter.Core.Entities.Domain.Terms;
using StudentProjectsCenter.Core.Entities.DTO.Terms;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.DTO;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;

namespace StudentProjectsCenter.Controllers
{
    [Route("api/terms")]
    [ApiController]
    public class TermsController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public TermsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> Get()
        {
            var term = await unitOfWork.termRepository.GetAll(x => true);
            if(term == null || !term.Any())
            {
                return new ApiResponse(200, "No date found.");
            }

            return Ok(new ApiResponse(200, result:term));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> Create([FromBody, Required] TermDTO termDTO)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new ApiValidationResponse(errors));
            }

            var isEmpty = await unitOfWork.termRepository.IsEmpty();
            if(!isEmpty)
            {
                return BadRequest(new ApiResponse(400, "A term already exists."));
            }

            if (termDTO == null)
            {
                return BadRequest(new ApiResponse(400, "The provided data is null or invalid."));
            }

            var term = new Term()
            {
                Description = termDTO.Description,
                Title = termDTO.Title
            };

            await unitOfWork.termRepository.Create(term);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Create Failed"));
            }

            return CreatedAtAction(nameof(Create), new { id = term.Id }, new ApiResponse(201, result: term));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> Update(int id, [FromBody, Required] TermUpdateDTO termDTO)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new ApiValidationResponse(errors));
            }

            if (termDTO == null)
            {
                return BadRequest(new ApiResponse(400, "The provided data is null or invalid."));
            }

            var term = await unitOfWork.termRepository.GetById(id);
            if (term == null)
            {
                return BadRequest(new ApiValidationResponse(new List<string> { "Term not found." }));
            }

            if (!termDTO.Title.IsNullOrEmpty())
            {
                term.Title = termDTO.Title;
            }
            if (!termDTO.Description.IsNullOrEmpty())
            {
                term.Description = termDTO.Description;
            }
            term.LastUpdatedAt = DateTime.UtcNow;

            unitOfWork.termRepository.Update(term);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Create Failed"));
            }

            return Ok(new ApiResponse(200, "Term updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            int successDelete = unitOfWork.termRepository.Delete(id);
            if (successDelete == 0)
            {
                return BadRequest(new ApiValidationResponse(new List<string> { "Delete Failed" }));
            }

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Delete Failed"));
            }

            return Ok(new ApiResponse(200, "Deleted successfully"));
        }
    }
}

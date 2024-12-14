using AutoMapper;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IUnitOfWork<Term> unitOfWork;
        private readonly IMapper mapper;

        public TermsController(IUnitOfWork<Term> unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> Create(
            [Required, FromQuery] int termGroupId,
            [FromBody] TermDTO termDTO)
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


            foreach (var model in termDTO.Description)
            {
                var term = new Term()
                {
                    Description = model,
                    TermGroupId = termGroupId
                };

                await unitOfWork.termRepository.Create(term);
            }

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Create Failed"));
            }

            return CreatedAtAction(nameof(Create), new { id = termGroupId }, new ApiResponse(201, result: termDTO));
        }

        [HttpPut]
        public async Task<ActionResult<ApiResponse>> Update([Required] int id, [FromBody] TermUpdateDTO termDTO)
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


            term.Description = termDTO.Description;

            unitOfWork.termRepository.Update(term);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Create Failed"));
            }

            return Ok(new ApiResponse(200, "Term updated successfully"));
        }

        [HttpDelete]
        public async Task<ActionResult<ApiResponse>> Delete([Required] int id)
        {
            var term = await unitOfWork.termRepository.GetById(id);
            if (term == null)
            {
                return BadRequest(new ApiValidationResponse(new List<string> { "Term not found." }));
            }

            term.IsDeleted = true;

            unitOfWork.termRepository.Update(term);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Delete Failed"));
            }

            return Ok(new ApiResponse(200, "Term deleted successfully"));
        }
    }
}

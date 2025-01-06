using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentProjectsCenter.Core.Entities.Domain.workgroup;
using StudentProjectsCenter.Core.Entities.DTO.Workgroup;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.project;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Security.Claims;

namespace StudentProjectsCenter.Controllers
{
    //[Authorize]
    [Route("api/celender")]
    [ApiController]
    public class CelenderController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly UserManager<LocalUser> userManager;

        public CelenderController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<LocalUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> Get()
        {
            Expression<Func<Celender, bool>> filter = c => c.EndAt >= DateTime.UtcNow;
            var events = await unitOfWork.celenderRepository.GetAll(filter);
            var eventsDto = new List<CelenderEventDTO>();

            foreach (var ele in events)
            {
                var author = await userManager.FindByIdAsync(ele.AuthorId);
                eventsDto.Add(new CelenderEventDTO()
                {
                    Id = ele.Id,
                    Title = ele.Title,
                    Description = ele.Description,
                    Author = author?.UserName ?? "",
                    AllDay = ele.AllDay,
                    StartAt = ele.StartAt,
                    EndAt = ele.EndAt,
                });
            }

            return Ok(new ApiResponse(200, result: eventsDto));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> Create([Required] CreateCelenderEventDTO eventDTO)
        {
            var CelenderEvent = mapper.Map<Celender>(eventDTO);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            CelenderEvent.AuthorId = userId ?? "";

            await unitOfWork.celenderRepository.Create(CelenderEvent);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Create Failed"));
            }

            return CreatedAtAction(nameof(Create), new { id = CelenderEvent.Id }, new ApiResponse(201, result: CelenderEvent));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> Update(int id, [Required] CreateCelenderEventDTO eventDTO)
        {
            var CelenderEvent = mapper.Map<Celender>(eventDTO);

            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (CelenderEvent.AuthorId != userId)
                return BadRequest(new ApiResponse(400, "You are not the author"));

            var existingEvent = await unitOfWork.celenderRepository.GetById(id);

            if(existingEvent == null)
            {
                return NotFound(new ApiResponse());
            }

            existingEvent.Title = eventDTO.Title;
            existingEvent.Description = eventDTO.Description;
            existingEvent.AllDay = eventDTO.AllDay;
            existingEvent.StartAt = eventDTO.StartAt;
            existingEvent.EndAt = eventDTO.EndAt;

            unitOfWork.celenderRepository.Update(existingEvent);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Update Failed"));
            }

            return new ApiResponse(200, result: existingEvent);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            int successDelete = unitOfWork.celenderRepository.Delete(id);
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

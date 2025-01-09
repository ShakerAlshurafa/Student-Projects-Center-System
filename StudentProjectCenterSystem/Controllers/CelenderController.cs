using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenter.Core.Entities.Domain.workgroup;
using StudentProjectsCenter.Core.Entities.DTO;
using StudentProjectsCenter.Core.Entities.DTO.Workgroup;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;

namespace StudentProjectsCenter.Controllers
{
    [Authorize]
    [Route("api/celender")]
    [ApiController]
    public class CelenderController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly UserManager<LocalUser> userManager;

        public CelenderController(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            UserManager<LocalUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        [HttpGet("{workgroupId}")]
        public async Task<ActionResult<ApiResponse>> Get(int workgroupId)
        {
            Expression<Func<Celender, bool>> filter = c =>
                (c.EndAt >= DateTime.UtcNow || c.AllDay)
                && c.WorkgroupId == workgroupId;

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
                    EndAt = ele.EndAt
                });
            }

            return Ok(new ApiResponse(200, result: eventsDto));
        }

        [HttpPost("{workgroupId}")]
        public async Task<ActionResult<ApiResponse>> Create(
            int workgroupId,
            [Required] CreateCelenderEventDTO eventDTO)
        {
            if (eventDTO.EndAt < eventDTO.StartAt)
            {
                return BadRequest(new ApiResponse(400, "End date must be later than start date"));
            }

            var workgroup = await unitOfWork.workgroupRepository.GetById(workgroupId, "Project.UserProjects");
            if (workgroup == null)
            {
                return NotFound(new ApiResponse(404, "Workgropu not found"));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return BadRequest(new ApiResponse(400, "User ID is required"));
            }

            var existInWorkgroup = workgroup.Project?.UserProjects?.Any(u => u.UserId == userId) ?? false;
            if (!existInWorkgroup)
            {
                return BadRequest(new ApiResponse(400, "You are not a member of this workgroup"));
            }

            var CelenderEvent = mapper.Map<Celender>(eventDTO);

            CelenderEvent.AuthorId = userId;
            CelenderEvent.Workgroup = workgroup;

            await unitOfWork.celenderRepository.Create(CelenderEvent);

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Create Failed"));
            }

            var celenderEventsDto = mapper.Map<CelenderEventDTO>(CelenderEvent);
            celenderEventsDto.Author = User?.Identity?.Name ?? "";

            return CreatedAtAction(nameof(Create), new { id = CelenderEvent.Id }, new ApiResponse(201, result: celenderEventsDto));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> Update(
            int id,
            [Required] CreateCelenderEventDTO eventDTO)
        {
            if (eventDTO.EndAt < eventDTO.StartAt)
            {
                return BadRequest(new ApiResponse(400, "End date must be later than start date"));
            }

            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return BadRequest(new ApiResponse(400, "User ID is required"));
            }

            var existingEvent = await unitOfWork.celenderRepository.GetById(id);

            if (existingEvent == null)
            {
                return NotFound(new ApiResponse(404, "Event not found"));
            }

            if (existingEvent.AuthorId != userId)
            {
                return BadRequest(new ApiResponse(400, "You are not authorized to modify this event"));
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

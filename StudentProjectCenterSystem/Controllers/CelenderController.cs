using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenter.Core.Entities.Domain.workgroup;
using StudentProjectsCenter.Core.Entities.DTO.Workgroup;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Security.Claims;

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
        private readonly IEmailService emailService;

        public CelenderController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<LocalUser> userManager,
            IEmailService emailService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
            this.emailService = emailService;
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

            var workgroup = await unitOfWork.workgroupRepository.GetById(workgroupId, "Project.UserProjects.User");
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

            var members = workgroup.Project?.UserProjects?
                .Where(u => !u.IsDeleted && u.User.EmailConfirmed && !string.IsNullOrWhiteSpace(u.User.Email))
                .Select(u => u.User)
                .ToList();

            if (members != null && members.Any())
            {
                foreach (var user in members)
                {
                    try
                    {
                        string emailContent = $@"
                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; 
                                        border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9;'>
                                <h2 style='color: #333; text-align: center;'>New Event Notification</h2>
                                <p style='font-size: 16px; color: #555;'>Hello <strong>{user.FirstName}</strong>,</p>
                                <p style='font-size: 16px; color: #555;'>
                                    A new event titled <strong>'{eventDTO.Title}'</strong> has been added by 
                                    <strong>{celenderEventsDto.Author}</strong>.
                                </p>
                                <p style='text-align: center;'>
                                    <a href='http://localhost:5173/workgroups/{workgroupId}' 
                                        style='display: inline-block; padding: 12px 20px; background-color: #007bff; 
                                        color: #fff; text-decoration: none; font-size: 16px; border-radius: 5px;'>
                                        View Event Details
                                    </a>
                                </p>
                                <p style='font-size: 14px; color: #777; text-align: center; margin-top: 20px;'>
                                    If you have any questions, please contact support.
                                </p>
                            </div>";

                        var emailSent = await emailService.SendEmailAsync(user.Email ?? "", "New Event Added", emailContent, true);

                        if (!emailSent.IsSuccess)
                        {
                            // Log the error instead of throwing an exception
                            Console.WriteLine($"Error sending email to {user.Email}: {emailSent.ErrorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception while sending email to {user.Email}: {ex.Message}");
                    }
                }
            }

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
                return Forbid(); 
            }

            // Update event details
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

            /* notification
            var members = workgroup.Project?.UserProjects
                .Where(u => u.User.EmailConfirmed && !u.IsDeleted && !string.IsNullOrWhiteSpace(u.User.Email))
                .Select(u => u.User)
                .ToList();

            if (members != null && members.Any())
            {
                string emailContent = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; 
                                    border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9;'>
                            <h2 style='color: #0275d8; text-align: center;'>Event Updated</h2>
                            <p style='font-size: 16px; color: #555;'>Hello,</p>
                            <p style='font-size: 16px; color: #555;'>
                                The event <strong>'{eventDTO.Title}'</strong> has been edited by 
                                <strong>{User?.Identity?.Name ?? "an unknown user"}</strong>.
                            </p>
                            <p style='text-align: center; margin-top: 20px;'>
                                <a href='http://localhost:5173/workgroups/{workgroup.Id}/calendar' 
                                    style='display: inline-block; padding: 12px 20px; background-color: #007bff; 
                                    color: #fff; text-decoration: none; font-size: 16px; border-radius: 5px;'>
                                    View Updated Event
                                </a>
                            </p>
                            <p style='font-size: 14px; color: #777; text-align: center; margin-top: 20px;'>
                                If you have any questions, please contact support.
                            </p>
                        </div>";

                foreach (var user in members)
                {
                    try
                    {
                        var emailSent = await emailService.SendEmailAsync(user.Email ?? "", "Event Updated Notification", emailContent, true);
                        if (!emailSent.IsSuccess)
                        {
                            Console.WriteLine($"Error sending email to {user.Email}: {emailSent.ErrorMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception while sending email to {user.Email}: {ex.Message}");
                    }
                }
            }
            */

            return Ok(new ApiResponse(200, "Event updated successfully.", result: existingEvent));
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> Delete(int id)
        {
            var existingEvent = await unitOfWork.celenderRepository.GetById(id, "Workgroup.Project.UserProjects.User");

            int successDelete = unitOfWork.celenderRepository.Delete(id);
            if (successDelete == 0)
            {
                return NotFound(new ApiResponse(404));
            }

            string emailContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; 
                            border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9;'>
                    <h2 style='color: #d9534f; text-align: center;'>Event Deleted</h2>
                    <p style='font-size: 16px; color: #555;'>Hello,</p>
                    <p style='font-size: 16px; color: #555;'>
                        The event <strong>'{existingEvent.Title}'</strong> has been deleted by 
                        <strong>{User?.Identity?.Name ?? "an unknown user"}</strong>.
                    </p>
                    <p style='font-size: 16px; color: #555;'>If this was a mistake, please contact the administrator.</p>
                    <p style='text-align: center; margin-top: 20px;'>
                        <a href='http://localhost:5173/workgroups/{existingEvent.WorkgroupId}/calendar' 
                            style='display: inline-block; padding: 12px 20px; background-color: #dc3545; 
                            color: #fff; text-decoration: none; font-size: 16px; border-radius: 5px;'>
                            View Workgroup Calendar
                        </a>
                    </p>
                    <p style='font-size: 14px; color: #777; text-align: center; margin-top: 20px;'>
                        If you have any questions, please contact support.
                    </p>
                </div>";

            int successSave = await unitOfWork.save();
            if (successSave == 0)
            {
                return StatusCode(500, new ApiResponse(500, "Deleted failed!"));
            }

            var membersEmail = existingEvent.Workgroup?.Project?.UserProjects
                .Where(u => !u.IsDeleted && u.User.EmailConfirmed && !string.IsNullOrWhiteSpace(u.User.Email))
                .Select(u => u.User.Email)
                .ToList();

            if (membersEmail != null && membersEmail.Any())
            {
                foreach(var email in membersEmail) 
                {
                    var emailSent = await emailService.SendEmailAsync(email ?? "", "Event Deleted", emailContent, true);

                    if (!emailSent.IsSuccess)
                    {
                        throw new Exception($"Error sending email to {email}: {emailSent.ErrorMessage}");
                    }
                };
            }

            return Ok(new ApiResponse(200, "Deleted Successfully"));
        }
    }
}

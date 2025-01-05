using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenter.Core.Entities.Domain;
using StudentProjectsCenter.Core.Entities.DTO.Message;
using StudentProjectsCenter.Core.Entities.DTO.Messages;
using StudentProjectsCenter.Core.IRepositories;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Security.Claims;

namespace StudentProjectsCenter.Controllers
{
    [Authorize]
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public ChatController(IChatService chatService, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _chatService = chatService;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        // Endpoint to get old messages
        [HttpGet("{workgroupId}/get-messages")]
        public async Task<IActionResult> GetMessages(int workgroupId)
        {
            Expression<Func<Message, bool>> filter = m => m.WorkgroupId == workgroupId;
            var messages = await unitOfWork.messageRepository.GetAll(filter, 30, 1, "Workgroup");

            var userName = User?.Identity?.Name;
            messages = messages.OrderBy(m => m.SentAt).ToList();

            var messageList = messages.Select(m => new MessageDTO()
            {
                WorkgroupName = m?.Workgroup?.Name ?? "",
                User = m?.User ?? "",
                Content = m?.Content ?? "",
                SentAt = m?.SentAt ?? new DateTime(),
                IsUserMessage = (m?.User == userName)
            }).ToList();

            //var messageList = mapper.Map<List<MessageDTO>>(messages);
            //foreach (var message in messageList)
            //{
            //    message.IsUserMessage = (message.User == userName);
            //}

            return Ok(messageList);
        }


        // POST: api/chat/send
        [HttpPost("{workgroupId}/send")]
        public async Task<IActionResult> SendMessage(int workgroupId, [FromBody, Required] SendMessageDTO message)
        {
            var workgroup = await unitOfWork.workgroupRepository.GetById(workgroupId);
            if (workgroup == null)
            {
                return NotFound("Workgroup cannot be found.");
            }

            if (string.IsNullOrWhiteSpace(workgroup.Name))
            {
                return BadRequest("Workgroup name cannot be empty.");
            }

            if (message == null || string.IsNullOrWhiteSpace(message.Message))
            {
                return BadRequest("Message details cannot be null or empty.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User ID is required.");
            }

            var userName = User?.Identity?.Name ?? "Anonymous"; //edit to allow authorize user alone
            await _chatService.SendMessageAsync(message.Message, workgroupId, workgroup.Name, userName);

            return Ok(new { Status = "Message sent successfully." });
        }

        // POST: api/chat/join
        [HttpPost("{workgroupId}/join")]
        public async Task<IActionResult> JoinGroup(int workgroupId)
        {
            var workgroup = await unitOfWork.workgroupRepository.GetById(workgroupId);
            if(workgroup == null)
            {
                return NotFound("Workgroup cannot be found.");
            }

            if (string.IsNullOrWhiteSpace(workgroup.Name))
            {
                return BadRequest("Workgroup name cannot be empty.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User ID is required.");
            }

            await _chatService.AddUserToWorkgroupAsync(workgroup.Name, userId);
            return Ok(new { Status = $"User '{userId}' joined workgroup '{workgroup.Name}' successfully." });
        }

        // POST: api/chat/leave
        [HttpPost("{workgroupId}/leave")]
        public async Task<IActionResult> LeaveGroup(int workgroupId)
        {
            var workgroup = await unitOfWork.workgroupRepository.GetById(workgroupId);
            if (workgroup == null)
            {
                return NotFound("Workgroup cannot be found.");
            }

            if (string.IsNullOrWhiteSpace(workgroup.Name))
            {
                return BadRequest("Workgroup name cannot be empty.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User ID is required.");
            }

            await _chatService.RemoveUserFromWorkgroupAsync(workgroup.Name, userId);
            return Ok(new { Status = $"User '{userId}' left workgroup '{workgroup.Name}' successfully." });
        }


    }
}

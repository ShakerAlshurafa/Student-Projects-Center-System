using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenter.Core.Entities.Domain;
using StudentProjectsCenter.Core.Entities.DTO.Message;
using StudentProjectsCenter.Core.Entities.DTO.Messages;
using StudentProjectsCenter.Core.IRepositories;
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
        [HttpGet("get-messages")]
        public async Task<IActionResult> GetMessages([FromQuery, Required] string workgroupName)
        {
            Expression<Func<Message, bool>> filter = m => m.WorkgroupName == workgroupName;
            var messages = await unitOfWork.messageRepository.GetAll(filter, 100, 1);

            var userName = User?.Identity?.Name;
            messages = messages.OrderBy(m => m.SentAt).ToList();

            var messageList = mapper.Map<List<MessageDTO>>(messages);
            foreach (var message in messageList)
            {
                message.IsUserMessage = (message.User == userName);
            }

            return Ok(messageList);
        }


        // POST: api/chat/send
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDTO message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.WorkgroupName) || string.IsNullOrWhiteSpace(message.Message))
            {
                return BadRequest("Message details cannot be null or empty.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User ID is required.");
            }

            var userName = User?.Identity?.Name ?? "Anonymous";
            await _chatService.SendMessageAsync(message, userName);

            return Ok(new { Status = "Message sent successfully." });
        }

        // POST: api/chat/join
        [HttpPost("join")]
        public async Task<IActionResult> JoinGroup([FromBody, Required] string workgroupName)
        {
            if (string.IsNullOrWhiteSpace(workgroupName))
            {
                return BadRequest("Workgroup name cannot be empty.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User ID is required.");
            }

            await _chatService.AddUserToWorkgroupAsync(workgroupName, userId);
            return Ok(new { Status = $"User '{userId}' joined workgroup '{workgroupName}' successfully." });
        }

        // POST: api/chat/leave
        [HttpPost("leave")]
        public async Task<IActionResult> LeaveGroup([FromBody, Required] string workgroupName)
        {
            if (string.IsNullOrWhiteSpace(workgroupName))
            {
                return BadRequest("Workgroup name cannot be empty.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User ID is required.");
            }

            await _chatService.RemoveUserFromWorkgroupAsync(workgroupName, userId);
            return Ok(new { Status = $"User '{userId}' left workgroup '{workgroupName}' successfully." });
        }


    }
}

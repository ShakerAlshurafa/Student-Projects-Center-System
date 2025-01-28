using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StudentProjectsCenter.Core.Entities.Domain;
using StudentProjectsCenter.Core.Entities.DTO.Message;
using StudentProjectsCenter.Core.Entities.DTO.Messages;
using StudentProjectsCenter.Core.IRepositories;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Repositories;
using StudentProjectsCenterSystem.Services.Hubs;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace StudentProjectsCenter.Controllers
{
    [Authorize]
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IChatService _chatService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        // In-memory store for groups
        private static readonly ConcurrentDictionary<string, string> _groups = new();

        public ChatController(IHubContext<ChatHub> hubContext, IChatService chatService, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _hubContext = hubContext;
            _chatService = chatService;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [HttpPost("create-group/{workgroupId}")]
        public async Task<ActionResult<ApiResponse>> CreateGroup(int workgroupId)
        {
            var workgroup = await unitOfWork.workgroupRepository.GetById(workgroupId);
            if (workgroup == null)
            {
                return NotFound("Workgroup not found.");
            }

            string groupId = $"Group-{workgroupId}";
            _groups.TryAdd(groupId, workgroup.Name);
            return Ok(new ApiResponse(200, result: new { GroupId = groupId }));
        }


        // Join an existing group
        [HttpPost("join-group/{workgroupId}")]
        public async Task<IActionResult> JoinGroup(int workgroupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User ID is required.");
            }

            var userName = User?.Identity?.Name ?? "Anonymous";

            string chatGroup = $"Group-{workgroupId}";

            // Validate if the group exists
            if (!_groups.ContainsKey(chatGroup))
            {
                return NotFound("Group not found.");
            }

            // Broadcast to the group that a new user has joined (optional)
            await _hubContext.Clients.Group(chatGroup)
                .SendAsync("ReceiveMessage", "System", $"{userName} has joined the group.");

            return Ok(new ApiResponse(200, $"Successfully joined group {chatGroup}."));
        }


        // Endpoint to send a message to a group
        [HttpPost("send-message/{workgroupId}")]
        public async Task<IActionResult> SendMessage(int workgroupId, [Required, FromBody] string message)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("User ID is required.");
            }

            var userName = User?.Identity?.Name ?? "Anonymous"; //edit to allow authorize user alone

            // Broadcast the message to the group via SignalR Hub
            await _hubContext.Clients.Group($"Group-{workgroupId}").SendAsync("ReceiveMessage", userName, message);

            // Save message to the database
            var newMessage = new Message
            {
                WorkgroupId = workgroupId,
                User = userName,
                Content = message,
                SentAt = DateTime.UtcNow
            };

            await unitOfWork.messageRepository.Create(newMessage);
            await unitOfWork.save();

            return Ok(new ApiResponse(200, "Message sent to group."));
        }



        // Endpoint to get old messages
        [HttpGet("{workgroupId}/get-messages")]
        public async Task<IActionResult> GetMessages(int workgroupId, int page = 1, int pageSize = 30)
        {
            Expression<Func<Message, bool>> filter = m => m.WorkgroupId == workgroupId;
            var messages = await unitOfWork.messageRepository.GetAll(filter, pageSize, page, "Workgroup");

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

            return Ok(new ApiResponse(200, result: messageList));
        }


    }
}

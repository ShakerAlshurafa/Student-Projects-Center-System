using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentProjectsCenter.Core.Entities.Domain;
using StudentProjectsCenter.Core.Entities.DTO.Message;
using StudentProjectsCenter.Core.Entities.DTO.Messages;
using StudentProjectsCenter.Core.IRepositories;
using StudentProjectsCenterSystem.Core.IRepositories;
using StudentProjectsCenterSystem.Infrastructure.Repositories;
using StudentProjectsCenterSystem.Services.Hubs;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace StudentProjectsCenter.Controllers
{
    [Authorize]
    [Route("api/chat")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IChatService chatService;
        private readonly IMapper mapper;

        public MessagesController(IUnitOfWork unitOfWork, IChatService chatService, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.unitOfWork = unitOfWork;
            this.chatService = chatService;
            this.mapper = mapper;
        }


        // Endpoint to get old messages
        [HttpGet("getmessages")]
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


        [HttpPost("sendmessage")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDTO request)
        {
            var userName = User?.Identity?.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("User is not authenticated.");
            }

            var message = new SendMessageDTO()
            {
                Message = request.Message,
                WorkgroupName = request.WorkgroupName
            };
            await chatService.SendMessageAsync(message, userName);
            return Ok();
        }

        [HttpPost("joingroup")]
        public async Task<IActionResult> JoinGroup([FromBody] string workgroupName)
        {
            await chatService.JoinGroupAsync(workgroupName, HttpContext.Connection.Id);
            return Ok();
        }


        [HttpPost("leavegroup")]
        public async Task<IActionResult> LeaveGroup([FromBody] string workgroupName)
        {
            await chatService.LeaveGroupAsync(workgroupName, HttpContext.Connection.Id);
            return Ok();
        }
    }
}

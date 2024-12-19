using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using StudentProjectsCenter.Core.Entities.Domain;
using StudentProjectsCenter.Core.Entities.DTO.Messages;
using StudentProjectsCenter.Core.IRepositories;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.Linq.Expressions;

namespace StudentProjectsCenterSystem.Services.Hubs
{
    public class ChatService : IChatService
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IUnitOfWork unitOfWork;

        public ChatService(IHubContext<ChatHub> hubContext, IUnitOfWork unitOfWork)
        {
            _hubContext = hubContext;
            this.unitOfWork = unitOfWork;
        }


        // Send a message to the workgroup
        public async Task SendMessageAsync(SendMessageDTO message, string user)
        {
            var newMessage = new Message
            {
                WorkgroupName = message.WorkgroupName,
                User = user,
                Content = message.Message,
                SentAt = DateTime.UtcNow,
            };

            await unitOfWork.messageRepository.Create(newMessage);
            await unitOfWork.save();

            await _hubContext.Clients.Group(message.WorkgroupName).SendAsync("ReceiveMessage", user, message.Message);
        }

        // Add a user to the workgroup
        public async Task JoinGroupAsync(string workgroupName, string connectionId)
        {
            await _hubContext.Groups.AddToGroupAsync(connectionId, workgroupName);
            await _hubContext.Clients.Group(workgroupName).SendAsync("UserJoined", $"{connectionId} has joined the group.");
        }

        // Remove a user from the workgroup
        public async Task LeaveGroupAsync(string workgroupName, string connectionId)
        {
            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, workgroupName);
            await _hubContext.Clients.Group(workgroupName).SendAsync("UserLeft", $"{connectionId} has left the group.");
        }
    }
}

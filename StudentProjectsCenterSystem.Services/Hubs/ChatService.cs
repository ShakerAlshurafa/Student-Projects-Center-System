using Microsoft.AspNetCore.SignalR;
using StudentProjectsCenter.Core.Entities.Domain;
using StudentProjectsCenter.Core.Entities.DTO.Messages;
using StudentProjectsCenter.Core.IRepositories;
using StudentProjectsCenterSystem.Core.IRepositories;
using System.Collections.Concurrent;

namespace StudentProjectsCenterSystem.Services.Hubs
{
    public class ChatService : IChatService
    {
        // Dictionary to track user connections
        private static readonly ConcurrentDictionary<string, string> _userConnections = new ConcurrentDictionary<string, string>();
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IUnitOfWork _unitOfWork;

        public ChatService(IHubContext<ChatHub> hubContext, IUnitOfWork unitOfWork)
        {
            _hubContext = hubContext;
            _unitOfWork = unitOfWork;
        }

        public async Task SendMessageAsync(SendMessageDTO message, string user)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.WorkgroupName) || string.IsNullOrWhiteSpace(message.Message))
            {
                throw new ArgumentException("Message details cannot be null or empty.");
            }

            // Save message to the database
            var newMessage = new Message
            {
                WorkgroupName = message.WorkgroupName,
                User = user,
                Content = message.Message,
                SentAt = DateTime.UtcNow
            };

            await _unitOfWork.messageRepository.Create(newMessage);
            await _unitOfWork.save();

            // Notify all users in the workgroup
            await _hubContext.Clients.Group(message.WorkgroupName).SendAsync("ReceiveMessage", user, newMessage.Content);
        }

        public async Task AddUserToWorkgroupAsync(string workgroupName, string userId)
        {
            if (string.IsNullOrWhiteSpace(workgroupName) || string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Workgroup name and user ID cannot be empty.");
            }

            var connectionId = GetConnectionIdByUserId(userId);
            if (string.IsNullOrEmpty(connectionId))
            {
                throw new InvalidOperationException("User is not connected.");
            }

            await _hubContext.Groups.AddToGroupAsync(connectionId, workgroupName);
        }

        public async Task RemoveUserFromWorkgroupAsync(string workgroupName, string userId)
        {
            if (string.IsNullOrWhiteSpace(workgroupName) || string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("Workgroup name and user ID cannot be empty.");
            }

            var connectionId = GetConnectionIdByUserId(userId);
            if (string.IsNullOrEmpty(connectionId))
            {
                throw new InvalidOperationException("User is not connected.");
            }

            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, workgroupName);
        }

        public string GetConnectionIdByUserId(string userId)
        {
            _userConnections.TryGetValue(userId, out var connectionId);
            return connectionId ?? "";
        }
    }
}

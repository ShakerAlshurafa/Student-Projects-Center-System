using StudentProjectsCenter.Core.Entities.Domain;
using StudentProjectsCenter.Core.Entities.DTO.Messages;
using StudentProjectsCenterSystem.Core.Entities;

namespace StudentProjectsCenter.Core.IRepositories
{
    public interface IChatService
    {
        public Task SendMessageAsync(SendMessageDTO message, string user);
        public Task AddUserToWorkgroupAsync(string workgroupName, string userId);
        public Task RemoveUserFromWorkgroupAsync(string workgroupName, string userId);
        public string GetConnectionIdByUserId(string userId);
    }
}

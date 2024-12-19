using StudentProjectsCenter.Core.Entities.Domain;
using StudentProjectsCenter.Core.Entities.DTO.Messages;

namespace StudentProjectsCenter.Core.IRepositories
{
    public interface IChatService
    {
        public Task SendMessageAsync(SendMessageDTO message, string user);
        public Task JoinGroupAsync(string workgroupName, string connectionId);
        public Task LeaveGroupAsync(string workgroupName, string connectionId);
    }
}

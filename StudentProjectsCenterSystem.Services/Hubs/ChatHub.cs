using Microsoft.AspNetCore.SignalR;

namespace StudentProjectsCenterSystem.Services.Hubs
{
    public class ChatHub : Hub
    {
        // Send a message to a specific workgroup
        public async Task SendMessage(string workgroupName, string user, string message)
        {
            await Clients.Group(workgroupName).SendAsync("ReceiveMessage", user, message, false); // false means this is another user's message
        }

        // Add the user to a specific workgroup
        public async Task JoinGroup(string workgroupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, workgroupName);
            await Clients.Group(workgroupName).SendAsync("UserJoined", $"{Context.ConnectionId} has joined the group.");
        }

        // Remove the user from a specific workgroup
        public async Task LeaveGroup(string workgroupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, workgroupName);
            await Clients.Group(workgroupName).SendAsync("UserLeft", $"{Context.ConnectionId} has left the group.");
        }
    }
}

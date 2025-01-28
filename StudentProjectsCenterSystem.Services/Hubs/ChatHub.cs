using Microsoft.AspNetCore.SignalR;

namespace StudentProjectsCenterSystem.Services.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} has joined the group.");
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Error joining group: {ex.Message}");
            }
        }

        public async Task LeaveGroup(string groupName)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} has left the group.");
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Error leaving group: {ex.Message}");
            }
        }

        public async Task SendMessageToGroup(string groupName, string user, string message)
        {
            try
            {
                await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Error sending message: {ex.Message}");
            }
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}

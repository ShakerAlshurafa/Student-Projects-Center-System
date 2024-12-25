using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace StudentProjectsCenterSystem.Services.Hubs
{
    public class ChatHub : Hub
    {
        // Concurrent dictionary to manage workgroup memberships
        private static readonly ConcurrentDictionary<string, HashSet<string>> WorkgroupMembers = new();

        public override async Task OnConnectedAsync()
        {
            var userId = Context?.User?.Identity?.Name ?? throw new HubException("User identity is not set.");
            await Clients.Caller.SendAsync("Connected", $"Welcome, {userId}!");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context?.User?.Identity?.Name;
            if (userId != null)
            {
                foreach (var workgroup in WorkgroupMembers.Where(wg => wg.Value.Contains(userId)).ToList())
                {
                    workgroup.Value.Remove(userId);
                    await Clients.Group(workgroup.Key).SendAsync("UserDisconnected", $"{userId} has left the workgroup.");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinWorkgroup(string workgroupName)
        {
            var userId = Context?.User?.Identity?.Name ?? throw new HubException("User identity is not set.");

            if (string.IsNullOrWhiteSpace(workgroupName))
            {
                throw new HubException("Workgroup name cannot be empty.");
            }

            // Add user to the workgroup
            var members = WorkgroupMembers.GetOrAdd(workgroupName, _ => new HashSet<string>());
            lock (members)
            {
                members.Add(userId);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, workgroupName);
            await Clients.Group(workgroupName).SendAsync("UserJoined", $"{userId} has joined the workgroup.");
        }

        public async Task LeaveWorkgroup(string workgroupName)
        {
            var userId = Context?.User?.Identity?.Name ?? throw new HubException("User identity is not set.");

            if (string.IsNullOrWhiteSpace(workgroupName))
            {
                throw new HubException("Workgroup name cannot be empty.");
            }

            if (WorkgroupMembers.TryGetValue(workgroupName, out var members))
            {
                lock (members)
                {
                    members.Remove(userId);
                }
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, workgroupName);
            await Clients.Group(workgroupName).SendAsync("UserLeft", $"{userId} has left the workgroup.");
        }

        public async Task SendMessageToWorkgroup(string workgroupName, string message)
        {
            var userId = Context?.User?.Identity?.Name ?? throw new HubException("User identity is not set.");

            if (string.IsNullOrWhiteSpace(workgroupName) || string.IsNullOrWhiteSpace(message))
            {
                throw new HubException("Workgroup name and message cannot be empty.");
            }

            if (!WorkgroupMembers.TryGetValue(workgroupName, out var members) || !members.Contains(userId))
            {
                throw new HubException("You are not a member of this workgroup.");
            }

            await Clients.Group(workgroupName).SendAsync("ReceiveMessage", userId, message);
        }
    }
}

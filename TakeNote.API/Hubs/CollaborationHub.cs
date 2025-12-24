using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace TakeNote.API.Hubs
{
    // [Authorize] KALDIRILDI - Token zaten query string'den geliyor
    public class CollaborationHub : Hub
    {
        // Kullanıcı bağlandığında
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Bilinmeyen";

            Console.WriteLine($"✅ {username} (ID: {userId}) bağlandı. ConnectionId: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        // Kullanıcı ayrıldığında
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Bilinmeyen";
            Console.WriteLine($"❌ {username} ayrıldı. ConnectionId: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        // Workspace grubuna katıl
        public async Task JoinWorkspaceGroup(string workspaceId)
        {
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Bilinmeyen";
            var groupName = $"workspace_{workspaceId}";

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // Diğer üyelere bildir
            await Clients.OthersInGroup(groupName).SendAsync(
                "UserJoinedWorkspace",
                username,
                workspaceId
            );

            Console.WriteLine($"👥 {username} -> {groupName} grubuna katıldı");
        }

        // Workspace grubundan ayrıl
        public async Task LeaveWorkspaceGroup(string workspaceId)
        {
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Bilinmeyen";
            var groupName = $"workspace_{workspaceId}";

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            // Diğer üyelere bildir
            await Clients.OthersInGroup(groupName).SendAsync(
                "UserLeftWorkspace",
                username,
                workspaceId
            );

            Console.WriteLine($"👋 {username} -> {groupName} grubundan ayrıldı");
        }

        // Not grubuna katıl
        public async Task JoinNoteGroup(string noteId)
        {
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Bilinmeyen";
            await Groups.AddToGroupAsync(Context.ConnectionId, $"note_{noteId}");
            Console.WriteLine($"📝 {username} -> note_{noteId} grubuna katıldı");
        }

        // Not grubundan ayrıl
        public async Task LeaveNoteGroup(string noteId)
        {
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Bilinmeyen";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"note_{noteId}");
            Console.WriteLine($"📝 {username} -> note_{noteId} grubundan ayrıldı");
        }

        // Yazma göstergesi gönder
        public async Task SendTypingIndicator(string noteId)
        {
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Bilinmeyen";
            await Clients.OthersInGroup($"note_{noteId}").SendAsync("UserTyping", username, noteId);
        }

        // Görev tamamlandı bildirimi
        public async Task NotifyTaskCompleted(string workspaceId, string taskTitle)
        {
            var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Bilinmeyen";
            await Clients.Group($"workspace_{workspaceId}").SendAsync(
                "TaskCompleted",
                taskTitle,
                username
            );
        }
    }
}
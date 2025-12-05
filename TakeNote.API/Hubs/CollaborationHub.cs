using Microsoft.AspNetCore.SignalR;

namespace TakeNote.API.Hubs
{
    public class CollaborationHub : Hub
    {
        // 1. Not Odası (Mevcut) - Notu düzenlerken kullanılır
        public async Task JoinNoteGroup(string noteId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"note_{noteId}");
        }

        public async Task LeaveNoteGroup(string noteId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"note_{noteId}");
        }

        // 2. Workspace Odası (YENİ) - Not listesini izlerken kullanılır
        public async Task JoinWorkspaceGroup(string workspaceId)
        {
            // İstemci (Frontend) bir çalışma alanını açtığında buraya abone olacak
            await Groups.AddToGroupAsync(Context.ConnectionId, $"workspace_{workspaceId}");
        }

        public async Task LeaveWorkspaceGroup(string workspaceId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"workspace_{workspaceId}");
        }
    }
}
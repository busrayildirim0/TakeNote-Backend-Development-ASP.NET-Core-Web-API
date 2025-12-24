using Microsoft.AspNetCore.SignalR;
using TakeNote.API.Hubs;
using TakeNote.Service.DTOs;
using TakeNote.Service.Interfaces;

namespace TakeNote.API.Services
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<CollaborationHub> _hubContext;

        // Burada artık kendi CollaborationHub'ını özgürce kullanabilirsin!
        public SignalRNotificationService(IHubContext<CollaborationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // --- NOT BİLDİRİMLERİ ---
        public async Task NotifyNoteCreatedAsync(int workspaceId, NoteDto note)
        {
            // Workspace odasındaki herkese "Yeni not var" de
            await _hubContext.Clients.Group($"workspace_{workspaceId}")
                .SendAsync("ReceiveNewNote", note);
        }

        public async Task NotifyNoteUpdatedAsync(int noteId, int idPayload, NoteUpdateDto dto)
        {
            // Not odasındaki (o notu açık tutan) herkese haber ver
            await _hubContext.Clients.Group($"note_{noteId}")
                .SendAsync("ReceiveNoteUpdate", idPayload, dto);
        }

        public async Task NotifyNoteDeletedAsync(int noteId)
        {
            await _hubContext.Clients.Group($"note_{noteId}")
                .SendAsync("ReceiveNoteDelete", noteId);
        }

        // --- GÖREV (TASK) BİLDİRİMLERİ ---
        public async Task NotifyTaskAddedAsync(int noteId, TaskItemDto task)
        {
            // Notu izleyenlere "Yeni görev eklendi" de
            await _hubContext.Clients.Group($"note_{noteId}")
                .SendAsync("ReceiveNewTask", task);
        }

        public async Task NotifyTaskUpdatedAsync(int noteId, TaskItemDto task)
        {
            await _hubContext.Clients.Group($"note_{noteId}")
                .SendAsync("ReceiveTaskUpdate", task);
        }

        public async Task NotifyTaskDeletedAsync(int noteId, int taskId)
        {
            await _hubContext.Clients.Group($"note_{noteId}")
                .SendAsync("ReceiveTaskDelete", taskId);
        }

        // --- WORKSPACE BİLDİRİMLERİ ---
        public async Task NotifyMemberAddedAsync(int workspaceId, Guid userId)
        {
            await _hubContext.Clients.Group($"workspace_{workspaceId}")
                .SendAsync("ReceiveMemberAdded", userId);
        }
    }
}